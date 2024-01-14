//'Pachyderm-Acoustic: Geometrical Acoustics for Rhinoceros (GPL) by Arthur van der Harten 
//' 
//'This file is part of Pachyderm-Acoustic. 
//' 
//'Copyright (c) 2008-2024, Arthur van der Harten 
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
    public class STI_ETC : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent2 class.
        /// </summary>
        public STI_ETC()
            : base("Speech Transmission Index", "STI",
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
            pManager.AddNumberParameter("Noise Levels", "N", "Background Noise, by octave band...", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Speech Transmission Index", "STI", "Speech Transmission Index (3 forms - 0: General via 2003 standard; 2: Using a male speech spectrum; 2: Using a female speech spectrum;", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Audio_Signal ETC = null;
            DA.GetData<Audio_Signal>(0, ref ETC);
            List<double> Noise = new List<double>();
            DA.GetDataList<double>(1, Noise);
            if (Noise.Count != 8) throw new Exception("Noise should be specified by octave band, 0 for 63 Hz. through 7 for 8000 Hz.");

            double[][] etc = new double[8][];
            for (int oct = 0; oct < 8; oct++) etc[oct] = ETC[oct];

            double[] STI = Pachyderm_Acoustic.Utilities.AcousticalMath.Speech_Transmission_Index(etc, 343*1.22, Noise.ToArray(), ETC.SampleFrequency);

            DA.SetDataList(0, STI);
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
                return Properties.Resources.Speech_Transmission_Index_2;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{0DD773A3-374D-4449-A34C-63EC1C7E6ED3}"); }
        }
    }
}