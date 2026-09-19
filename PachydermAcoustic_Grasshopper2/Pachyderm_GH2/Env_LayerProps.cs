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
using Grasshopper2.Components;
using Grasshopper2.Parameters;
using Grasshopper2.UI;
using Grasshopper2.UI.Icon;
using GrasshopperIO;
using Pachyderm_Acoustic;
using Pachyderm_Acoustic.Environment;
using Rhino.Geometry;
using Grasshopper2.Data;
using System.Collections.Generic;
using Grasshopper2.UI.InputPanel;
using System.Linq;

namespace PachydermGH
{
    [IoId("B04F1D06-D582-41F8-BFC5-EEAE47F4D7D3")]
    public class AssignLayerProperties : Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public AssignLayerProperties()
            : base(new Nomen("Layer Material Properties",
                "Assign Properties of materials by Layer.",
                "Acoustics", "Model"))
        {
        }

        public AssignLayerProperties(IReader reader) : base(reader) { }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void AddInputs(InputAdder inputs)
        {
            inputs.AddInteger("Layer Index", "L ID", "The zero-based index of the layer. This is also how layer are identified in scene components.", Access.Item);
            inputs.AddInteger("Absorption", "ABS", "The absorption coefficient of the material in percent (%). (provide 8 - one for each octave.)", Access.Tree);
            inputs.AddInteger("Scattering", "SCT", "The scattering coefficient of the material in percent (%). (provide 8 - one for each octave.)", Access.Tree, Requirement.MayBeMissing);
            inputs.AddInteger("Transmission", "TRN", "The transmission coefficient of the material in percent (%). (provide 8 - one for each octave.)", Access.Tree, Requirement.MayBeMissing);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void AddOutputs(OutputAdder outputs)
        {
            outputs.AddInteger("Layer Index", "L ID", "The zero-based index of the layer. This is also how layer are identified in scene components.", Access.Item);
        }

        bool recompute = true;
        bool expired = false;

        //protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu)
        //{
        //    Menu_AppendItem(menu, "Trigger Recompute", ToggleRecompute, true, recompute);
        //    base.AppendAdditionalComponentMenuItems(menu);
        //}

        private void ToggleRecompute(object sender, EventArgs e)
        {
            recompute = !recompute;
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void Process(IDataAccess access)
        {
            int id = 0;
            Tree<int> ABS, SCT, TRN;

            //access.AddMessage = "";

            if (!access.GetItem<int>(0, out id)) throw new Exception("Please provide a layer index to assign material to.");
            if (!access.GetTree<int>(1, out ABS)) throw new Exception("Please provide absorption information at a minimum.");
            if (!access.GetTree<int>(2, out SCT))
            {
                access.AddMessage(Grasshopper2.Doc.Message.Remark("Default Scattering - 0.1 all octaves \n",""));
                SCT = Garden.TreeFromList(new System.Collections.Generic.List<int> { 10, 10, 10, 10, 10, 10, 10, 10 });
            }
            if (!access.GetTree<int>(3, out TRN))
            {
                access.AddMessage(Grasshopper2.Doc.Message.Remark("No transmission.", ""));
                TRN = Garden.TreeFromList(new System.Collections.Generic.List<int> { 0, 0, 0, 0, 0, 0, 0, 0 });
            }

            for(int i = 0; i < ABS.ItemCount; i++) { ABS.AllItems.ToArray()[i] *= 10; }

            Pachyderm_Acoustic.Utilities.RCPachTools.Material_SetLayer(id, ABS.AllItems.ToArray(), SCT.AllItems.ToArray(), TRN.AllItems.ToArray());

            access.SetItem(0, id);

            if (recompute && !expired)
            {
                //foreach (Grasshopper2.Doc.DocumentObject C in Grasshopper2.Doc.ObjectList)
                //{
                //    if (C is NURBSScene_Component) (C as NURBSScene_Component).ExpireSolution(true);
                //    if (C is PolygonScene_Component) (C as PolygonScene_Component).ExpireSolution(true);
                //}
                expired = true;
            }
            else expired = false;

        }
        protected override IIcon IconInternal
        {
            get
            {
                var assembly = typeof(SPLETC).Assembly;
                var resourceName = "Pachyderm_GH.Icons.Medium_Properties.png";

                using (var stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream == null) return null;

                    var ms = new System.IO.MemoryStream();
                    stream.CopyTo(ms);
                    ms.Position = 0;
                    return Grasshopper2.UI.Icon.PixelIcon.FromStream(ms);
                }
            }
        }
        //protected override IIcon IconInternal => new Grasshopper2.UI.Icon.PixelIcon("Medium_Properties");
    }
}