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

using Grasshopper2.Components;
using Grasshopper2.Data;
using Grasshopper2.Parameters;
using Grasshopper2.UI;
using Grasshopper2.UI.Icon;
using GrasshopperIO;
using Pachyderm_Acoustic;
using Pachyderm_Acoustic.Environment;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
    
namespace PachydermGH
{

    [IoId("EEF8BC75-1451-4486-975E-C6F3DE488FF4")]
    public class LF_ETC : Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent2 class.
        /// </summary>
        public LF_ETC()
            : base(new Nomen("Lateral Fraction",
                "Computes Lateral Fraction from Energy Time Curve",
                "Acoustics", "Analysis"))
        {
        }
        public LF_ETC(IReader reader) : base(reader) { }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void AddInputs(InputAdder inputs)
        {
            inputs.AddGeneric("Direct Sound", "D", "Plug the Direct Sound in here.", Access.Tree);
            inputs.AddGeneric("Image Source", "IS", "Plug the Image Source in here.", Access.Tree);
            inputs.AddGeneric("Ray Tracing", "Tr", "Plug the Receiver from Ray Tracing in here.", Access.Tree);
            inputs.AddNumber("Altitude", "Alt", "Euler altitude angle.", Access.Tree);
            inputs.AddNumber("Azimuth", "Azi", "Euler azimuth angle.", Access.Tree);
            inputs.AddInterval("Frequency Scope", "Oct", "An interval of the first and last octave to calculate (0 = 62.5 Hz, 1 = 125 HZ., ..., 7 = 8000 Hz.", Access.Item);

            inputs[1].Requirement = Requirement.MayBeMissing | Requirement.MayBeNull;
            inputs[2].Requirement = Requirement.MayBeMissing | Requirement.MayBeNull;
            inputs[3].Requirement = Requirement.MayBeMissing | Requirement.MayBeNull;
            inputs[4].Requirement = Requirement.MayBeMissing | Requirement.MayBeNull;
            inputs[5].Requirement = Requirement.MayBeMissing | Requirement.MayBeNull;
       }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void AddOutputs(OutputAdder outputs)
        {
            outputs.AddNumber("Lateral Fraction", "LF", "Lateral Fraction", Access.Tree);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void Process(IDataAccess access)
        {
            Grasshopper2.Data.Tree<Pachyderm_Acoustic.Direct_Sound> D_T;
            access.GetTree<Pachyderm_Acoustic.Direct_Sound>(0, out D_T);
            Grasshopper2.Data.Tree<Pachyderm_Acoustic.ImageSourceData> IS_T;
            access.GetTree<Pachyderm_Acoustic.ImageSourceData>(1, out IS_T);
            Grasshopper2.Data.Tree<Pachyderm_Acoustic.Environment.Receiver_Bank> Rec_T;
            access.GetTree<Pachyderm_Acoustic.Environment.Receiver_Bank>(2, out Rec_T);
            Grasshopper2.Data.Tree<double> t_alt, t_azi;
            if (!(access.GetTree<double>(3, out t_alt) && access.GetTree<double>(4, out t_azi))) return;

            List<double> alt = new List<double>(t_alt.AllItems);
            List<double> azi = new List<double>(t_azi.AllItems);
            List<Receiver_Bank> Rec = new List<Receiver_Bank>(Rec_T.AllItems);
            List<ImageSourceData> IS = new List<ImageSourceData>(IS_T.AllItems);
            List<Pachyderm_Acoustic.Direct_Sound> D = new List<Pachyderm_Acoustic.Direct_Sound>(D_T.AllItems);

            if (alt.Count != 0)
            {
                if (alt.Count != azi.Count) throw new Exception("Incomplete altitude/azimuth pairs...");
                if (alt.Count != Rec.Count) throw new Exception("Must specify an altitude/azimuth pair for each receiver...");
            }
            else
            {
                for (int i = 0; i < Rec.Count; i++)
                {
                    double ALT, AZI;
                    Pachyderm_Acoustic.Utilities.PachTools.World_Angles(D[0].Src.Origin, Rec[0].Origin(i), true, out ALT, out AZI);
                    alt.Add(ALT);
                    azi.Add(AZI);
                }
            }
            if (D.Count != 1) throw new Exception("Altitude and Azimuth must be specified if using more than one source...");
            Interval Oct = new Interval(0, 7);
            access.GetItem<Interval>(5, out Oct);

            int max = Math.Max(D.Count, Rec.Count);
            if (D.Count == 0) for (int i = 0; i < max; i++) D.Add(null);
            if (IS.Count == 0) for (int i = 0; i < max; i++) IS.Add(null);
            if (Rec.Count == 0) for (int i = 0; i < max; i++) Rec.Add(null);

            double[][][] LF_final = new double[max][][];

            for (int s = 0; s < max; s++)
            {
                for (int r = 0; r < Rec[s].Rec_List.Length; r++)
                {
                    List<double> LF = new List<double>();
                    double Alt = (double)alt[r];
                    double Azi = (double)azi[r];
                    if (Alt > 90) Alt -= 180;
                    if (Alt < -90) Alt += 180;
                    if (Azi > 360) Azi -= 360;
                    if (Azi < 0) Azi += 360;

                    double[][] S = new double[(int)Math.Abs(Oct.T1 - Oct.T0 + 1)][];
                    int[] direct = new int[(int)Oct.T1 - (int)Oct.T0 + 1];
                    for (int o = (int)Oct.T0; o <= Oct.T1; o++)
                    {
                        double[] ETC = Pachyderm_Acoustic.Utilities.IR_Construction.ETCurve(D[s], IS[s], Rec[s], Rec[s].CutOffTime, Rec[s].SampleRate, o, r, false);
                        double[] LETC = Pachyderm_Acoustic.Utilities.IR_Construction.ETCurve_1d_Tight(D[s], IS[s], Rec[s], Rec[s].CutOffTime, Rec[s].SampleRate, o, r, false, Alt, Azi, true)[1];
                        S[(int)(o - Oct.T0)] = ETC;
                        direct[(int)(o - Oct.T0)] = (int)Math.Round(D[s].Time(r) * Rec[s].SampleRate);
                        LF.Add(Pachyderm_Acoustic.Utilities.AcousticalMath.Lateral_Fraction(ETC, LETC, Rec[s].SampleRate, (double)direct[(int)(o - Oct.T0)] / (double)Rec[s].SampleRate, false));
                    }

                    if (s == 0 && r == 0)
                    {
                        LF_final[s] = new double[Rec[s].Rec_List.Length][];
                        LF_final[s][r] = LF.ToArray();
                    }
                    else
                    {
                        LF_final[s][r] = LF.ToArray();
                    }
                }
            }
            access.SetTree(0, Garden.TreeFromArrays<double[]>(LF_final));
        }

        protected override IIcon IconInternal
        {
            get
            {
                var assembly = typeof(SPLETC).Assembly;
                var resourceName = "Pachyderm_GH.Icons.Geodesic_Source.png";

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
        //protected override IIcon IconInternal => new Grasshopper2.UI.Icon.PixelIcon("Geodesic_Source");
    }
}