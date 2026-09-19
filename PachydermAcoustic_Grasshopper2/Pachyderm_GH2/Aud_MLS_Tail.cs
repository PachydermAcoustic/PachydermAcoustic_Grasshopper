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
using Grasshopper2.Data;
using System.Linq;

namespace PachydermGH
{
    [IoId("89ebd299-d562-41b4-91bd-0b17a3253c15")]
    public class MLS_Tail : Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public MLS_Tail()
          : base(new Nomen("Diffuse MLS Tail",
              "Dr. Ning Xiang sent AvH a paper concerning constructing a reverberant tail using MLS based signals. This node is the result.",
              "Acoustics", "Audio"))
        {
            Threading = Grasshopper2.Components.ThreadingState.SingleThreaded;
        }

        public MLS_Tail(IReader reader) : base(reader) { Threading = Grasshopper2.Components.ThreadingState.SingleThreaded; }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void AddInputs(InputAdder inputs)
        {
            inputs.AddNumber("Reverberation Time", "RT", "Reverberation time in seconds. Specify 8 values (for octave bands 63 - 8k).", Access.Tree);
            inputs.AddNumber("Direct to Reverberant Ratio", "D2R", "Dictates the reverberant level of the output response, as a function of the direct sound power. Specify 8 values (for octave bands 63 - 8k).", Access.Tree);
            inputs.AddNumber("Sampling Frequency", "FS", "The number of samples per second (sampling frequency). 44100 hz. default.", Access.Item).Set(44100.0);
            inputs.AddNumber("Duration (milliseconds)", "D_ms", "Impulse Response length in milliseconds. 1000 ms. default", Access.Item).Set(1000.0);
            inputs.AddGeneric("Direct Sound", "D", "Input the direct sound simulation. (optional) If no direct sound is added, a value of 1 is assumed for the direct intensity.", Access.Item, Requirement.MayBeMissing);
            inputs.AddGeneric("Image Source", "IS", "Input the image-source simulation. (optional)", Access.Item, Requirement.MayBeMissing);
            inputs[4].Requirement = Requirement.MayBeMissing;
            inputs[5].Requirement = Requirement.MayBeMissing;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void AddOutputs(OutputAdder outputs)
        {
            outputs.AddGeneric("Impulse Response", "IR", "The artificial reverberant tail based on MLS noise...", Access.Item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void Process(IDataAccess access)
        {
            var rt=ComponentSupport.Items<double>(access,0); var ratio=ComponentSupport.Items<double>(access,1);
            access.GetItem<double>(2,out double fsValue); access.GetItem<double>(3,out double duration);
            if(rt.Length!=8 || ratio.Length!=8 || rt.Any(x=>x<=0) || fsValue<=0 || duration<=0) throw new ArgumentException("Provide eight positive decay times, eight ratios, and positive sample rate/duration.");
            int fs=checked((int)fsValue);
            access.GetItem<Pachyderm_Acoustic.Direct_Sound>(4,out var direct);
            access.GetItem<Pachyderm_Acoustic.ImageSourceData>(5,out var images);
            if(images!=null && direct==null) throw new ArgumentException("Image-source input requires direct sound.");
            var magnitude=new double[8];
            for(int oct=0;oct<8;oct++) magnitude[oct]=Math.Sqrt((direct==null?1.0:direct.EnergyValue(oct,0).Sum())*Math.Pow(10,ratio[oct]/10));
            double dt=direct?.Time(0) ?? 0;
            var response=new double[(int)Math.Ceiling(duration*fs/1000.0)+16384];
            if(direct!=null) {
                direct.Create_Filter(); images?.Create_Filter(direct.SWL,16384);
                response=Pachyderm_Acoustic.Utilities.IR_Construction.Auralization_Filter(new[]{direct},new[]{images},null,duration,fs,0,new List<int>{0},false,true);
            }
            var tail=Pachyderm_Acoustic.Audio.Pach_SP.MLS_Reverb(duration/1000.0,rt,fs,magnitude);
            double onset=dt+0.007;
            if(images!=null && images.Paths.Length>0) {
                var early=images.Paths[0].Where(path=>path.Path[0].Length<=3 && path.TravelTime>0.010).Select(path=>path.TravelTime).ToArray();
                if(early.Length>0) onset=Math.Min(dt+0.1,early.Min());
            }
            int start=Math.Max(0,(int)Math.Round(onset*fs));
            if(start>response.Length) throw new ArgumentException("Reverberant onset exceeds response duration.");
            for(int i=0;i<Math.Min(tail.Length,response.Length-start);i++) response[start+i]+=tail[i];
            access.SetItem(0,new Audio_Signal(response,fs,(int)Math.Round(dt*fs)));
        }
    }
}
