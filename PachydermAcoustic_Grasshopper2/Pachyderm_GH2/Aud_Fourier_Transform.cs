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
        }

        public Fourier_Transform(IReader reader) : base(reader) { }

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
            outputs.AddGeneric("Spectrum", "F", "Fourier Transform of the signal...", Access.Item);
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
            int SamplingFreq = 0;
            Audio_Signal buffer = new Audio_Signal();
            access.GetItem<Audio_Signal>(0, out buffer);
            SamplingFreq = buffer.SampleFrequency;
            //access.GetItem<Grasshopper.Kernel.Types.GH_Number>(1, out buffer);

            //Grasshopper2.Data.Tree<float> signal_2 = new Grasshopper2.Data.Tree<float>();//[Signal_FD.Length / 4];
            //Grasshopper2.Data.Tree<float> f_domain = new Grasshopper2.Data.Tree<float>();//new float[Signal_FD.Length / 4];
            //Grasshopper2.Data.Tree<float> signal_whole = new Grasshopper2.Data.Tree<float>();//[Signal_FD.Length / 4];
            
            //for (int b = 0; b < buffer.Branches.Count; b++)
            //{
            float[][] Channels = new float[buffer.ChannelCount][];
            float[][] freq = new float[buffer.ChannelCount][];
            for (int c = 0; c < buffer.ChannelCount; c++)
            {
                System.Numerics.Complex[] signal_C = Pachyderm_Acoustic.Audio.Pach_SP.FFT_General(buffer[c], 0);

                //int s_ct = buffer.Branches[b].Count;
                //int W = 1;
                //do { W *= 2; } while (W < buffer.Count);

                //double[] SignalBuffer = buffer.(c);


                //for (int i = 0; i < buffer.Count; i++) SignalBuffer[i] = buffer[i];

                //Real Declarations
                //Array.Resize(out SignalBuffer, W);
                //GCHandle S_in = GCHandle.Alloc(SignalBuffer, GCHandleType.Pinned);

                ////Complex Declarations
                //double[] Signal_FD = new double[2 * W];
                //GCHandle S_out = GCHandle.Alloc(Signal_FD, GCHandleType.Pinned);

                //int W2 = W / 2;

                ///// Straight Frequency Domain Convolution
                //IntPtr Signal_in = fftw.malloc(W * 8);
                //Marshal.Copy(SignalBuffer, 0, Signal_in, W);

                //IntPtr Signal_out = fftw.malloc(2 * W * 8);
                //Marshal.Copy(Signal_FD, 0, Signal_out, 2 * W);

                //IntPtr S_Plan = fftw.dft_r2c_1d(W, Signal_in, Signal_out, fftw_flags.Estimate);
                //fftw.execute(S_Plan);

                //Marshal.Copy(Signal_out, Signal_FD, 0, 2 * W);

                float[] signal_2 = new float[signal_C.Length / 2];
                float[] f_domain = new float[signal_C.Length / 2];
                //float[] signal_whole = new float[Signal_FD.Length / 4];
                //System.Numerics.Complex[] signal_C = new System.Numerics.Complex[Signal_FD.Length / 4];

                float df = (float)SamplingFreq / signal_C.Length;
                ////signal_2.Add((float)Signal_FD[Signal_FD.Length / 2], new Grasshopper2.Data.Path(b));
                ////f_domain.Add(df / 2, new Grasshopper2.Data.Path(b));
                signal_2[0] = (float)Math.Sqrt(signal_C[0].Real * signal_C[0].Real + signal_C[0].Imaginary * signal_C[0].Imaginary);
                f_domain[0] = df / 2;

                for (int i = 1; i < signal_C.Length/2; i++)
                {
                    //signal_2[i] = (float)Math.Sqrt(((double)Signal_FD[i] * (double)Signal_FD[i]) + ((double)Signal_FD[i + Signal_FD.Length / 4] * (double)Signal_FD[i + Signal_FD.Length / 4]));
                    signal_2[i] = (float)Math.Sqrt(signal_C[i].Real * signal_C[i].Real + signal_C[i].Imaginary * signal_C[i].Imaginary);
                    f_domain[i] = (f_domain[i - 1] + df);
                }

                Frequency_Spectrum Spec = new Frequency_Spectrum(signal_2, signal_C, f_domain);

                //for (int i = 0; i < Signal_FD.Length; i++) signal_whole((float)Signal_FD[i], new Grasshopper2.Data.Path(b));
                //}
                access.SetItem(0, Spec);
            }
            //access.SetItemTree(0, signal_whole);
            //access.SetItemTree(1, signal_2);
            //access.SetItemTree(2, f_domain);
        }
        protected override IIcon IconInternal
        {
            get
            {
                var assembly = typeof(SPLETC).Assembly;
                var resourceName = "Pachyderm_GH.Icons.FFT.png";

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
    }
}