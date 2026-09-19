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
using System.Collections.Generic;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace PachydermGH
{
    public class LF_ETC : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent2 class.
        /// </summary>
        public LF_ETC()
            : base("Lateral Fraction", "LF",
                "Computes Lateral Fraction from Energy Time Curve",
                "Acoustics", "Analysis")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Direct Sound", "D", "Plug the Direct Sound in here.", GH_ParamAccess.list);
            pManager.AddGenericParameter("Image Source", "IS", "Plug the Image Source in here.", GH_ParamAccess.list);
            pManager.AddGenericParameter("Ray Tracing", "Tr", "Plug the Receiver from Ray Tracing in here.", GH_ParamAccess.list);
            pManager.AddNumberParameter("Altitude", "Alt", "Euler altitude angle.", GH_ParamAccess.list);
            pManager.AddNumberParameter("Azimuth", "Azi", "Euler azimuth angle.", GH_ParamAccess.list);
            pManager.AddIntervalParameter("Frequency Scope", "Oct", "An interval of the first and last octave to calculate (0 = 62.5 Hz, 1 = 125 HZ., ..., 7 = 8000 Hz.", GH_ParamAccess.item);

            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;
            pManager[4].Optional = true;
            pManager[5].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Lateral Fraction", "LF", "Lateral Fraction", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Pachyderm_Acoustic.Direct_Sound> D = new List<Pachyderm_Acoustic.Direct_Sound>();
            DA.GetDataList<Pachyderm_Acoustic.Direct_Sound>(0, D);
            List<Pachyderm_Acoustic.ImageSourceData> IS = new List<Pachyderm_Acoustic.ImageSourceData>();
            DA.GetDataList<Pachyderm_Acoustic.ImageSourceData>(1, IS);
            List<Pachyderm_Acoustic.Environment.Receiver_Bank> Rec = new List<Pachyderm_Acoustic.Environment.Receiver_Bank>();
            DA.GetDataList<Pachyderm_Acoustic.Environment.Receiver_Bank>(2, Rec);
            List<double> alt = new List<double>(), azi = new List<double>();
            if (!(DA.GetDataList<double>(3, alt) && DA.GetDataList<double>(4, azi))) return;
            if (alt.Count != 0)
            {
                if (alt.Count != azi.Count) throw new Exception("Incomplete altitude/azimuth pairs...");
                if (alt.Count != Rec.Count) throw new Exception("Must specify an altitude/azimuth pair for each receiver...");
            }
            else 
            {
                for(int i = 0; i < Rec.Count; i++)
                {
                    double ALT, AZI;
                    Pachyderm_Acoustic.Utilities.PachTools.World_Angles(D[0].Src.Origin, Rec[0].Origin(i), true, out ALT, out AZI);
                    alt.Add(ALT);
                    azi.Add(AZI);
                }
            }
            if (D.Count != 1) throw new Exception("Altitude and Azimuth must be specified if using more than one source...");
            Interval Oct = new Interval(0, 7);
            DA.GetData<Interval>(5, ref Oct);

            int max = Math.Max(D.Count, Rec.Count);
            if (D.Count == 0) for (int i = 0; i < max; i++) D.Add(null);
            if (IS.Count == 0) for (int i = 0; i < max; i++) IS.Add(null);
            if (Rec.Count == 0) for (int i = 0; i < max; i++) Rec.Add(null);
            
            Grasshopper.DataTree<double> LF_final = new Grasshopper.DataTree<double>();

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
                        LF_final = new Grasshopper.DataTree<double>(LF, new Grasshopper.Kernel.Data.GH_Path(new int[2] { 0, 0 }));
                    }
                    else
                    {
                        int[] path = new int[2] { s, r };
                        LF_final.EnsurePath(path);
                        LF_final.AddRange(LF, new Grasshopper.Kernel.Data.GH_Path(path));
                    }
                }
            }
            DA.SetDataTree(0, LF_final);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                System.Drawing.Bitmap b = Properties.Resources.Geodesic_Source;
                b.MakeTransparent(System.Drawing.Color.White);
                return b;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("EEF8BC75-1451-4486-975E-C6F3DE488FF4"); }
        }
    }
}