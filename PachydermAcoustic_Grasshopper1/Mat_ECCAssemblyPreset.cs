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
using Pachyderm_Acoustic.AbsorptionModels;

namespace PachydermGH
{
    public class Mat_ECCAssemblyPreset : GH_Component
    {
        private EHM_Data_Library library;
        private string libraryPath;
        private string[] names;
        private string selectedName = "";
        private bool wasReload;
        private bool attachComponents = true;
        private bool attachmentScheduled;

        public Mat_ECCAssemblyPreset()
            : base("ECC Assembly Preset", "ECC Lib", "Select an ECC assembly layer train for TMM.", "Acoustics", "Materials")
        {
        }

        public override void AddedToDocument(GH_Document document)
        {
            base.AddedToDocument(document);
            ScheduleAttachments(document);
        }

        private void ScheduleAttachments(GH_Document document)
        {
            if (!attachComponents || attachmentScheduled || document == null) return;
            attachmentScheduled = true;
            document.ScheduleSolution(1, doc =>
            {
                attachmentScheduled = false;
                if (OnPingDocument() != doc) return;
                try
                {
                    if (Params.Output[0].Recipients.Count == 0)
                    {
                        var tmm = new Mat_TMM();
                        tmm.CreateAttributes();
                        tmm.Attributes.Pivot = new System.Drawing.PointF(Attributes.Pivot.X + 240, Attributes.Pivot.Y);
                        doc.UndoUtil.RecordAddObjectEvent("Add ECC TMM", tmm);
                        doc.AddObject(tmm, false);
                        // Wire before TMM's deferred placement callback so it does not add a Merge.
                        tmm.Params.Input[0].AddSource(Params.Output[0]);
                        tmm.ExpireSolution(false);
                    }
                }
                catch (Exception error) { AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Could not attach the ECC TMM: " + error.Message); }
                try
                {
                    if (Params.Input[0].SourceCount != 0) { attachComponents = false; return; }
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
                    attachComponents = false;
                    ExpireSolution(false);
                }
                catch (Exception error) { AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Could not attach the ECC workflow: " + error.Message); }
            });
        }
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Selection", "S", "Exact assembly name or zero-based index. Leave empty to use the preset selected in the component menu.", GH_ParamAccess.item, "");
            pManager.AddBooleanParameter("Reload", "R", "Toggle false to true to reload the configured ECC library.", GH_ParamAccess.item, false);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Layers", "L", "ECC assembly layers in exposed-face-to-backing order, ready for TMM L.", GH_ParamAccess.list);
        }

        private void LoadLibrary(bool reload)
        {
            string path = Pach_Properties.Instance.Lib_Path;
            if (library != null && names != null && names.Length > 0 && !reload && string.Equals(path, libraryPath, StringComparison.OrdinalIgnoreCase)) return;
            var loaded = new EHM_Data_Library();
            string[] loadedNames = loaded.Names.Skip(loaded.ECC_Abs.Count).ToArray();
            if (loadedNames.Length != loaded.Substrates.Count)
                throw new ArgumentException("The ECC library returned mismatched assembly names and layer trains.");
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
                        RecordUndoEvent("Select ECC assembly");
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
                if (names.Length == 0) throw new ArgumentException("No ECC assemblies were loaded. Check Pachyderm's library directory.");
                if (string.IsNullOrWhiteSpace(selection)) selection = selectedName;
                if (string.IsNullOrWhiteSpace(selection)) { Message = "Select a preset"; return; }
                int index = Array.FindIndex(names, name => string.Equals(name, selection, StringComparison.OrdinalIgnoreCase));
                if (index < 0 && (!int.TryParse(selection, NumberStyles.Integer, CultureInfo.InvariantCulture, out index) || index < 0 || index >= names.Length))
                    throw new ArgumentException("Select an existing ECC assembly by name or zero-based index.");
                ABS_Layer[] layers = library.Substrates[index];
                if (layers.Length == 0 || layers.Any(layer => layer == null || !(layer.depth > 0) || double.IsInfinity(layer.depth)))
                    throw new ArgumentException("The selected assembly has missing or invalid layer thicknesses.");
                // The library is backing-to-face; the Grasshopper TMM input is face-to-backing.
                Message = names[index];
                DA.SetDataList(0, layers.Reverse());
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
            attachComponents = false;
            reader.TryGetString("PresetName", ref selectedName);
            return base.Read(reader);
        }

        protected override System.Drawing.Bitmap Icon
        {
            get { return Properties.Resources.ECC_Assembly_Preset; }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("2094304e-6ec4-4bd2-96ea-1e76bbb88a85"); }
        }
    }
}
