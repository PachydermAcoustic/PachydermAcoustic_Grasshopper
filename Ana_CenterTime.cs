//'Pachyderm-Acoustic: Geometrical Acoustics for Rhinoceros (GPL) by Arthur van der Harten 
//' 
//'This file is part of Pachyderm-Acoustic. 
//' 
//'Copyright (c) 2008-2015, Arthur van der Harten 
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
using System.Collections.Generic;
using Grasshopper.Kernel;

namespace PachydermGH
{
    public class CT_ETC : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent2 class.
        /// </summary>
        public CT_ETC()
            : base("Center Time", "CT",
                "Computes Center Time from Energy Time Curve",
                "Acoustics", "Analysis")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Energy Time Curve", "ETC", "Energy Time Curve", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Center Time", "CT", "Center Time", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Audio_Signal ETC = null;
            DA.GetData<Audio_Signal>(0, ref ETC);
            int Cx = 30;
            DA.GetData<int>(1, ref Cx);

            List<double> CT = new List<double>();
            foreach (double[] f in ETC.Value)
            {
                double[] s = new double[f.Length];
                int start = 0;
                for (int i = 0; i < f.Length; i++)
                {
                    s[i] += (double)f[i];
                    if (start == 0) if (s[i] != 0) start = i;
                }
                CT.Add(Pachyderm_Acoustic.Utilities.AcousticalMath.Center_Time(s, ETC.SampleFrequency, (double)start/(double)ETC.SampleFrequency));
            }

            DA.SetDataList(0, CT);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return null;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{5D6451A7-525E-4203-9205-2D33BEB56007}"); }
        }
    }
}