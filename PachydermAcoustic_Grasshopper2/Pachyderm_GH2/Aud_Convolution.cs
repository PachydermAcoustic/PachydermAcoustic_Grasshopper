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
    [IoId("DC043794-6565-4E9B-A390-696EE111B405")]
    public class Convolution : Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public Convolution()
            : base(new Nomen("Convolution",
                "Performs a convolution of your input data",
                "Acoustics", "Audio"))
        {
        }

        public Convolution(IReader reader) : base(reader) { }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void AddInputs(InputAdder inputs)
        {
            inputs.AddGeneric("Signal 1", "S1", "The first signal", Access.Item);
            inputs.AddGeneric("Signal 2", "S2", "The first signal", Access.Item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void AddOutputs(OutputAdder outputs)
        {
            outputs.AddGeneric("Result", "S_out", "The convolution of the two signals.", Access.Item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void Process(IDataAccess access)
        {
            Audio_Signal Signal1 = new Audio_Signal(), Signal2 = new Audio_Signal();
            access.GetItem<Audio_Signal>(0, out Signal1);
            access.GetItem<Audio_Signal>(1, out Signal2);
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
            access.SetItem(0, as_out);
        }
        protected override IIcon IconInternal
        {
            get
            {
                var assembly = typeof(SPLETC).Assembly;
                var resourceName = "Pachyderm_GH.Icons.Divide_Signal.png";

                using (var stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream == null) return null;

                    var ms = new System.IO.MemoryStream();
                    stream.CopyTo(ms);
                    ms.Position = 0;
                    return Grasshopper2.UI.Icon.PixelIcon.FromStream(ms);
                }
            }
        }

        //protected override IIcon IconInternal => new Grasshopper2.UI.Icon.PixelIcon("Divide_Signal");
    }
}