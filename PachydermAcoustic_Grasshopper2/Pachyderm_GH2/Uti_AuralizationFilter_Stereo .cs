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
    [IoId("44E35BD4-1AB4-4D73-83F5-CA17C5E02A29")]
    public class Stereo_ImpulseResponse : Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent2 class.
        /// </summary>
        public Stereo_ImpulseResponse()
            : base(new Nomen( "StereoIR",
                "Creates the Stereo Impulse Response from simulation results. Note that this version of the impulse response has a flat power spectrum, and can be used for auralizations, but should not be used for sound pressure level predictions.",
                "Acoustics", "Utility"))
        {
        }

        public Stereo_ImpulseResponse(IReader reader) : base(reader) { }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void AddInputs(InputAdder inputs)
        {
            inputs.AddGeneric("Direct Sound", "D", "Plug the Direct Sound in here.", Access.Tree);
            inputs.AddGeneric("Image Source", "IS", "Plug the Image Source in here.", Access.Tree);
            inputs.AddGeneric("Ray Tracing", "Tr", "Plug the Receiver from Ray Tracing in here.", Access.Tree);
            inputs.AddNumber("Altitude", "Alt", "Euler altitude angle.", Access.Item);
            inputs.AddNumber("Azimuth", "Azi", "Euler azimuth angle.", Access.Item);
            inputs.AddInterval("Frequency Scope", "Oct", "An interval of the first and last octave to calculate (0 = 62.5 Hz, 1 = 125 HZ., ..., 7 = 8000 Hz.", Access.Item);

            inputs[1].Requirement = Requirement.MayBeMissing;
            inputs[2].Requirement = Requirement.MayBeMissing;
            inputs[3].Requirement = Requirement.MayBeMissing;
            inputs[4].Requirement = Requirement.MayBeMissing;
            inputs[5].Requirement = Requirement.MayBeMissing;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void AddOutputs(OutputAdder outputs)
        {
            outputs.AddGeneric("Energy-Time Curve", "ETC", "The energy-time-curve result of the simulation...", Access.Tree);
        }


        //public override bool AppendMenuItems(ToolStripDropDown menu)
        //{
        //    Menu_AppendItem(menu, "Sum all ETCs.", Combine_Click, true, Combine);
        //    return base.AppendMenuItems(menu);
        //}

        bool Combine = true;

        //private void Combine_Click(Object sender, EventArgs e)
        //{
        //    Combine = !Combine;
        //    ExpireSolution(true);
        //}

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="access">The access object is used to retrieve from inputs and store in outputs.</param>
        protected override void Process(IDataAccess access)
        {
            Tree<Pachyderm_Acoustic.Direct_Sound> D_T;
            access.GetTree<Pachyderm_Acoustic.Direct_Sound>(0, out D_T);
            Tree<Pachyderm_Acoustic.ImageSourceData> IS_T;
            access.GetTree<Pachyderm_Acoustic.ImageSourceData>(1, out IS_T);
            Tree<Pachyderm_Acoustic.Environment.Receiver_Bank> Rec_T;
            access.GetTree<Pachyderm_Acoustic.Environment.Receiver_Bank>(2, out Rec_T);
            double alt = 0;
            double azi = 0;

            bool ealt, eazi;
            ealt = access.GetItem<double>(3, out alt);
            eazi = access.GetItem<double>(4, out azi);
            
            if ( (!ealt | !eazi) )
            {
                if (ealt && eazi)
                {
                    throw new Exception("In order to specify an angle, both azimuth and altitude must be specified. Are you missing a parameter?");
                }
                else
                {

                }
            }
            Interval Oct = new Interval(0, 7);
            access.GetItem<Interval>(5, out Oct);

            List<Pachyderm_Acoustic.Direct_Sound> D = new List<Pachyderm_Acoustic.Direct_Sound>();
            List<ImageSourceData> IS = new List<ImageSourceData>();
            List<Receiver_Bank> Rec = new List<Receiver_Bank>();

            int max = Math.Max(D.Count, Rec_T.ItemCount);
            if (D_T.ItemCount == 0) for (int i = 0; i < max; i++) D.Add(null);
            if (IS_T.ItemCount == 0) for (int i = 0; i < max; i++) IS.Add(null);
            if (Rec_T.ItemCount == 0) for (int i = 0; i < max; i++) Rec.Add(null);

            List<Audio_Signal> AS_final = new List<Audio_Signal>();
            List<Audio_Signal> AS_comb = new List<Audio_Signal>();

            for (int s = 0; s < max; s++)
            {
                //Need to create filters?
                if (!Rec[s].HasFilter()) Rec[s].Create_Filter(null);
                while (!Rec[s].HasFilter()) System.Threading.Thread.Sleep(500);

                List<Audio_Signal> AS = new List<Audio_Signal>();
                for (int r = 0; r < Rec[s].Rec_List.Length; r++)
                {
                    //ProgressBox VB = new ProgressBox("Creating Impulse Responses...");
                    //VB.Show();
                    D_T.Items[s].Get_Filter(r, 44100);

                    double[][] Response = new double[2][];
                    int[] DT = new int[2];
                    for (int i = 0; i < 2; i++)
                    {
                        double Alt = -(double)alt + 180 * Math.Asin(0) / Math.PI;
                        double Azi = (double)alt + 180 * Math.Atan2(Math.Pow(-1, i+2) * 1 / Math.Sqrt(2), -1 / Math.Sqrt(2)) / Math.PI;
                        if (alt > 90) alt -= 180;
                        if (alt < -90) alt += 180;
                        if (azi > 360) azi -= 360;
                        if (azi < 0) azi += 360;
                        Response[i] = Pachyderm_Acoustic.Utilities.IR_Construction.AurFilter_Directional(D, IS, Rec, Rec[s].CO_Time, 44100, 8, r, new List<int> { s }, false, Alt, Azi, true, true);//, VB);
                        DT[i] = (int)Math.Round(D_T.Items[s].Time(r) * Rec[s].SampleRate);
                    }

                    //double[] AFTC = Pachyderm_Acoustic.Utilities.IR_Construction.Auralization_Filter(D.ToArray(), IS.ToArray(), Rec.ToArray(), Rec[s].CutOffTime, Rec[s].SampleRate, r, new List<int> { s }, false, true, VB);
                    //VB.Close();
                    AS.Add(new Audio_Signal(Response, Rec[0].SampleRate, DT));
                }

                if (s == 0)
                {
                    if (Combine)
                    {
                        AS_comb = AS;
                        AS_final = AS;
                    }
                    else
                    {
                        AS_final = AS;
                    }
                }
                else if (Combine)
                {
                    for (int r = 0; r < Rec[s].Rec_List.Length; r++)
                    {
                        AS_comb[r] += AS[r];
                    }
                    AS_final = AS;
                }
                else
                {
                    AS_final.AddRange(AS);
                }
            }
            access.SetTree(0, Garden.TreeFromList(AS_final));
        }
        protected override IIcon IconInternal
        {
            get
            {
                var assembly = typeof(SPLETC).Assembly;
                var resourceName = "Pachyderm_GH.Icons.Energy_Time_Curve.png";

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
        //protected override IIcon IconInternal => new Grasshopper2.UI.Icon.PixelIcon("Energy_Time_Curve");
    }
}