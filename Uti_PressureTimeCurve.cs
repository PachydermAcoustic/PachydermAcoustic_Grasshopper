﻿//'Pachyderm-Acoustic: Geometrical Acoustics for Rhinoceros (GPL)   
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
using System.Linq;
using Grasshopper.Kernel;
using Pachyderm_Acoustic;
using Rhino.Geometry;

namespace PachydermGH
{
    public class PressureTimeCurve : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent2 class.
        /// </summary>
        public PressureTimeCurve()
            : base("Pressure-Time Curve", "PTC",
                "Creates the Impulse Response from simulation results. Note that this version of the impulse response has the power spectrum of the source, and can be used for SPL predictions, but should not be used for auralization.",
                "Acoustics", "Utility")
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
            pManager.AddIntervalParameter("Frequency Scope", "Oct", "An interval of the first and last octave to calculate (0 = 62.5 Hz, 1 = 125 HZ., ..., 7 = 8000 Hz.", GH_ParamAccess.item);

            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Energy-Time Curve", "ETC", "The energy-time-curve result of the simulation...", GH_ParamAccess.tree);
        }

        bool Combine = true;

        private void Combine_Click(Object sender, EventArgs e)
        {
            Combine = !Combine;
            ExpireSolution(true);
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
            Interval Oct = new Interval(0, 7);
            DA.GetData<Interval>(3, ref Oct);

            int max = Math.Max(D.Count, Rec.Count);
            if (D.Count == 0) for(int i = 0; i < max; i++) D.Add(null);
            if (IS.Count == 0) for (int i = 0; i < max; i++) IS.Add(null);
            if (Rec.Count == 0) for (int i = 0; i < max; i++) Rec.Add(null);

            Grasshopper.DataTree<Audio_Signal> AS_final = new Grasshopper.DataTree<Audio_Signal>();
            List<Audio_Signal> AS_comb = new List<Audio_Signal>();

            for (int s = 0; s < max; s++)
            {
                //Need to create filters?
                //ProgressBox VB = new ProgressBox("Creating Impulse Responses...");

                D[s].Create_Filter();
                if (IS != null && s < IS.Count && IS[s] != null) IS[s].Create_Filter(D[s].SWL, 4096);
                if (Rec != null && Rec[s] != null && !Rec[s].HasFilter()) Rec[s].Create_Filter();

                List<Audio_Signal> AS = new List<Audio_Signal>();
                //for (int r = 0; r < Rec[s].Rec_List.Length; r++)
                for (int r = 0; r < D[s].rec_count; r++)
                {
                    double COTime = Rec[s] != null ? Rec[s].CutOffTime : 1000;
                    int FS = Rec[s] != null ? Rec[s].SampleRate : 44100;
                    double[] PTC = Pachyderm_Acoustic.Utilities.IR_Construction.PressureTimeCurve(D.ToArray(), IS.ToArray(), Rec.ToArray(),  COTime, FS, r, new List<int> { s }, false, true);
                    //VB.Close();
                    AS.Add(new Audio_Signal(PTC, FS, (int)Math.Round(D[s].Time(r) * FS)));
                }

                if (s == 0)
                {
                    if (Combine)
                    {
                        AS_comb = AS;
                        AS_final = new Grasshopper.DataTree<Audio_Signal>(AS, new Grasshopper.Kernel.Data.GH_Path(0));
                    }
                    else
                    {
                        AS_final = new Grasshopper.DataTree<Audio_Signal>(AS, new Grasshopper.Kernel.Data.GH_Path(max));
                    }
                }
                else if (Combine)
                {
                    for (int r = 0; r < Rec[s].Rec_List.Length; r++)
                    {
                        AS_comb[r] += AS[r];
                    }
                    AS_final = new Grasshopper.DataTree<Audio_Signal>(AS, new Grasshopper.Kernel.Data.GH_Path(0));
                }
                else
                {
                    AS_final.EnsurePath(new int[1] { s });
                    AS_final.AddRange(AS, new Grasshopper.Kernel.Data.GH_Path(s));
                }
            }
            DA.SetDataTree(0, AS_final);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                System.Drawing.Bitmap b = Properties.Resources.Energy_Time_Curve;
                b.MakeTransparent(System.Drawing.Color.White);
                return b;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{6DB47BF7-B4C5-410F-8155-88434902BE4A}"); }
        }
    }
}