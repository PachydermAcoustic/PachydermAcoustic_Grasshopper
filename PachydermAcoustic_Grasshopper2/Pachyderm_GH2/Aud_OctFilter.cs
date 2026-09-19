using Grasshopper2.Data;
using System.Linq;
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
using System.Collections.Generic;

namespace PachydermGH
{
    [IoId("8BAC28B1-041C-4A5A-8EC9-5997F95C875C")]
    public class OctFilter : Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public OctFilter()
            : base(new Nomen("Octave Band Filter",
                "Filters incoming signal by an octave band (0 for 63 Hz, 7 for 8000 hz)",
                "Acoustics", "Audio"))
        {
            Threading = Grasshopper2.Components.ThreadingState.SingleThreaded;
        }

        public OctFilter(IReader reader) : base(reader) { Threading = Grasshopper2.Components.ThreadingState.SingleThreaded; }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void AddInputs(InputAdder inputs)
        {
            inputs.AddGeneric("Audio Signal", "Signal", "The data to divide...", Access.Item);
            inputs.AddInteger("Octave Band", "oct", "integer indicating octave band (0 -> 63 Hz, 1 -> 125 Hz, 2 -> 250 Hz, 3 -? 500 Hz, 4 -> 1000 Hz, 5 -> 2000 Hz, 6 -> 4000 HZ, 7 -> 8000 Hz", Access.Item);
            //Grasshopper.Kernel.Parameters.Param_Integer param = (inputs[1] as Grasshopper.Kernel.Parameters.Param_Integer);
            //if (param != null) param.SetPersistentData(4);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void AddOutputs(OutputAdder outputs)
        {
            outputs.AddGeneric("AudioSignal", "Signal", "The filtered signal.", Access.Item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void Process(IDataAccess access)
        {
            Audio_Signal Buffer = new Audio_Signal();
            int oct_id = 0;
            Buffer = ComponentSupport.Signal(access, 0);
            access.GetItem<int>(1, out oct_id);
            if (oct_id < 0 || oct_id > 7) throw new ArgumentException("Octave band must be 0 through 7.");
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
            access.SetItem(0, result);
        }
        protected override IIcon IconInternal
        {
            get
            {
                var assembly = typeof(SPLETC).Assembly;
                var resourceName = "PachydermGH2.Resources.Filter Octave Band.png";

                using (var stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream == null) return null;

                    // FromStream reads serialized .ghicon data, not PNG/BMP images.
                    // The PixelIcon retains the bitmap for its cached lifetime.
                    return new Grasshopper2.UI.Icon.PixelIcon(new Eto.Drawing.Bitmap(stream));
                }
            }
        }
        //protected override IIcon IconInternal => new Grasshopper2.UI.Icon.PixelIcon("Filter_Octave_Band");
    }
}
