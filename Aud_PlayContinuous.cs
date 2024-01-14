////'Pachyderm-Acoustic: Geometrical Acoustics for Rhinoceros (GPL) by Arthur van der Harten 
////' 
////'This file is part of Pachyderm-Acoustic. 
////' 
////'Copyright (c) 2008-2024, Arthur van der Harten 
////'Pachyderm-Acoustic is free software; you can redistribute it and/or modify 
////'it under the terms of the GNU General Public License as published 
////'by the Free Software Foundation; either version 3 of the License, or 
////'(at your option) any later version. 
////'Pachyderm-Acoustic is distributed in the hope that it will be useful, 
////'but WITHOUT ANY WARRANTY; without even the implied warranty of 
////'MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
////'GNU General Public License for more details. 
////' 
////'You should have received a copy of the GNU General Public 
////'License along with Pachyderm-Acoustic; if not, write to the Free Software 
////'Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA. 

//using System;
//using System.Windows.Forms;
//using Grasshopper.Kernel;

//namespace PachydermGH
//{
//    public class Continuous_Play : GH_Component
//    {

//        /// <summary>
//        /// Initializes a new instance of the MyComponent1 class.
//        /// </summary>
//        public Continuous_Play()
//            : base("Play Continuous", "P_Cont",
//                "Continuously Plays audio over a stream that can change attributes in real time.",
//                "Acoustics", "Audio")
//        {
//        }

//        /// <summary>
//        /// Registers all the input parameters for this component.
//        /// </summary>
//        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
//        {
//            pManager.AddGenericParameter("Signal", "S", "The first signal", GH_ParamAccess.item);
//        }

//        /// <summary>
//        /// Registers all the output parameters for this component.
//        /// </summary>
//        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
//        {
//        }

//        protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu)
//        {
//            Menu_AppendItem(menu, playing ? "Stop":"Play", Play_Click, true, false);
//            base.AppendAdditionalComponentMenuItems(menu);
//        }

//        private void Play_Click(object sender, EventArgs e)
//        {
//            if (playing)
//            {
//                //Add code to kill the audio.

//            }
//            else
//            {
//                //Add code to start the audio.

//            }
//        }

//        bool playing = false;
//        Modifiable_Provider MP;
//        SampleToWaveProvider WP;
//        NAudio.Wave.WaveOut Player;
//        /// <summary>
//        /// This is the method that actually does the work.
//        /// </summary>
//        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
//        protected override void SolveInstance(IGH_DataAccess DA)
//        {
//            Audio_Signal Signal1 = new Audio_Signal(), Signal2 = new Audio_Signal();
//            DA.GetData<Audio_Signal>(0, ref Signal1);
//            int SamplingFreq1 = Signal1.SampleFrequency;
//            if (MP == null) MP = new Modifiable_Provider(Signal1.SampleFrequency);
//            if (MP.sample_rate == Signal1.SampleFrequency)
//            {
//                WP = new NAudio.Wave.SampleProviders.SampleToWaveProvider(MP);
//                Player = new NAudio.Wave.WaveOut();
//                Player.NumberOfBuffers = 1;
//                Player.Volume = 1.0f;
//                Player.Init(WP);
//            }
//        }

//        /// <summary>
//        /// Provides an Icon for the component.
//        /// </summary>
//        protected override System.Drawing.Bitmap Icon
//        {
//            get
//            {
//                System.Drawing.Bitmap b = Properties.Resources.Divide_Signal;
//                b.MakeTransparent(System.Drawing.Color.White);
//                return b;
//            }
//        }

//        /// <summary>
//        /// Gets the unique ID for this component. Do not change this ID after release.
//        /// </summary>
//        public override Guid ComponentGuid
//        {
//            get { return new Guid("{37FDAA07-0088-4DD8-B3E7-8B4B5036F3AD}"); }
//        }

//        private class Modifiable_Provider : NAudio.Wave.ISampleProvider
//        {
//            int sample = 0;
//            public int sample_rate;
//            private float[] signal;
//            private System.Threading.Semaphore signal_available;


//            public Modifiable_Provider(int _sample_rate)
//            {
//                signal_available = new System.Threading.Semaphore(1, 1);
//                sample_rate = _sample_rate;
//            }


//            /// <summary>
//            /// The output WaveFormat of this sample provider
//            /// </summary>
//            public NAudio.Wave.WaveFormat WaveFormat { get; private set; }

//            public float[] Set_Signal
//            {
//                set
//                {
//                    signal_available.WaitOne();
//                    signal = value;
//                    signal_available.Release();
//                }
//            }

//            public int Read(float[] buffer, int offset, int sampleCount)
//            {
//                signal_available.WaitOne();
//                if (sample + offset >= signal.Length)
//                    sample = 0; ;

//                for (int n = 0; n < sampleCount; n++)
//                {
//                    int nsample = n + sample;
//                    if (nsample >= signal.Length)
//                    {
//                        //buffer[n + offset] = (float)(Amplitude * Math.Sin((2 * Math.PI * sample * Frequency) / sampleRate));
//                        //if (sample >= sampleRate) sample = 0;
//                        buffer[n + offset] = 0;
//                        //OnEnd(EventArgs.Empty);
//                        sample += n;
//                        return n;
//                    }
//                    else
//                    {
//                        buffer[n + offset] = signal[nsample];
//                    }
//                }
//                signal_available.Release();
//                sample += sampleCount;
//                return sampleCount;
//            }
//        }
//    }
//}