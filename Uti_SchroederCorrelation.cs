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
using System.Linq;
using System.Windows.Forms;
using Grasshopper.Kernel;
using Pachyderm_Acoustic.Utilities;
using Rhino.Geometry;

namespace PachydermGH
{
    public class Schroeder_Correlation : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent2 class.
        /// </summary>
        public Schroeder_Correlation()
            : base("Schroeder Correlation", "Schr_Corr",
                "Calculates the Schroeder integral of two impulse responses, and then calculates the correlation between them.",
                "Acoustics", "Utility")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Impulse Response 1", "IR1", "Plug the audio signal impulse response in here.", GH_ParamAccess.item);
            pManager.AddGenericParameter("Impulse Response 2", "IR2", "Plug the audio signal impulse response in here.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Delay", "D", "Enter an offset in samples to help match the two integrals. A positive number will take samples off the front, of IR1. A negative numuber will take sampels off the front of IR2.", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Correlation R", "R_c", "Correlation R value for the two IRs Schroeder Integrals.", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Audio_Signal IR1 = new Audio_Signal();
            DA.GetData<Audio_Signal>(0, ref IR1);
            Audio_Signal IR2 = new Audio_Signal();
            DA.GetData<Audio_Signal>(1, ref IR2);
            double delay = 0;
            DA.GetData<double>(2, ref delay);

            double[][] signal1 = new double[8][];
            for (int i = 0; i < IR1.ChannelCount; i++) signal1[i] = new double[IR1.Count];
            double[][] signal2 = new double[8][];
            for (int i = 0; i < IR2.ChannelCount; i++) signal2[i] = new double[IR2.Count];

            List<double> R = new List<double>();

            for (int j = 0; j < IR1.ChannelCount; j++)
            {
                double[] ETC1 = AcousticalMath.SPL_Intensity_Signal(IR1.Value[j]);
                double[] Schr1 = AcousticalMath.Schroeder_Integral(ETC1);
                double[] ETC2 = AcousticalMath.SPL_Intensity_Signal(IR2.Value[j]);
                double[] Schr2 = AcousticalMath.Schroeder_Integral(ETC2);

                if (delay > 0) { Schr1.Reverse(); Array.Resize(ref Schr1, Schr1.Length - (int)delay); Schr1.Reverse(); }
                if (delay < 0) { Schr2.Reverse(); Array.Resize(ref Schr2, Schr2.Length + (int)delay); Schr2.Reverse(); }

                if (Schr1.Length < Schr2.Length) Array.Resize(ref Schr1, Schr2.Length);
                if (Schr2.Length < Schr1.Length) Array.Resize(ref Schr2, Schr1.Length);


                R.Add(MathNet.Numerics.Statistics.Correlation.Spearman(Schr1, Schr2));
            }

            DA.SetDataList(0, R);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                System.Drawing.Bitmap b = Properties.Resources.Energy_Time_Curve;
                b.MakeTransparent(System.Drawing.Color.White);
                return b;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{EFCA3675-B9C5-4428-B69C-7E1CA000C86B}"); }
        }
    }
}