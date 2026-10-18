//'Pachyderm-Acoustic: Geometrical Acoustics for Rhinoceros (GPL)   
//' 
//'This file is part of Pachyderm-Acoustic. 
//' 
//'Copyright (c) 2008-2025, Open Research in Acoustical Science and Education, Inc. - a 501(c)3 nonprofit 
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
using Grasshopper2.Components;
using Grasshopper2.Parameters;
using Grasshopper2.UI;
using Grasshopper2.UI.Icon;
using GrasshopperIO;
using Pachyderm_Acoustic;
using Pachyderm_Acoustic.AbsorptionModels;

namespace PachydermGH
{
    [IoId("2094304e-6ec4-4bd2-96ea-1e76bbb88a85")]
    public class Mat_ECCAssemblyPreset : Component
    {
        private EHM_Data_Library library;
        private string libraryPath;
        private string[] names;
        private string selectedName = "";
        private bool wasReload;
        private bool attachComponents = true;
        private bool attachmentScheduled;

        public Mat_ECCAssemblyPreset()
            : base(new Nomen("ECC Assembly Preset", "Select an ECC assembly layer train for TMM.", "Acoustics", "Materials"))
        {
            Threading = ThreadingState.UiSingleThreaded;
            DocumentChanged += AttachComponents;
        }

        public Mat_ECCAssemblyPreset(IReader reader) : base(reader)
        {
            Threading = ThreadingState.UiSingleThreaded;
            attachComponents = false;
            if (reader.HasItem("PresetName")) selectedName = reader.String("PresetName");
        }

        public string SelectedPreset { get { return selectedName; } set { selectedName = value ?? ""; } }

        private void AttachComponents(object sender, Grasshopper2.Doc.ObjectDocumentEventArgs args)
        {
            if (args.NewDocument == null) return;
            DocumentChanged -= AttachComponents;
            ScheduleAttachments(args.NewDocument);
        }

        private void ScheduleAttachments(Grasshopper2.Doc.Document document)
        {
            if (!attachComponents || attachmentScheduled || document == null) return;
            attachmentScheduled = true;
            Eto.Forms.Application.Instance.AsyncInvoke(() =>
            {
                attachmentScheduled = false;
                if (Document != document || !document.Objects.Objects.Contains(this)) return;
                try
                {
                    if (Parameters.Output(0).Outputs.Count == 0)
                    {
                        var tmm = new Mat_TMM();
                        tmm.Attributes.Pivot = new Eto.Drawing.PointF(Attributes.Pivot.X + 240, Attributes.Pivot.Y);
                        if (document.Objects.Add(tmm))
                        {
                            var tmmActions = new Grasshopper2.Undo.ActionList(new Grasshopper2.Undo.Actions.CreateObjectAction(tmm));
                            // TMM's deferred callback sees this connection and does not add a Merge.
                            Connections.Connect(Parameters.Output(0), tmm.Parameters.Input(0), tmmActions);
                            document.Undo.Do(new Grasshopper2.Undo.VerbNoun("Add", "ECC TMM"), tmmActions);
                        }
                    }
                }
                catch (Exception error) { Eto.Forms.MessageBox.Show(Rhino.UI.RhinoEtoApp.MainWindow, "Could not attach the ECC TMM: " + error.Message, "Pachyderm"); }
                try
                {
                    if (Parameters.Input(0).Inputs.Count != 0) { attachComponents = false; return; }
                    LoadLibrary(false);
                    if (names.Length == 0) return;
                    var list = new Grasshopper2.Parameters.Special.ValueListObject();
                    // Populate through the public persistence API; the native Set method is internal.
                    var data = GrasshopperIO.DataBase.Node.Create();
                    list.Store(data);
                    data.RemoveNode("ListItems");
                    data.RemoveItem("ListSelection");
                    var items = names.Select(name => new Grasshopper2.Data.NamedPear(name, "\"" + name + "\"", null)).ToArray();
                    data.StorableArray("ListItems", items);
                    int selected = Array.FindIndex(names, name => string.Equals(name, selectedName, StringComparison.OrdinalIgnoreCase));
                    if (selected < 0) selected = 0;
                    data.BooleanArray("ListSelection", Enumerable.Range(0, names.Length).Select(i => i == selected).ToArray());
                    list = new Grasshopper2.Parameters.Special.ValueListObject(data);
                    list.Mode = Grasshopper2.Parameters.Special.ValueListMode.ShowOne;
                    list.UserName = "Materials";
                    list.Attributes.Pivot = new Eto.Drawing.PointF(Attributes.Pivot.X - 280, Attributes.Pivot.Y);
                    if (!document.Objects.Add(list)) return;
                    var actions = new Grasshopper2.Undo.ActionList(new Grasshopper2.Undo.Actions.CreateObjectAction(list));
                    Grasshopper2.Parameters.Connections.Connect(list, Parameters.Input(0), actions);
                    document.Undo.Do(new Grasshopper2.Undo.VerbNoun("Add", "material Value List"), actions);
                    attachComponents = false;
                }
                catch (Exception error) { Eto.Forms.MessageBox.Show(Rhino.UI.RhinoEtoApp.MainWindow, "Could not attach the ECC workflow: " + error.Message, "Pachyderm"); }
            });
        }
        protected override void AddInputs(InputAdder pManager)
        {
            pManager.AddText("Selection", "S", "Exact assembly name or zero-based index. Leave empty to use the preset selected in the component menu.", Access.Item).Set("");
            pManager.AddBoolean("Reload", "R", "Toggle false to true to reload the configured ECC library.", Access.Item).Set(false);
        }

