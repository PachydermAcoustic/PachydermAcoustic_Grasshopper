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
using Grasshopper.Kernel;

namespace PachydermGH
{
    public class Mat_Airspace : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public Mat_Airspace()
            : base("Airspace", "AS",
                "Signifies an airspace layer.",
                "Acoustics", "Materials")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Distance", "D", "The depth(mm) of the airspace layer", GH_ParamAccess.item);
            (pManager[0] as Grasshopper.Kernel.Parameters.Param_Number).SetPersistentData(25);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Properties", "P", "Pachyderm airspace layer. Merge layers in exposed-face-to-backing order and connect to TMM.", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            double D = 0;
            if (!DA.GetData<double>(0, ref D)) D = 25;
            if (!(D > 0) || double.IsInfinity(D))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Airspace depth must be finite and positive.");
                return;
            }
            Pachyderm_Acoustic.AbsorptionModels.ABS_Layer AS = Pachyderm_Acoustic.AbsorptionModels.ABS_Layer.Airspace(D / 1000);
            DA.SetData(0, AS);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                System.Drawing.Bitmap b = Properties.Resources.Airspace_Layer;
                return b;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{AAC3FC9E-0B27-4CF7-B556-2FE5132C60AF}"); }
        }
    }
}
