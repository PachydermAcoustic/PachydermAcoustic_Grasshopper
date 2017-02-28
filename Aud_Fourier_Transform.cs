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
using Grasshopper.Kernel;

namespace PachydermGH
{
    public class Fourier_Transform : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public Fourier_Transform()
            : base("FastFourierTransform", "FFTW",
                "Performs the Fourier Transform on your input data",
                "Acoustics", "Audio")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Audio Signal", "Signal", "The data to perform FFT on...", GH_ParamAccess.item);
            
            //pManager.AddIntegerParameter("Sample Frequency", "Freq", "Sampling Frequency of the input signal", GH_ParamAccess.item);
            //pManager.AddNumberParameter("Input Data", "Signal", "The data to perform FFT on...", GH_ParamAccess.tree);
            //pManager.AddNumberParameter("Input Data", "Signal", "The data to perform FFT on...", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Spectrum", "F", "Fourier Transform of the signal...", GH_ParamAccess.item);
            //pManager.AddNumberParameter("Z Magnitude", "Z", "The Symmetrical Frequency Spectrum of the input signal", GH_ParamAccess.tree);
            //pManager.AddNumberParameter("Z Magnitude Half Spectrum", "Z/2", "The Frequency Specturm of the input signal", GH_ParamAccess.tree);
            //pManager.AddNumberParameter("Frequency Domain", "F", "The frequency domain of the output spectrum", GH_ParamAccess.tree);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            int SamplingFreq = 0;
            Audio_Signal buffer = new Audio_Signal();
            DA.GetData<Audio_Signal>(0, ref buffer);
            SamplingFreq = buffer.SampleFrequency;
            //DA.GetData<Grasshopper.Kernel.Types.GH_Number>(1, out buffer);

            //Grasshopper.DataTree<float> signal_2 = new Grasshopper.DataTree<float>();//[Signal_FD.Length / 4];
            //Grasshopper.DataTree<float> f_domain = new Grasshopper.DataTree<float>();//new float[Signal_FD.Length / 4];
            //Grasshopper.DataTree<float> signal_whole = new Grasshopper.DataTree<float>();//[Signal_FD.Length / 4];
            
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
                //Array.Resize(ref SignalBuffer, W);
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
                ////signal_2.Add((float)Signal_FD[Signal_FD.Length / 2], new Grasshopper.Kernel.Data.GH_Path(b));
                ////f_domain.Add(df / 2, new Grasshopper.Kernel.Data.GH_Path(b));
                signal_2[0] = (float)Math.Sqrt(signal_C[0].Real * signal_C[0].Real + signal_C[0].Imaginary * signal_C[0].Imaginary);
                f_domain[0] = df / 2;

                for (int i = 1; i < signal_C.Length/2; i++)
                {
                    //signal_2[i] = (float)Math.Sqrt(((double)Signal_FD[i] * (double)Signal_FD[i]) + ((double)Signal_FD[i + Signal_FD.Length / 4] * (double)Signal_FD[i + Signal_FD.Length / 4]));
                    signal_2[i] = (float)Math.Sqrt(signal_C[i].Real * signal_C[i].Real + signal_C[i].Imaginary * signal_C[i].Imaginary);
                    f_domain[i] = (f_domain[i - 1] + df);
                }

                Frequency_Spectrum Spec = new Frequency_Spectrum(signal_2, signal_C, f_domain);

                //for (int i = 0; i < Signal_FD.Length; i++) signal_whole((float)Signal_FD[i], new Grasshopper.Kernel.Data.GH_Path(b));
                //}
                DA.SetData(0, Spec);
            }
            //DA.SetDataTree(0, signal_whole);
            //DA.SetDataTree(1, signal_2);
            //DA.SetDataTree(2, f_domain);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                System.Drawing.Bitmap b = Properties.Resources.FFT;
                b.MakeTransparent(System.Drawing.Color.White);
                return b;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{5A997419-58D7-4904-8605-F8926A60865D}"); }
        }
    }
}