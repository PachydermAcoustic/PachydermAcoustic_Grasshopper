using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper2.Components;
using Grasshopper2.Data;
using Pachyderm_Acoustic.Environment;
using DS = Pachyderm_Acoustic.Direct_Sound;
using IS = Pachyderm_Acoustic.ImageSourceData;

namespace PachydermGH
{
    internal static class ComponentSupport
    {
        private sealed class Delay { public double Milliseconds; }
        private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<Source,Delay> Delays = new System.Runtime.CompilerServices.ConditionalWeakTable<Source,Delay>();
        public static void SetDelay(Source source,double milliseconds) {
            if(double.IsNaN(milliseconds) || double.IsInfinity(milliseconds) || milliseconds<0) throw new ArgumentException("Source delay must be a finite, nonnegative number of milliseconds.");
            Delays.GetOrCreateValue(source).Milliseconds=milliseconds;
        }
        public static double GetDelay(Source source) => Delays.TryGetValue(source,out var delay) ? delay.Milliseconds : 0;
        public static void SetTree(IDataAccess access,int index,ITree tree)
        {
            access.SetTree(index, access.Iterations>1 ? tree.WithPathPrefix(access.Index) : tree);
        }
        public static Audio_Signal Signal(IDataAccess access, int input)
        {
            if (!access.GetItem<Audio_Signal>(input, out var signal) || signal == null || signal.Value == null || signal.ChannelCount == 0 || signal.Count == 0 || signal.SampleFrequency <= 0)
                throw new ArgumentException("Provide a nonempty audio signal with a positive sample rate.");
            return signal;
        }
        public static T[] Items<T>(IDataAccess access, int index)
        {
            return access.GetTree<T>(index, out var tree) && tree != null ? tree.AllItems.ToArray() : Array.Empty<T>();
        }
        public static Audio_Signal Sum(Audio_Signal a, Audio_Signal b)
        {
            if (a.ChannelCount != b.ChannelCount || a.SampleFrequency != b.SampleFrequency) throw new ArgumentException("Cannot combine signals with different channels or sample rates.");
            var channels = new double[a.ChannelCount][];
            var direct = new int[a.ChannelCount];
            for (int c=0; c<channels.Length; c++) {
                channels[c] = new double[Math.Max(a.Count,b.Count)];
                for (int i=0;i<a.Count;i++) channels[c][i] += a[c][i];
                for (int i=0;i<b.Count;i++) channels[c][i] += b[c][i];
                direct[c] = Math.Min(a.Direct_Sample[c], b.Direct_Sample[c]);
            }
            return new Audio_Signal(channels,a.SampleFrequency,direct);
        }
        public static Receiver_Bank Bank(Receiver_Bank[] banks, Source source, Scene scene, int index, int count)
        {
            if (banks.Length != 1 && banks.Length != count) throw new ArgumentException("Provide one receiver bank or one bank per source.");
            var bank = banks.Length == 1 ? banks[0] : banks[index];
            if (bank == null) throw new ArgumentException("Receiver bank is missing.");
            var copy = bank.Duplicate(source, scene);
            copy.delay_ms = GetDelay(source);
            return copy;
        }
        public static void Octaves(Rhino.Geometry.Interval interval, out int first, out int last)
        {
            first = (int)interval.T0; last = (int)interval.T1;
            if (first < 0 || last > 7 || first > last || first != interval.T0 || last != interval.T1) throw new ArgumentException("Frequency scope must contain integer octave indices from 0 through 7.");
        }
        public static double Angle(double[] values,int receiver,int count) {
            if(values.Length==1) return values[0];
            if(values.Length!=count) throw new ArgumentException("Provide one angle or one angle per receiver.");
            return values[receiver];
        }
        public static Audio_Signal Response(Simulations sims,int s,int r,string kind,int first,int last,double alt,double azi,int degree,int standard)
        {
            if(kind!="Energy" && (first!=0 || last!=7)) throw new ArgumentException("This core response API generates broadband audio. Use scope 0–7, then Octave Filter for individual bands.");
            int fs = kind=="Energy" ? sims.SampleRate(s) : 44100;
            double[][] channels;
            var direct=sims.Direct; var images=sims.Images; var receivers=sims.Receivers;
            var sources=new List<int>{s};
            if(kind=="Energy") {
                channels=new double[last-first+1][];
                for(int o=first;o<=last;o++) channels[o-first]=Pachyderm_Acoustic.Utilities.IR_Construction.ETCurve(direct[s],images[s],receivers[s],sims.Cutoff(s),fs,o,r,false);
            } else {
                sims.Filters(s);
                if(kind=="Pressure") channels=new[]{Pachyderm_Acoustic.Utilities.IR_Construction.PressureTimeCurve(direct,images,receivers,sims.Cutoff(s),fs,r,sources,false,true)};
                else if(kind=="Stereo") {
                    channels=new double[2][];
                    channels[0]=Pachyderm_Acoustic.Utilities.IR_Construction.AurFilter_Directional(direct,images,receivers,sims.Cutoff(s),fs,8,r,sources,false,alt,azi-45,true,true);
                    channels[1]=Pachyderm_Acoustic.Utilities.IR_Construction.AurFilter_Directional(direct,images,receivers,sims.Cutoff(s),fs,8,r,sources,false,alt,azi+45,true,true);
                } else if(kind=="Ambisonic" && degree>0) {
                    var ordering=(Pachyderm_Acoustic.Utilities.IR_Construction.Ambisonics_Component_Order)standard;
                    if(degree==1) channels=Pachyderm_Acoustic.Utilities.IR_Construction.AurFilter_Fig8_3Axis(direct,images,receivers,sims.Cutoff(s),fs,r,sources,false,alt,azi,true,true,ordering);
                    else if(degree==2) channels=Pachyderm_Acoustic.Utilities.IR_Construction.AurFilter_Ambisonics2(direct,images,receivers,sims.Cutoff(s),fs,r,sources,false,alt,azi,true,true,ordering);
                    else channels=Pachyderm_Acoustic.Utilities.IR_Construction.AurFilter_Ambisonics3(direct,images,receivers,sims.Cutoff(s),fs,r,sources,false,alt,azi,true,true,ordering);
                } else channels=new[]{Pachyderm_Acoustic.Utilities.IR_Construction.Auralization_Filter(direct,images,receivers,sims.Cutoff(s),fs,r,sources,false,true)};
            }
            return new Audio_Signal(channels,fs,Enumerable.Repeat(sims.Arrival(s,r,fs),channels.Length).ToArray());
        }
        internal sealed class Simulations
        {
            private readonly HashSet<int> prepared = new HashSet<int>();
            private static readonly object filterGate = new object();
            public DS[] Direct; public IS[] Images; public Receiver_Bank[] Receivers;
            public int Count => Direct.Length;
            public Simulations(IDataAccess access)
            {
                Direct=Items<DS>(access,0); Images=Items<IS>(access,1); Receivers=Items<Receiver_Bank>(access,2);
                int count=Math.Max(Direct.Length,Math.Max(Images.Length,Receivers.Length));
                if(count==0) throw new ArgumentException("Provide simulation results.");
                Direct=Align(Direct,count); Images=Align(Images,count); Receivers=Align(Receivers,count);
                for(int s=0;s<count;s++) if(Direct[s]==null && Receivers[s]==null) throw new ArgumentException("Each source needs direct sound or receiver results.");
            }
            static T[] Align<T>(T[] values,int count) {
                if(values.Length==0) return new T[count];
                if(values.Length!=count) throw new ArgumentException("Simulation lists must align by source; missing simulations may be omitted or represented by null entries.");
                return values;
            }
            public int ReceiverCount(int s) => Receivers[s]?.Count ?? Direct[s].rec_count;
            public int SampleRate(int s) => Receivers[s]?.SampleRate ?? 44100;
            public double Cutoff(int s) => Receivers[s]?.CutOffTime ?? 1000;
            public int Arrival(int s,int r,int fs) => Direct[s]==null ? 0 : (int)Math.Round(Direct[s].Time(r)*fs);
            public void Filters(int s) {
                lock(filterGate) {
                if(prepared.Contains(s)) return;
                Direct[s]?.Create_Filter();
                if(Images[s]!=null) {
                    if(Direct[s]==null) throw new ArgumentException("Image-source filters require the corresponding direct-sound result.");
                    Images[s].Create_Filter(Direct[s].SWL,16384);
                }
                if(Receivers[s]!=null && !Receivers[s].HasFilter()) Receivers[s].Create_Filter();
                prepared.Add(s);
                }
            }
        }
    }
}
