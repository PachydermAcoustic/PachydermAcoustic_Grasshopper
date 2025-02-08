//'Pachyderm-Acoustic: Geometrical Acoustics for Rhinoceros (GPL) by Arthur van der Harten 
//' 
//'This file is part of Pachyderm-Acoustic. 
//' 
//'Copyright (c) 2008-2025, Arthur van der Harten 
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
    public class RT_Sabine : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent2 class.
        /// </summary>
        public RT_Sabine()
            : base("Sabine Reverberation", "RT",
                "Computes reverberation time from the model using the Sabine method",
                "Acoustics", "Analysis")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Room", "R", "Pachyderm Scene", GH_ParamAccess.item);
            pManager.AddNumberParameter("Volume", "V", "Room volume in cubic meters.", GH_ParamAccess.item);

            Grasshopper.Kernel.Parameters.Param_Integer param = (pManager[1] as Grasshopper.Kernel.Parameters.Param_Integer);
            if (param != null) param.Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Rerberation Time", "RT", "Reverberation Time", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Pachyderm_Acoustic.Environment.Scene Room = null;
            DA.GetData<Pachyderm_Acoustic.Environment.Scene>(0, ref Room);
            double Volume = 0;
            DA.GetData<double>(1, ref Volume);

            double[] RT = new double[8];
            Pachyderm_Acoustic.Utilities.AcousticalMath.Sabine(Room, Volume, ref RT);

            DA.SetDataList(0, RT);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                System.Drawing.Bitmap b = Properties.Resources.RT;
                b.MakeTransparent(System.Drawing.Color.White);
                return b;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{61CF0D82-4D78-4575-996B-4162E9362E0B}"); }
        }
    }
}