        protected override void AddOutputs(OutputAdder pManager)
        {
            pManager.AddGeneric("Layers", "L", "ECC assembly layers in exposed-face-to-backing order, ready for TMM L.", Access.Twig);
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

        public override void AppendToInputPanel(Grasshopper2.UI.InputPanel.InputPanel panel)
        {
            base.AppendToInputPanel(panel);
            try
            {
                LoadLibrary(false);
                var presets = new Eto.Forms.DropDown { DataStore = names };
                presets.SelectedIndex = Array.FindIndex(names, name => string.Equals(name, selectedName, StringComparison.OrdinalIgnoreCase));
                presets.SelectedIndexChanged += (sender, args) =>
                {
                    if (!(presets.SelectedValue is string choice)) return;
                    AddUndoRecord(new Grasshopper2.Undo.VerbNoun("Select", "Preset"),
                        new Grasshopper2.Undo.Actions.PropertyAction(this, nameof(SelectedPreset), Grasshopper2.Undo.Effects.ExpireSolution));
                    selectedName = choice;
                    Expire();
                    Document?.Solution.Start();
                };
                panel.AddLabel("Preset");
                panel.Add(presets);
            }
            catch (Exception error) { panel.AddLabel(error.Message); }
        }
        protected override void Process(IDataAccess DA)
        {
            string selection = "";
            bool reload = false;
            if (!DA.GetItem(0, out selection) || !DA.GetItem(1, out reload)) return;
            try
            {
                LoadLibrary(reload && !wasReload);
                wasReload = reload;
                if (names.Length > 0) ScheduleAttachments(Document);
                if (names.Length == 0) throw new ArgumentException("No ECC assemblies were loaded. Check Pachyderm's library directory.");
                if (string.IsNullOrWhiteSpace(selection)) selection = selectedName;
                if (string.IsNullOrWhiteSpace(selection)) { Label = "Select a preset"; return; }
                int index = Array.FindIndex(names, name => string.Equals(name, selection, StringComparison.OrdinalIgnoreCase));
                if (index < 0 && (!int.TryParse(selection, NumberStyles.Integer, CultureInfo.InvariantCulture, out index) || index < 0 || index >= names.Length))
                    throw new ArgumentException("Select an existing ECC assembly by name or zero-based index.");
                ABS_Layer[] layers = library.Substrates[index];
                if (layers.Length == 0 || layers.Any(layer => layer == null || !(layer.depth > 0) || double.IsInfinity(layer.depth)))
                    throw new ArgumentException("The selected assembly has missing or invalid layer thicknesses.");
                // The library is backing-to-face; the Grasshopper TMM input is face-to-backing.
                Label = names[index];
                DA.SetTwig(0, layers.Reverse().ToArray());
            }
            catch (Exception error) { DA.AddMessage(Grasshopper2.Doc.Message.Error(error.Message, "")); }
        }

        public override void Store(IWriter writer)
        {
            base.Store(writer);
            writer.String("PresetName", selectedName);
        }
        protected override IIcon IconInternal
        {
            get
            {
                using (var stream = typeof(Mat_ECCAssemblyPreset).Assembly.GetManifestResourceStream("PachydermGH2.Resources.Medium Properties.png"))
                    return stream == null ? null : new PixelIcon(new Eto.Drawing.Bitmap(stream));
            }
        }
    }
}
