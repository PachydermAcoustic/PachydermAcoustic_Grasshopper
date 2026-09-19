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
using System.Numerics;
using System.Collections;
using System.Linq;

namespace PachydermGH
{
    public class Audio_Signal: IEnumerable<double[][]>
    {
        int SamplingFrequency;
        int[] Sample_of_Direct = new int[1] { 0 };
        public double[][] Value;
        public Audio_Signal()
        {
        }

        public Audio_Signal(float[] Aud_in, int Fs, int Direct_Sample = 0)
        {
            SamplingFrequency = Fs;
            Value = new double[1][];
            Value[0] = new double[Aud_in.Length];
            for(int i = 0; i < Aud_in.Length; i++) Value[0][i] = (double)Aud_in[i];
            Sample_of_Direct = new int[1] { Direct_Sample };
        }

        public Audio_Signal(double[] Aud_in, int Fs, int Direct_Sample = 0)
        {
            SamplingFrequency = Fs;
            Value = new double[1][];
            Value[0] = Aud_in;
            Sample_of_Direct = new int[1] { Direct_Sample };
        }

        public Audio_Signal(float[][] Aud_in, int Fs, int[] Direct_Sample = null)
        {
            SamplingFrequency = Fs;
            Value = new double[Aud_in.Length][];
            for (int i = 0; i < Aud_in.Length; i++) Value[i] = new double[Aud_in[i].Length];
            for (int i = 0; i < Aud_in.Length; i++) for(int j = 0; j < Aud_in[i].Length; j++) Value[i][j] = (double)Aud_in[i][j];
            //no jagged arrays allowed. Pad with zeros where necessary.
            int length = 0;
            foreach (float[] signal in Aud_in) if (signal.Length > length) length = signal.Length;
            for (int i = 0; i < Value.Length; i++) if (this[i].Length < length) Array.Resize<double>(ref Value[i], length);
            if (Direct_Sample == null)
            {
                Sample_of_Direct = new int[Aud_in.Length];
                for(int i = 0; i < Aud_in.Length; i++)
                {
                    Sample_of_Direct[i] = 0;
                }
            }
            else { Sample_of_Direct = Direct_Sample; }
        }

        public Audio_Signal(double[][] Aud_in, int Fs, int[] Direct_Sample = null)
        {
            SamplingFrequency = Fs;
            Value = Aud_in;
            //no jagged arrays allowed. Pad with zeros where necessary.
            int length = 0;
            foreach (double[] signal in Aud_in) if (signal.Length > length) length = signal.Length;
            for (int i = 0; i < Value.Length; i++) if (this[i].Length < length) Array.Resize<double>(ref Value[i], length);
            if (Direct_Sample == null)
            {
                Sample_of_Direct = new int[Aud_in.Length];
                for (int i = 0; i < Aud_in.Length; i++)
                {
                    Sample_of_Direct[i] = 0;
                }
            }
            else { Sample_of_Direct = Direct_Sample; }
        }

        public double[] this[int channel]
        {
            get
            {
                return Value[channel];
            }
            set
            {
                Value[channel] = value;
            }
        }

        public float[] toFloat(int c)
        {
            float[] ret = new float[Value[c].Length];
            for (int i = 0; i < Value[c].Length; i++) ret[i] = (float)Value[c][i];
            return ret;
        }

        public float[][] toFloat()
        {
            float[][] ret = new float[Value.Length][];
            for (int i = 0; i < Value.Length; i++) ret[i] = new float[Value[i].Length];
            for (int i = 0; i < Value.Length; i++) for (int j = 0; j < Value[i].Length; j++) ret[i][j] = (float)Value[i][j];
            return ret;
        }

        public int SampleFrequency
        {
            get { return SamplingFrequency; }
        }

        public int[] Direct_Sample
        {
            get { return Sample_of_Direct; }
            set { Sample_of_Direct = value; }
        }

        public double Direct_Time(int channel)
        {
            return (double)Sample_of_Direct[channel] / (double)SampleFrequency;
        }

        public int Count
        {
            get { return Value[0].Length; }
        }

        public int ChannelCount
        {
            get { return Value.Length; }
        }

        public Audio_Signal Duplicate()
        {
            //Audio_Signal ASdup = new Audio_Signal();
            double[][] dup = new double[Value.Length][];
            for (int i = 0; i < Value.Length; i++)
            {
                dup[i] = new double[Value[i].Length];
                for (int j = 0; j < Value[i].Length; j++) dup[i][j] = Value[i][j];
            }
            return new Audio_Signal(dup, SampleFrequency);
        }

        public override string ToString()
        {
            return string.Format("Audio: {0} channels, {1} samples, {2} Hz.", ChannelCount, this.Value[0].Length, SampleFrequency);
        }

        public IEnumerator<double[][]> GetEnumerator()
        {
            return ((IEnumerable<double[][]>)Value.ToList()).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public static Audio_Signal operator +(Audio_Signal AS1, Audio_Signal AS2)
        {
            if (AS1.Count != AS2.Count && AS1.ChannelCount != AS2.ChannelCount) throw new Exception("Audio Signals do not have the same number of channels or samples...");
            Audio_Signal AS_new = AS1.Duplicate() as Audio_Signal;
            for (int c = 0; c < AS1.ChannelCount; c++)
            {
                for(int s = 0; s < AS1.Count; s++)
                {
                    AS_new[c][s] += AS2[c][s];
                }
            }
            return AS_new;
        }
    }

    public class Frequency_Spectrum: IEnumerable<Complex[]>
    {
        public float[] Magnitude;
        public float[] Frequency;
        public List<Complex> Value;

        public Frequency_Spectrum()
        { }

        public Frequency_Spectrum(float[] Spec_in, System.Numerics.Complex[] Spec_Complex, float[] F_dom)
        {
            Value = Spec_Complex.ToList<Complex>();
            Magnitude = Spec_in;
            Frequency = F_dom;
        }

        public override string ToString()
        {
            return string.Format("Spectrum: {0} samples, {1} Hz. Max", Magnitude.Length, Frequency[Frequency.Length - 1]);
        }

        public IEnumerator<Complex[]> GetEnumerator()
        {
            return ((IEnumerable<Complex[]>)Value).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Length
        {
            get
            {
                return Magnitude.Length;
            }
        }
        
        public string TypeDescription
        {
            get { return "Frequency Spectrum"; }
        }

        public string TypeName
        {
            get { return "Frequency Spectrum"; }
        }
    }
}