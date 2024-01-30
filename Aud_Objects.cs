//'Pachyderm-Acoustic: Geometrical Acoustics for Rhinoceros (GPL) by Arthur van der Harten 
//' 
//'This file is part of Pachyderm-Acoustic. 
//' 
//'Copyright (c) 2008-2024, Arthur van der Harten 
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
using Grasshopper.Kernel.Types;

namespace PachydermGH
{
    public class Audio_Signal:Grasshopper.Kernel.Types.GH_Goo<double[][]>
    {
        int SamplingFrequency;
        int[] Sample_of_Direct = new int[1] { 0 };
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
            base.Value = new double[Aud_in.Length][];
            for (int i = 0; i < Aud_in.Length; i++) Value[i] = new double[Aud_in[i].Length];
            for (int i = 0; i < Aud_in.Length; i++) for(int j = 0; j < Aud_in[i].Length; j++) base.Value[i][j] = (double)Aud_in[i][j];
            //no jagged arrays allowed. Pad with zeros where necessary.
            int length = 0;
            foreach (float[] signal in Aud_in) if (signal.Length > length) length = signal.Length;
            for (int i = 0; i < base.Value.Length; i++) if (this[i].Length < length) Array.Resize<double>(ref base.Value[i], length);
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
            base.Value = Aud_in;
            //no jagged arrays allowed. Pad with zeros where necessary.
            int length = 0;
            foreach (double[] signal in Aud_in) if (signal.Length > length) length = signal.Length;
            for (int i = 0; i < base.Value.Length; i++) if (this[i].Length < length) Array.Resize<double>(ref base.Value[i], length);
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
                return base.Value[channel];
            }
            set
            {
                base.Value[channel] = value;
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
            get { return base.Value[0].Length; }
        }

        public int ChannelCount
        {
            get { return base.Value.Length; }
        }

        public override Grasshopper.Kernel.Types.IGH_Goo Duplicate()
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

        public override bool IsValid
        {
            get { return true; }
        }

        public override string IsValidWhyNot
        {
            get
            {
                return "Have data, will travel...";
            }
        }

        public override string ToString()
        {
            return string.Format("Audio: {0} channels, {1} samples, {2} Hz.", ChannelCount, this.Value[0].Length, SampleFrequency);
        }

        public override bool CastFrom(object source)
        {
            return base.CastFrom(source);
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

        public override bool CastTo<Q>(ref Q target)
        {
            //if (typeof(Q) is Grasshopper.Kernel.Types.GH_Point)
            //{
            //    List<Grasshopper.Kernel.Types.GH_Point>[] L = new List<Grasshopper.Kernel.Types.GH_Point>[ChannelCount];
            //    for(int c = 0; c <ChannelCount; c++)
            //    {
            //        L[c] = new List<Grasshopper.Kernel.Types.GH_Point>();
            //        for (int i = 0; i < Audio.Length; i++) L[c].Add(new Grasshopper.Kernel.Types.GH_Point(new Rhino.Geometry.Point3d(i/SamplingFrequency, Audio[c][i], 0)));
            //    }
            //    target = L as Q;
            //    return true;
            //}
            //else if(typeof(Q) is Grasshopper.Kernel.Types.GH_Number)
            //{
            //    target = new Grasshopper.Kernel.Types.GH_Number().CastFrom(Audio);
            //    return true;
            //}
            return base.CastTo<Q>(ref target);
        }

        public override string TypeDescription
        {
            get { return "Audio Signal"; }
        }

        public override string TypeName
        {
            get { return "Audio Signal"; }
        }
    }

    public class Frequency_Spectrum: Grasshopper.Kernel.Types.GH_Goo<System.Numerics.Complex[]>
    {
        public float[] Magnitude;
        public float[] Frequency;

        public Frequency_Spectrum()
        { }

        public Frequency_Spectrum(float[] Spec_in, System.Numerics.Complex[] Spec_Complex, float[] F_dom)
        {
            this.Value = Spec_Complex;
            Magnitude = Spec_in;
            Frequency = F_dom;
        }

        public override Grasshopper.Kernel.Types.IGH_Goo Duplicate()
        {
            throw new NotImplementedException();
        }

        public override bool IsValid
        {
            get { return true; }
        }

        public override string IsValidWhyNot
        {
            get
            {
                return "Have data, will travel...";
            }
        }

        public override string ToString()
        {
            return string.Format("Spectrum: {0} samples, {1} Hz. Max", Magnitude.Length, Frequency[Frequency.Length - 1]);
        }

        public override bool CastFrom(object source)
        {
            return base.CastFrom(source);
        }

        public override bool CastTo<Q>(ref Q target)
        {
                //if (typeof(Q) is Grasshopper.Kernel.Types.GH_Point)
                //{
                //    List<Grasshopper.Kernel.Types.GH_Point> L = new List<Grasshopper.Kernel.Types.GH_Point>();
                //    for (int i = 0; i < Magnitude.Length; i++) L.Add(new Grasshopper.Kernel.Types.GH_Point(new Rhino.Geometry.Point3d(Frequency[i], Magnitude[i], 0)));
                //}
                //if (typeof(Q) is Grasshopper.Kernel.Types.GH_Number)
                //{
                //    return new Grasshopper.Kernel.Types.GH_Number().CastFrom(Magnitude);
                //}
                //else
                //{
                    return base.CastTo<Q>(ref target);
            //}
        }

        public int Length
        {
            get
            {
                return Magnitude.Length;
            }
        }

        public override string TypeDescription
        {
            get { return "Frequency Spectrum"; }
        }

        public override string TypeName
        {
            get { return "Frequency Spectrum"; }
        }
    }
}