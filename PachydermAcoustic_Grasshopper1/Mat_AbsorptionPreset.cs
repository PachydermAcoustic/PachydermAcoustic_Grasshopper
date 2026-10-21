//'Pachyderm-Acoustic: Geometrical Acoustics for Rhinoceros (GPL)   
//' 
//'This file is part of Pachyderm-Acoustic. 
//' 
//'Copyright (c) 2008-2026, Open Research in Acoustical Science and Education, Inc. - a 501(c)3 nonprofit 
//'Pachyderm-Acoustic is free software; you can redistribute it and/or modify 
//'it under the terms of the GNU General Public License as published 
//'by the Free Software Foundation; either version 3 of the License, or 
//'(at your option) any later version. 
//'Pachyderm-Acoustic is distributed in the hope that it will be useful, 
//'but WITHOUT ANY WARRANTY; without even the implied warranty of 
//'MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
//'GNU General Public License for more details. 
//' 
//'You should have received a copy of the GNU General Public 
//'License along with Pachyderm-Acoustic; if not, write to the Free Software 
//'Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA. 

using System;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using Grasshopper.Kernel;
using Pachyderm_Acoustic;

namespace PachydermGH
{
    public class Mat_AbsorptionPreset : GH_Component
    {
        private Acoustics_Library library;
        private string libraryPath;
        private string[] names;
        private string selectedName = "";
        private bool wasReload;
        private bool attachValueList = true;
        private bool attachmentScheduled;

        public Mat_AbsorptionPreset()
            : base("Absorption Preset", "Abs Lib", "Select octave absorption from the main Pachyderm layer assigner's library.", "Acoustics", "Materials")
        {
        }

        public override void AddedToDocument(GH_Document document)
        {
            base.AddedToDocument(document);
            ScheduleAttachments(document);
        }

        private void ScheduleAttachments(GH_Document document)
        {
            if (!attachValueList || attachmentScheduled || document == null) return;
            attachmentScheduled = true;
            document.ScheduleSolution(1, doc =>
            {
                attachmentScheduled = false;
                if (OnPingDocument() != doc) return;
                if (Params.Input[0].SourceCount != 0) { attachValueList = false; return; }
                try
                {
                    LoadLibrary(false);
                    if (names.Length == 0) return;
                    var list = new Grasshopper.Kernel.Special.GH_ValueList();
                    list.ListMode = Grasshopper.Kernel.Special.GH_ValueListMode.DropDown;
                    list.ListItems.Clear();
                    int selected = Array.FindIndex(names, name => string.Equals(name, selectedName, StringComparison.OrdinalIgnoreCase));
                    if (selected < 0) selected = 0;
                    for (int i = 0; i < names.Length; i++)
                    {
                        // GH1 expression strings escape embedded quotes by doubling them.
                        var item = new Grasshopper.Kernel.Special.GH_ValueListItem(names[i], "\"" + names[i].Replace("\"", "\"\"") + "\"");
                        item.Selected = i == selected;
                        list.ListItems.Add(item);
                    }
                    list.CreateAttributes();
                    list.Attributes.PerformLayout();
                    list.Attributes.Pivot = new System.Drawing.PointF(Attributes.Pivot.X - list.Attributes.Bounds.Width - 50, Attributes.Pivot.Y);
                    doc.UndoUtil.RecordAddObjectEvent("Add material Value List", list);
                    doc.AddObject(list, false);
                    Params.Input[0].AddSource(list);
                    attachValueList = false;
                    ExpireSolution(false);
                }
                catch (Exception error) { AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Could not attach the material Value List: " + error.Message); }
            });
        }
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Selection", "S", "Exact preset name or zero-based index. Leave empty to use the preset selected in the component menu.", GH_ParamAccess.item, "");
            pManager.AddBooleanParameter("Reload", "R", "Toggle false to true to reload the configured library.", GH_ParamAccess.item, false);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddIntegerParameter("Absorption", "ABS", "Eight whole-percent octave absorption coefficients, 63 Hz to 8 kHz. Connect directly to Layer Material Properties ABS.", GH_ParamAccess.list);
        }

        private void LoadLibrary(bool reload)
        {
            string path = Pach_Properties.Instance.Lib_Path;
            if (library != null && names != null && names.Length > 0 && !reload && string.Equals(path, libraryPath, StringComparison.OrdinalIgnoreCase)) return;
            var loaded = new Acoustics_Library();
            string[] loadedNames = loaded.Names_Abs().ToArray();
            library = loaded;
            names = loadedNames;
            libraryPath = path;
        }

        protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu)
        {
            base.AppendAdditionalComponentMenuItems(menu);
            try
            {
                LoadLibrary(false);
                var presets = Menu_AppendItem(menu, "Preset");
                foreach (string name in names)
                {
                    string choice = name;
                    Menu_AppendItem(presets.DropDown, choice, (sender, args) =>
                    {
                        RecordUndoEvent("Select absorption preset");
                        selectedName = choice;
                        ExpireSolution(true);
                    }, true, string.Equals(selectedName, choice, StringComparison.OrdinalIgnoreCase));
                }
            }
            catch (Exception error) { Menu_AppendItem(menu, error.Message).Enabled = false; }
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string selection = "";
            bool reload = false;
            if (!DA.GetData(0, ref selection) || !DA.GetData(1, ref reload)) return;
            try
            {
                LoadLibrary(reload && !wasReload);
                wasReload = reload;
                if (names.Length > 0) ScheduleAttachments(OnPingDocument());
                if (names.Length == 0) throw new ArgumentException("No absorption presets were loaded. Check Pachyderm's library directory.");
                if (string.IsNullOrWhiteSpace(selection)) selection = selectedName;
                if (string.IsNullOrWhiteSpace(selection)) { Message = "Select a preset"; return; }
                int index = Array.FindIndex(names, name => string.Equals(name, selection, StringComparison.OrdinalIgnoreCase));
                if (index < 0 && (!int.TryParse(selection, NumberStyles.Integer, CultureInfo.InvariantCulture, out index) || index < 0 || index >= names.Length))
                    throw new ArgumentException("Select an existing absorption preset by name or zero-based index.");
                double[] percent = library.Abs_List[index].Values;
                if (percent == null || percent.Length != 8 || percent.Any(value => double.IsNaN(value) || double.IsInfinity(value)))
                    throw new ArgumentException("The selected preset must contain eight finite octave absorption coefficients.");
                int[] absorption = percent.Select(value => (int)Math.Round(value, MidpointRounding.ToEven)).ToArray();
                if (absorption.Any(value => value < 0 || value > 100))
                    throw new ArgumentException("The selected preset cannot be represented as absorption percentages from 0 to 100.");
                Message = names[index];
                DA.SetDataList(0, absorption);
            }
            catch (Exception error) { AddRuntimeMessage(GH_RuntimeMessageLevel.Error, error.Message); }
        }

        public override bool Write(GH_IO.Serialization.GH_IWriter writer)
        {
            writer.SetString("PresetName", selectedName);
            return base.Write(writer);
        }

        public override bool Read(GH_IO.Serialization.GH_IReader reader)
        {
            attachValueList = false;
            reader.TryGetString("PresetName", ref selectedName);
            return base.Read(reader);
        }

        protected override System.Drawing.Bitmap Icon
        {
            get { return Properties.Resources.Absorption_Preset; }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("5ecc9d46-c582-46d9-a668-f2a2720fe405"); }
        }
    }
}
