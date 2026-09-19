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
using Grasshopper2.UI.InputPanel;
using System.Linq;

namespace PachydermGH
{
    [IoId("D6198179-44DC-4DF9-8B4C-7DC35C268E8B")]
    public class EnergyTimeCurve : Component
    {
        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        public EnergyTimeCurve()
            : base(new Nomen("Energy-Time Curve",
                "Creates the Energy-Time Curve from simulation results",
                "Acoustics", "Utility"))
        {
        }

        public EnergyTimeCurve(IReader reader) : base(reader) { }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void AddInputs(InputAdder inputs)
        {
            inputs.AddGeneric("Direct Sound", "D", "Plug the Direct Sound in here.", Access.Tree);
            inputs.AddGeneric("Image Source", "IS", "Plug the Image Source in here.", Access.Tree);
            inputs.AddGeneric("Ray Tracing", "Tr", "Plug the Receiver from Ray Tracing in here.", Access.Tree);
            inputs.AddInterval("Frequency Scope", "Oct", "An interval of the first and last octave to calculate (0 = 62.5 Hz, 1 = 125 HZ., ..., 7 = 8000 Hz.", Access.Item);

            inputs[1].Requirement = Requirement.MayBeMissing;
            inputs[2].Requirement = Requirement.MayBeMissing;
            inputs[3].Requirement = Requirement.MayBeMissing;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void AddOutputs(OutputAdder outputs)
        {
            outputs.AddGeneric("Energy-Time Curve", "ETC", "The energy-time-curve result of the simulation...", Access.Tree);
        }

        public override void AppendToInputPanel(InputPanel panel)
        {
            panel.AddCheck("Sum all ETCs", Combine, Combine_Click);
                //"Sum all ETCs...", Combine_Click);
            base.AppendToInputPanel(panel);
        }

        //public override bool AppendMenuItems(ToolStripDropDown menu)
        //{
        //    Menu_AppendItem(menu, "Sum all ETCs.", Combine_Click, true, Combine);
        //    return base.AppendMenuItems(menu);
        //}

        bool Combine = true;

        private void Combine_Click(bool set)
        {
            Combine = set;
            Document.Solution.ReleaseExpirationBlock();
            this.Expire();
            Document.Solution.Start();
        }

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
            Interval Oct = new Interval(0, 7);
            access.GetItem<Interval>(3, out Oct);

            List<Pachyderm_Acoustic.Direct_Sound> D = new List<Pachyderm_Acoustic.Direct_Sound>();
            List<ImageSourceData> IS = new List<ImageSourceData>();
            List<Receiver_Bank> Rec = new List<Receiver_Bank>();

            int max = Math.Max(D.Count, Rec_T.ItemCount);
            if (D_T.ItemCount == 0) for (int i = 0; i < max; i++) D.Add(null);
            else { D = D_T.AllItems.ToList(); }
            if (IS_T.ItemCount == 0) for (int i = 0; i < max; i++) IS.Add(null);
            else { IS = IS_T.AllItems.ToList(); }
            if (Rec_T.ItemCount == 0) for (int i = 0; i < max; i++) Rec.Add(null);
            else { Rec = Rec_T.AllItems.ToList(); }

            List<Audio_Signal> AS_final = new List<Audio_Signal>();
            List<Audio_Signal> AS_comb = new List<Audio_Signal>();

            for (int s = 0; s < max; s++)
            {
                List<Audio_Signal> AS = new List<Audio_Signal>();
                for (int r = 0; r < Rec[s].Rec_List.Length; r++)
                {
                    double[][] S = new double[(int)Math.Abs(Oct.T1 - Oct.T0 + 1)][];
                    int[] direct = new int[(int)Oct.T1 - (int)Oct.T0 + 1];
                    for (int o = (int)Oct.T0; o <= Oct.T1; o++)
                    {
                        double[] ETC = Pachyderm_Acoustic.Utilities.IR_Construction.ETCurve(D[s], IS[s], Rec[s], Rec[s].CutOffTime, Rec[s].SampleRate, o, r, false);
                        S[(int)(o - Oct.T0)] = ETC;
                        direct[(int)(o - Oct.T0)] = (int)Math.Round(D[s].Time(r) * Rec[s].SampleRate);
                    }
                    AS.Add(new Audio_Signal(S, Rec[0].SampleRate, direct));
                }


                if (s == 0)
                {
                    if (Combine)
                    {
                        AS_comb = AS;
                    }
                }
                
                if (Combine)
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