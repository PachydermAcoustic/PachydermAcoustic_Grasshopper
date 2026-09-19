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

namespace PachydermGH
{
    [IoId("2F45F41C-3027-4FE1-8071-26495398C06B")]
    public class PTC2ETC : Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent2 class.
        /// </summary>
        public PTC2ETC()
            : base(new Nomen("Energy-Time Curve from Impulse Response",
                "Creates the Energy-Time Curve from an impulse response, measured or simulated",
                "Acoustics", "Utility"))
        {
        }

        public PTC2ETC(IReader reader) : base(reader) { }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void AddInputs(InputAdder inputs)
        {
            inputs.AddGeneric("Impulse Response", "IR", "Plug the audio signal impulse response in here.", Access.Item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void AddOutputs(OutputAdder outputs)
        {
            outputs.AddGeneric("Energy-Time Curve", "ETC", "The energy-time-curve result of conversion...", Access.Item);
        }

        public override void AppendToInputPanel(InputPanel panel)
        {
            panel.AddCheck("Sum all ETCs...", Combine, Combine_Click);
            base.AppendToInputPanel(panel);
        }

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
            Audio_Signal IR = new Audio_Signal();
            access.GetItem<Audio_Signal>(0, out IR);

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