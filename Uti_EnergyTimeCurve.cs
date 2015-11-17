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
using System.Collections.Generic;
using System.Windows.Forms;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace PachydermGH
{
    public class EnergyTimeCurve : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent2 class.
        /// </summary>
        public EnergyTimeCurve()
            : base("Energy-Time Curve", "ETC",
                "Creates the Energy-Time Curve from simulation results",
                "Acoustics", "UTilities")
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
            pManager.AddGenericParameter("Energy-Time Curve", "ETC", "The energy-time-curve result of the simulation...", GH_ParamAccess.item);
        }

        public override bool AppendMenuItems(ToolStripDropDown menu)
        {
            Menu_AppendItem(menu, "Sum all ETCs.", Combine_Click, true, Combine);
            return base.AppendMenuItems(menu);
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

            List<Audio_Signal> AS_final = new List<Audio_Signal>();
            for (int i = 0; i < max; i++)
            {
                List<Audio_Signal> AS = new List<Audio_Signal>();
                for (int r = 0; r < Rec[i].Rec_List.Length; r++)
                {
                    float[][] S = new float[(int)Math.Abs(Oct.T1 - Oct.T0 + 1)][];

                    for (int o = (int)Oct.T0; o <= Oct.T1; o++)
                    {
                        double[] ETC = Pachyderm_Acoustic.Utilities.AcousticalMath.ETCurve(D[i], IS[i], Rec[i], Rec[i].CutOffTime, Rec[i].SampleRate, o, r, false);
                        float[] ETCf = new float[ETC.Length];
                        for (int j = 0; j < ETC.Length;j++) ETCf[i] = (float)ETC[j];
                        S[(int)(o - Oct.T0)] = ETCf;
                    }
                    AS.Add(new Audio_Signal(S, Rec[0].SampleRate));
                }
                if (AS_final.Count == 0) AS_final.AddRange(AS);
                else if (Combine)
                {
                    for (int r = 0; r < Rec[i].Rec_List.Length; r++)
                    {
                        AS_final[r] += AS[r];
                    }
                }
            }
            DA.SetDataList(0, AS_final);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return null;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{D6198179-44DC-4DF9-8B4C-7DC35C268E8B}"); }
        }
    }
}