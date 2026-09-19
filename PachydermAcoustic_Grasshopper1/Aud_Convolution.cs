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
    public class Convolution : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public Convolution()
            : base("Convolution", "Conv",
                "Performs a convolution of your input data",
                "Acoustics", "Audio")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Signal 1", "S1", "The first signal", GH_ParamAccess.item);
            pManager.AddGenericParameter("Signal 2", "S2", "The first signal", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Result", "S_out", "The convolution of the two signals.", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Audio_Signal Signal1 = new Audio_Signal(), Signal2 = new Audio_Signal();
            DA.GetData<Audio_Signal>(0, ref Signal1);
            DA.GetData<Audio_Signal>(1, ref Signal2);
            int SamplingFreq1 = Signal1.SampleFrequency;
            int SamplingFreq2 = Signal2.SampleFrequency;

            if (SamplingFreq1 != SamplingFreq2) throw new Exception("At this time, Pachyderm only supports convolving signals with identical sampling frequencies...");
            if (Signal1.ChannelCount != 1 && Signal2.ChannelCount != 1) throw new Exception("With two signals with more than one channel, this convolution would not be applicable to room acoustics...");

            float[][] s_out = new float[(int)Math.Max(Signal1.ChannelCount, Signal2.ChannelCount)][];

            for (int c1 = 0; c1 < Signal1.ChannelCount; c1++)
            {
                for (int c2 = 0; c2 < Signal2.ChannelCount; c2++)
                {
                     s_out[Math.Max(c1, c2)] = Pachyderm_Acoustic.Audio.Pach_SP.FFT_Convolution(Signal1[c1], Signal2[c2], 0);
                }
            }

            Audio_Signal as_out = new Audio_Signal(s_out, SamplingFreq1);
            DA.SetData(0, as_out);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                System.Drawing.Bitmap b = Properties.Resources.Divide_Signal;
                b.MakeTransparent(System.Drawing.Color.White);
                return b;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{DC043794-6565-4E9B-A390-696EE111B405}"); }
        }
    }
}