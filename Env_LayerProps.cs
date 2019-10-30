//'Pachyderm-Acoustic: Geometrical Acoustics for Rhinoceros (GPL) by Arthur van der Harten 
//' 
//'This file is part of Pachyderm-Acoustic. 
//' 
//'Copyright (c) 2008-2019, Arthur van der Harten 
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
using Grasshopper.Kernel;

namespace PachydermGH
{
    public class AssignLayerProperties : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public AssignLayerProperties()
            : base("Layer Material Properties", "LMP",
                "Assign Properties of materials by Layer.",
                "Acoustics", "Model")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddIntegerParameter("Layer Index", "L ID", "The zero-based index of the layer. This is also how layer are identified in scene components.", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Absorption", "ABS", "The absorption coefficient of the material. (provide 8 - one for each octave.)", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Scattering", "SCT", "The scattering coefficient of the material. (provide 8 - one for each octave.)", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Transmission", "TRN", "The transmission coefficient of the material. (provide 8 - one for each octave.)", GH_ParamAccess.list);
            pManager[2].Optional = true;
            pManager[3].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddIntegerParameter("Layer Index", "L ID", "The zero-based index of the layer. This is also how layer are identified in scene components.", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            int id = 0;
            System.Collections.Generic.List<int> ABS = new System.Collections.Generic.List<int>(), SCT = new System.Collections.Generic.List<int>(), TRN = new System.Collections.Generic.List<int>();

            this.Message = "";

            if (!DA.GetData<int>(0, ref id)) throw new Exception("Please provide a layer index to assign material to.");
            if (!DA.GetDataList<int>(1, ABS)) throw new Exception("Please provide absorption information at a minimum.");
            if (!DA.GetDataList<int>(2, SCT))
            {
                this.Message += "Default Scattering - 0.1 all octaves \n";
                SCT = new System.Collections.Generic.List<int> { 10, 10, 10, 10, 10, 10, 10, 10 };
            }
            if (!DA.GetDataList<int>(3, TRN))
            {
                this.Message += "No transmission.";
                TRN = new System.Collections.Generic.List<int> { 0, 0, 0, 0, 0, 0, 0, 0 };
            }

            Pachyderm_Acoustic.Utilities.RC_PachTools.Material_SetLayer(id, ABS.ToArray(), SCT.ToArray(), TRN.ToArray());

            DA.SetData(0, id);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                System.Drawing.Bitmap b = Properties.Resources.Medium_Properties;
                b.MakeTransparent(System.Drawing.Color.White);
                return b;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{B04F1D06-D582-41F8-BFC5-EEAE47F4D7D3}"); }
        }
    }
}