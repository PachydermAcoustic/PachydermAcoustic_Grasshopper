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
using Grasshopper2.Data;
using System.Collections.Generic;

namespace PachydermGH
{
    [IoId("5A997419-58D7-4904-8605-F8926A60865D")]
    public class Fourier_Transform : Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public Fourier_Transform()
            : base(new Nomen("FastFourierTransform",
                "Performs the Fourier Transform on your input data",
                "Acoustics", "Audio"))
        {
            Threading = Grasshopper2.Components.ThreadingState.SingleThreaded;
        }

        public Fourier_Transform(IReader reader) : base(reader) { Threading = Grasshopper2.Components.ThreadingState.SingleThreaded; }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void AddInputs(InputAdder inputs)
        {
            inputs.AddGeneric("Audio Signal", "Signal", "The data to perform FFT on...", Access.Item);
            //inputs.AddInteger("Sample Frequency", "Freq", "Sampling Frequency of the input signal", Access.Item);
            //inputs.AddNumber("Input Data", "Signal", "The data to perform FFT on...", Access.Tree);
            //inputs.AddNumber("Input Data", "Signal", "The data to perform FFT on...", Access.Item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void AddOutputs(OutputAdder outputs)
        {
            outputs.AddGeneric("Spectrum", "F", "Fourier Transform of the signal...", Access.Tree);
            //inputs.AddNumber("Z Magnitude", "Z", "The Symmetrical Frequency Spectrum of the input signal", Access.Tree);
            //inputs.AddNumber("Z Magnitude Half Spectrum", "Z/2", "The Frequency Specturm of the input signal", Access.Tree);
            //inputs.AddNumber("Frequency Domain", "F", "The frequency domain of the output spectrum", Access.Tree);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void Process(IDataAccess access)
        {
            var signal = ComponentSupport.Signal(access, 0);
            var spectra = new List<Frequency_Spectrum>();
            for (int c = 0; c < signal.ChannelCount; c++) {
                var fft = Pachyderm_Acoustic.Audio.Pach_SP.FFT_General(signal[c], 0);
                int count = fft.Length / 2 + 1;
                var values = new System.Numerics.Complex[count];
                var magnitude = new float[count]; var frequency = new float[count];
                for (int k = 0; k < count; k++) { values[k] = fft[k]; magnitude[k] = (float)fft[k].Magnitude; frequency[k] = (float)((double)k * signal.SampleFrequency / fft.Length); }
                spectra.Add(new Frequency_Spectrum(magnitude, values, frequency));
            }
            ComponentSupport.SetTree(access, 0, Garden.TreeFromList(spectra));
        }
        protected override IIcon IconInternal
        {
            get
            {
                var assembly = typeof(SPLETC).Assembly;
                var resourceName = "PachydermGH2.Resources.FFT.png";

                using (var stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream == null) return null;

                    // FromStream reads serialized .ghicon data, not PNG/BMP images.
                    // The PixelIcon retains the bitmap for its cached lifetime.
                    return new Grasshopper2.UI.Icon.PixelIcon(new Eto.Drawing.Bitmap(stream));
                }
            }
        }
    }
}
