//'Pachyderm-Acoustic: Geometrical Acoustics for Rhinoceros (GPL) by Arthur van der Harten 
//' 
//'This file is part of Pachyderm-Acoustic. 
//' 
//'Copyright (c) 2008-2018, Arthur van der Harten 
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
    public class OpenWaveFile : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public OpenWaveFile()
            : base("Read WaveFile", ".Wav-in",
                "Opens a Wave File",
                "Acoustics", "Audio")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Wave File Path", "Path", "The location of the wave file.", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Signal Buffer", "Signal", "The signal held in the wave file", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<string> Path = new List<string>();
            DA.GetDataList<string>(0, Path);

            int[] Bitrate = new int[Path.Count];
            int[] Sample_Freq = new int[Path.Count];
            int[] ChannelCt = new int[Path.Count];
            List<float> Signal = new List<float>();

            for (int i = 0; i < Path.Count; i++)
            {
                NAudio.Wave.WaveFileReader WP = new NAudio.Wave.WaveFileReader(Path[i]);

                ChannelCt[i] = WP.WaveFormat.Channels;
                Sample_Freq[i] = WP.WaveFormat.SampleRate;
                Bitrate[i] = WP.WaveFormat.BitsPerSample;

                int BytesPerSample = WP.WaveFormat.Channels * Bitrate[i] / 8;
                int BytesPerChannel = WP.WaveFormat.BitsPerSample / 8;
                byte[] signalbuffer = new byte[BytesPerSample];

                if (WP.WaveFormat.BitsPerSample == 8)
                {
                    System.Windows.Forms.MessageBox.Show("Selected File is an 8-Bit audio file. This program requires a minimum bit-depth of 16.");
                    continue;
                }
                byte[] temp = new byte[4];
                float Max;

                switch (WP.WaveFormat.BitsPerSample)
                {
                    case 32:
                        Max = Int32.MaxValue;
                        break;
                    case 24:
                        Max = BitConverter.ToInt32(new byte[] { 0, byte.MaxValue, byte.MaxValue, byte.MaxValue }, 0);
                        break;
                    case 16:
                        Max = Int16.MaxValue;
                        break;
                    case 8:
                        Max = BitConverter.ToInt16(new byte[] { 0, byte.MaxValue }, 0);
                        break;
                    default:
                        throw new Exception("Invalid bit depth variable... Where did you get this audio file again?");
                }

                //for (int s = 0; s < WP.SampleCount; s++)//Have we chosen the right property to get the number of bytes in the file?
                //{
                //    WP.Read(signalbuffer, 0, BytesPerSample);
                //    temp = new Byte[4];

                //    if (WP.WaveFormat.BitsPerSample == 32)
                //    {
                //        for (int c = 0; c < ChannelCt[i]; c++)
                //        {
                //            temp[0] = signalbuffer[c * BytesPerChannel];
                //            temp[1] = signalbuffer[c * BytesPerChannel + 1];
                //            temp[2] = signalbuffer[c * BytesPerChannel + 2];
                //            temp[3] = signalbuffer[c * BytesPerChannel + 3];
                //            Signal.Add((float)(BitConverter.ToInt32(temp, 0) / Max));
                //        }
                //    }
                //    else if (WP.WaveFormat.BitsPerSample == 24)
                //    {
                //        for (int c = 0; c < ChannelCt[i]; c++)
                //        {
                //            temp[1] = signalbuffer[c * BytesPerChannel];
                //            temp[2] = signalbuffer[c * BytesPerChannel + 1];
                //            temp[3] = signalbuffer[c * BytesPerChannel + 2];
                //            Signal.Add((float)(BitConverter.ToInt32(temp, 0)) / Max);
                //        }
                //    }
                //    else if (WP.WaveFormat.BitsPerSample == 16)
                //    {
                //        for (int c = 0; c < ChannelCt[i]; c++)
                //        {
                //            temp[2] = signalbuffer[c * BytesPerChannel];
                //            temp[3] = signalbuffer[c * BytesPerChannel + 1];
                //            Signal.Add((float)(BitConverter.ToInt16(temp, 2)) / Max);
                //        }
                //    }
                //}
                
                float[][] S = new float[ChannelCt[i]][];
                //for (int j = 0; j < ChannelCt[i]; j++)
                //{
                //    int pos = 0;
                //    S[j] = new float[Signal.Count / ChannelCt[i]];
                //    for (int k = 0; k < (Signal.Count - ChannelCt[i]); k += ChannelCt[i])
                //    {
                //        S[j][pos] = Signal[k + j];
                //        pos++;
                //    }
                //}

                    for (int k = 0; k < ChannelCt[i]; k++) S[k] = new float[WP.SampleCount];
    
                    for (int k = 0; k < WP.SampleCount; k++)
                    {
                        float[] frame = WP.ReadNextSampleFrame();
                        for (int l = 0; l < frame.Length; l++)
                        {
                            S[l][k] = frame[l];
                        }
                    }

                Audio_Signal AS = new Audio_Signal(S, Sample_Freq[i]);
                DA.SetData(0, AS);
            }
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                System.Drawing.Bitmap b = Properties.Resources.Wave_File;
                b.MakeTransparent(System.Drawing.Color.White);
                return b;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{ce8f1991-f459-46c4-abcc-996b898e7206}"); }
        }
    }
}