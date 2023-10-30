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
using System.Collections.Generic;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace PachydermGH
{
    public class OctFilter : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public OctFilter()
            : base("Octave Band Filter", "FilterOctave",
                "Filters incoming signal by an octave band (0 for 63 Hz, 7 for 8000 hz)",
                "Acoustics", "Audio")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Audio Signal", "Signal", "The data to divide...", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Octave Band", "oct", "integer indicating octave band (0 -> 63 Hz, 1 -> 125 Hz, 2 -> 250 Hz, 3 -? 500 Hz, 4 -> 1000 Hz, 5 -> 2000 Hz, 6 -> 4000 HZ, 7 -> 8000 Hz", GH_ParamAccess.item);
            Grasshopper.Kernel.Parameters.Param_Integer param = (pManager[1] as Grasshopper.Kernel.Parameters.Param_Integer);
            if (param != null) param.SetPersistentData(4);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("AudioSignal", "Signal", "The filtered signal.", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Audio_Signal Buffer = new Audio_Signal();
            int oct_id = 0;
            DA.GetData<Audio_Signal>(0, ref Buffer);
            DA.GetData<int>(1, ref oct_id);
            int[] direct_samples = new int[Buffer.ChannelCount];

            double[][] sig = new double[Buffer.ChannelCount][];

            for (int channel = 0; channel < Buffer.ChannelCount; channel++)
            {
                sig[channel] = new double[Buffer[channel].Length];
                for(int s = 0; s < sig[channel].Length; s++)
                {
                    sig[channel][s] = Buffer[channel][s];
                }
                direct_samples[channel] = Buffer.Direct_Sample[channel];
            }

            for (int channel = 0; channel < Buffer.ChannelCount; channel++)
            {
                sig[channel] = Pachyderm_Acoustic.Audio.Pach_SP.FIR_Bandpass(sig[channel], oct_id, Buffer.SampleFrequency, 0);
            }

            float[][] sigf = new float[sig.Length][];

            for (int channel = 0; channel < Buffer.ChannelCount; channel++)
            {
                sigf[channel] = new float[sig[0].Length];
                for (int s = 0; s < sig[channel].Length; s++)
                {
                    sigf[channel][s] = (float)sig[channel][s];
                }
            }

            Audio_Signal result = new Audio_Signal(sigf, Buffer.SampleFrequency, direct_samples);
            DA.SetData(0, result);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                System.Drawing.Bitmap b = Properties.Resources.Filter_Octave_Band;
                b.MakeTransparent(System.Drawing.Color.White);
                return b;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{8BAC28B1-041C-4A5A-8EC9-5997F95C875C}"); }
        }
    }
}