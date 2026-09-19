////'Pachyderm-Acoustic: Geometrical Acoustics for Rhinoceros (GPL)   
////' 
////'This file is part of Pachyderm-Acoustic. 
////' 
////'Copyright (c) 2008-2025, Open Research in Acoustical Science and Education, Inc. - a 501(c)3 nonprofit 
////'Pachyderm-Acoustic is free software; you can redistribute it and/or modify 
////'it under the terms of the GNU General Public License as published 
////'by the Free Software Foundation; either version 3 of the License, or 
////'(at your option) any later version. 
////'Pachyderm-Acoustic is distributed in the hope that it will be useful, 
////'but WITHOUT ANY WARRANTY; without even the implied warranty of 
////'MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
////'GNU General Public License for more details. 
////' 
////'You should have received a copy of the GNU General Public 
////'License along with Pachyderm-Acoustic; if not, write to the Free Software 
////'Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA. 

//using System;
//using System.Collections.Generic;
//using System.Windows.Forms;
//using Grasshopper.Kernel;
//using Rhino.Geometry;

//namespace PachydermGH
//{
//    public class Measurement : Component
//    {
//        /// <summary>
//        /// Initializes a new instance of the MyComponent2 class.
//        /// </summary>
//        public Measurement()
//            : base(new Nomen("Impulse_Measurement", "IR_Meas",
//                "Measures the impulse response using hardware hooked up to your computer. (Works best with a professional grade interface and a relatively flat omnidirectional microphone.",
//                "Acoustics", "Utility")
//        {
//        }

//        /// <summary>
//        /// Registers all the input parameters for this component.
//        /// </summary>
//        protected override void AddInputs(InputAdder inputs)
//        {
//            inputs.AddInteger("Number of Channels", "Ch#", "The number of microphone/input channels", Access.Item, 1);
//            inputs.AddInteger("Number of Averages", "Av#", "The number of averaged impulse responses taken in a single measurement.", Access.Item, 1);
//            inputs.AddInteger("Sample Frequency", "FS", "The frequency of input and output channels", Access.Item, 44100);
//            inputs.AddNumber("Gain", "G", "A factor indicating the strength of the signal.", Access.Item, 1);
//            inputs.AddNumber("Signal Time", "ST", "The duration of each impulse response.", Access.Item, 1.1);
//        }

//        /// <summary>
//        /// Registers all the output parameters for this component.
//        /// </summary>
//        protected override void AddOutputs(OutputAdder outputs)
//        {
//            inputs.AddGeneric("Impulse Response", "IR", "The Measured impulse responses...", Access.Tree);
//        }

//        public override bool AppendMenuItems(ToolStripDropDown menu)
//        {
//            List<string> input = Pachyderm_Acoustic.Audio.Pach_SP.Measurement.Get_Input_Devices();
//            List<string> output = Pachyderm_Acoustic.Audio.Pach_SP.Measurement.Get_Output_Devices();
//            for (int i = 0; i < input.Count; i++) Menu_AppendItem(menu, i.ToString() + ":" + input[i], input_click, true, i == input_id);
//            for (int i = 0; i < output.Count; i++) Menu_AppendItem(menu, i.ToString() + ":" + output[i], output_click, true, i == output_id);
//            return base.AppendMenuItems(menu);
//        }

//        int input_id = 0, output_id = 0;

//        public void input_click(Object sender, EventArgs e)
//        {
//            input_id = int.Parse(sender.ToString().Split(':')[0]);
//        }

//        public void output_click(Object sender, EventArgs e)
//        {
//            output_id = int.Parse(sender.ToString().Split(':')[0]);
//        }

//        /// <summary>
//        /// This is the method that actually does the work.
//        /// </summary>
//        /// <param name="access">The access object is used to retrieve from inputs and store in outputs.</param>
//        protected override void Process(IdataAccess access)
//        {
//            int NC = 0, NA = 0, FS = 0;
//            double G = 0, ST = 0;
//            access.GetItem(0, out NC);
//            access.GetItem(1, out NA);
//            access.GetItem(2, out FS);
//            access.GetItem(3, out G);
//            access.GetItem(4, out ST);

//            List<Audio_Signal> AS_final = new List<Audio_Signal>();
//            Pachyderm_Acoustic.Audio.Pach_SP.Measurement.IO_Tester test = new Pachyderm_Acoustic.Audio.Pach_SP.Measurement.IO_Tester() { Sample_Frequency = FS, Gain = G, No_of_Averages = NA, No_of_Channels = NC, Signal_Time_s = ST };

//            test.Acquire(input_id, Pachyderm_Acoustic.Audio.Pach_SP.Measurement.Signal_Type.Swept_Sine, output_id);

//            for (int i = 0; i < NC; i++) AS_final.Add(new Audio_Signal(test.IR[i], test.Sample_Frequency));

//            while(test.Running)
//            {
//                System.Threading.Thread.Sleep(100);
//            }

//            string message = "Signal to Noise Ratio \n" + "Freq  63    125   250   500   1000  2000  4000  8000";
//            for (int i = 0; i < NC; i++) message += "\nch." + i + "   " + Math.Round(test.SNR[i][0], 1) + "   " + Math.Round(test.SNR[i][1], 1) + "   " + Math.Round(test.SNR[i][2], 1) + "   " + Math.Round(test.SNR[i][3], 1) + "   " + Math.Round(test.SNR[i][4], 1) + "   " + Math.Round(test.SNR[i][5], 1) + "   " + Math.Round(test.SNR[i][6], 1) + "   " + Math.Round(test.SNR[i][7], 1);
//            this.Message = message;
//            ComponentSupport.SetTree(access, 0, AS_final);
//        }

//        /// <summary>
//        /// Provides an Icon for the component.
//        /// </summary>
//        protected override System.Drawing.Bitmap Icon
//        {
//            get
//            {
//                System.Drawing.Bitmap b = Properties.Resources.Energy_Time_Curve;
//                b.MakeTransparent(System.Drawing.Color.White);
//                return b;
//            }
//        }

//        /// <summary>
//        /// Gets the unique ID for this component. Do not change this ID after release.
//        /// </summary>
//        public override Guid ComponentGuid
//        {
//            get { return new Guid("{3E8A5783-B435-4576-8906-AEED7D47E9FB}"); }
//        }
//    }
//}
