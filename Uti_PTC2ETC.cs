//'Pachyderm-Acoustic: Geometrical Acoustics for Rhinoceros (GPL) by Arthur van der Harten 
//' 
//'This file is part of Pachyderm-Acoustic. 
//' 
//'Copyright (c) 2008-2024, Arthur van der Harten 
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
//using System.Windows.Forms;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace PachydermGH
{
    public class PTC2ETC : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent2 class.
        /// </summary>
        public PTC2ETC()
            : base("Energy-Time Curve from Impulse Response", "IR-2-ETC",
                "Creates the Energy-Time Curve from an impulse response, measured or simulated",
                "Acoustics", "Utility")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Impulse Response", "IR", "Plug the audio signal impulse response in here.", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Energy-Time Curve", "ETC", "The energy-time-curve result of conversion...", GH_ParamAccess.item);
        }

        //public override bool AppendMenuItems(ToolStripDropDown menu)
        //{
        //    Menu_AppendItem(menu, "Sum all ETCs.", Combine_Click, true, Combine);
        //    return base.AppendMenuItems(menu);
        //}

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
            Audio_Signal IR = new Audio_Signal();
            DA.GetData<Audio_Signal>(0, ref IR);

            List<Audio_Signal> AS_final = new List<Audio_Signal>();

            double[][] signal = new double[8][];
            for (int i = 0; i < IR.ChannelCount; i++) signal[i] = new double[IR.Count];

            for (int j = 0; j < IR.ChannelCount; j++)
            {
                for (int oct = 0; oct < 8; oct++)
                {
                    double[] IR_oct = Pachyderm_Acoustic.Audio.Pach_SP.FIR_Bandpass(IR[j], oct, IR.SampleFrequency, 0);
                    signal[oct] = new double[IR_oct.Length];
                    for (int i = 0; i < IR_oct.Length; i++)
                    {
                        signal[oct][i] = IR_oct[i] * IR_oct[i];
                    }
                }

                AS_final.Add(new Audio_Signal(signal, IR.SampleFrequency, IR.Direct_Sample));
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
            get { return new Guid("{2F45F41C-3027-4FE1-8071-26495398C06B}"); }
        }
    }
}