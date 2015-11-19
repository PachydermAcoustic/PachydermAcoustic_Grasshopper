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
using Rhino.Geometry;

namespace PachydermGH
{
    public class RT_X_ETC : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent2 class.
        /// </summary>
        public RT_X_ETC()
            : base("Reverberation Time", "RT",
                "Computes reverberation time from Energy Time Curve",
                "Acoustics", "Analysis")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Energy Time Curve", "ETC", "Energy Time Curve", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Decay Index", "t_X", "A linear regression will be performed on the schroeder integral from -5 dB of decay to -X-5 dB of decay", GH_ParamAccess.item);

            Grasshopper.Kernel.Parameters.Param_Integer param = (pManager[1] as Grasshopper.Kernel.Parameters.Param_Integer);
            if (param != null) param.SetPersistentData(30);
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
            Audio_Signal ETC = null;
            DA.GetData<Audio_Signal>(0, ref ETC);
            int tx = 30;
            DA.GetData<int>(1, ref tx);

            List<double> RT = new List<double>();
            foreach (double[] f in ETC.Value)
            {
                double[] s = new double[f.Length];
                for (int i = 0; i < f.Length; i++) s[i] += (double)f[i];
                double[] si = Pachyderm_Acoustic.Utilities.AcousticalMath.Schroeder_Integral(s);
                RT.Add(Pachyderm_Acoustic.Utilities.AcousticalMath.T_X(si, tx, ETC.SampleFrequency));
            }

            DA.SetDataList(0, RT);
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
            get { return new Guid("{575F7509-97DE-4021-8A46-B72BA96E6531}"); }
        }
    }
}