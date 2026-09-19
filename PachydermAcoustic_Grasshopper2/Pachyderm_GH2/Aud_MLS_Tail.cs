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
        }

        public MLS_Tail(IReader reader) : base(reader) { }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void AddInputs(InputAdder inputs)
        {
            inputs.AddNumber("Reverberation Time", "RT", "Reverberation time in seconds. Specify 8 values (for octave bands 63 - 8k).", Access.Tree);
            inputs.AddNumber("Direct to Reverberant Ratio", "D2R", "Dictates the reverberant level of the output response, as a function of the direct sound power. Specify 8 values (for octave bands 63 - 8k).", Access.Tree);
            inputs.AddNumber("Sampling Frequency", "FS", "The number of samples per second (sampling frequency). 44100 hz. default.", Access.Item);
            inputs.AddNumber("Duration (milliseconds)", "D_ms", "Impulse Response length in milliseconds. 1000 ms. default", Access.Item);
            inputs.AddGeneric("Direct Sound", "D", "Input the direct sound simulation. (optional) If no direct sound is added, a value of 1 is assumed for the direct intensity.", Access.Item);
            inputs.AddGeneric("Image Source", "IS", "Input the image-source simulation. (optional)", Access.Item);
            inputs[4].Requirement = Requirement.MayBeMissing;
            inputs[5].Requirement = Requirement.MayBeMissing;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void AddOutputs(OutputAdder outputs)
        {
            outputs.AddGeneric("Impulse Response", "IR", "The artificial reverberant tail based on MLS noise...", Access.Tree);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void Process(IDataAccess access)
        {
            Tree<double> RT_T;
            access.GetTree<double>(0, out RT_T);
            List<double> RT = new List<double>(RT_T.AllItems);
            Tree<double> D2R_T;
            access.GetTree<double>(1, out D2R_T);
            List<double> D2R = new List<double>(D2R_T.AllItems);
            double FS = 0;
            access.GetItem<double>(2, out FS);
            double Dur = 0;
            access.GetItem<double>(3, out Dur);
            Pachyderm_Acoustic.Direct_Sound Dir = default;
            access.GetItem<Pachyderm_Acoustic.Direct_Sound>(4, out Dir);
            Pachyderm_Acoustic.ImageSourceData IS = default;
            access.GetItem<Pachyderm_Acoustic.ImageSourceData>(5, out IS);

            Dir.Create_Filter();

            ProgressBox VB = new ProgressBox("Creating IR Filters for Deterministic Reflections...");
            VB.Show();
            if (IS != null) IS.Create_Filter(Dir.SWL, 16384, VB);
            VB.Close();

            Pachyderm_Acoustic.Environment.Receiver_Bank[] rec = new Pachyderm_Acoustic.Environment.Receiver_Bank[1];

            double[] magnitude = new double[8] {1,1,1,1,1,1,1,1};
            if (Dir != null) { for (int i = 0; i < magnitude.Length; i++) { magnitude[i] = Math.Sqrt(Dir.EnergyValue(i, 0).Sum() * Math.Pow(10, D2R[0] / 10)); } }

            VB = new Pachyderm_Acoustic.ProgressBox("Extrapolating Filter...");
            VB.Show();
            double[] AF = Pachyderm_Acoustic.Utilities.IR_Construction.Auralization_Filter(new Pachyderm_Acoustic.Direct_Sound[1] { Dir }, new Pachyderm_Acoustic.ImageSourceData[1] { IS }, null, Dur, (int)FS, 0, new List<int> { 0 }, false, true, VB);
            VB.Close();

            double[] Tail = Pachyderm_Acoustic.Audio.Pach_SP.MLS_Reverb((double)Dur/1000d, RT.ToArray(), (int)FS, magnitude);
            double dt = Dir.Time(0);

            double t = 0.1 + dt;

            if (IS != null && IS.Paths.Length > 0)
            {
                //Find the first 1st order sidewall reflection...
                for (int i = 0; i < IS.Paths.Length; i++)
                {
                    if (IS.Paths[0][i].Path[0].Length > 3) { continue; }
                    if (IS.Paths[0][i].TravelTime > 0.010 && IS.Paths[0][i].TravelTime < t) t = IS.Paths[0][i].TravelTime;
                }
                if (t == 0.1 + dt) t = dt + 0.007;
            }
            else
            {
                t = dt + 0.007;
            }

            int start = (int)(t * FS);
            int end = Math.Min(Tail.Length, AF.Length - start);
            for (int i = start; i < end; i++)
            {
                AF[start + i] += Tail[i];
            }

            access.SetItem(0, new Audio_Signal(AF, (int)FS, (int)(dt * FS)));
        }
    }
}