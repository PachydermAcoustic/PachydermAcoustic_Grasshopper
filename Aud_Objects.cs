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

namespace PachydermGH
{
    public class Audio_Signal:Grasshopper.Kernel.Types.GH_Goo<float[][]>
    {
        int SamplingFrequency;

        public Audio_Signal()
        { 
        }

        public Audio_Signal(float[] Aud_in, int Fs)
        {
            SamplingFrequency = Fs;
            Value = new float[1][];
            Value[0] = Aud_in;        
        }

        public Audio_Signal(float[][] Aud_in, int Fs)
        {
            SamplingFrequency = Fs;
            base.Value = Aud_in;
            //no jagged arrays allowed. Pad with zeros where necessary.
            int length = 0;
            foreach (float[] signal in base.Value) if (signal.Length > length) length = signal.Length;
            for (int i = 0; i < base.Value.Length; i++) if (this[i].Length < length) Array.Resize<float>(ref base.Value[i], length);
        }

        public float[] this[int channel]
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

        public int SampleFrequency
        {
            get { return SamplingFrequency; }
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
            return string.Format("Audio: {0} channels, {1} samples, {2} Hz.", ChannelCount, this.Value[0].Length, SampleFrequency);
        }

        public override bool CastFrom(object source)
        {
            return base.CastFrom(source);
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