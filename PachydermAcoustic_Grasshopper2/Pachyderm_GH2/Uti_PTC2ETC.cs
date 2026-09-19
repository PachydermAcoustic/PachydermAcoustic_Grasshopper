using System.Linq;
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
            Threading = Grasshopper2.Components.ThreadingState.SingleThreaded;
        }

        public PTC2ETC(IReader reader) : base(reader) { Threading = Grasshopper2.Components.ThreadingState.SingleThreaded; Combine=reader.TryRead<bool>("Combine",true);}

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        public override void Store(IWriter writer) { base.Store(writer); writer.Boolean("Combine",Combine); }
        protected override void AddInputs(InputAdder inputs)
        {
            inputs.AddGeneric("Impulse Response", "IR", "Plug the audio signal impulse response in here.", Access.Item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void AddOutputs(OutputAdder outputs)
        {
            outputs.AddGeneric("Energy-Time Curve", "ETC", "The energy-time-curve result of conversion...", Access.Tree);
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
            // Expire schedules a fresh solution after changing a setting.
            this.Expire();
            Document.Solution.Start();
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="access">The access object is used to retrieve from inputs and store in outputs.</param>
        protected override void Process(IDataAccess access)
        {
            var input = ComponentSupport.Signal(access, 0);
            var result = new List<Audio_Signal>();
            for (int c = 0; c < input.ChannelCount; c++) {
                var bands = new double[8][]; var direct = new int[8];
                for (int oct = 0; oct < 8; oct++) {
                    var band = Pachyderm_Acoustic.Audio.Pach_SP.FIR_Bandpass(input[c], oct, input.SampleFrequency, 0);
                    bands[oct] = new double[band.Length];
                    for (int i = 0; i < band.Length; i++) bands[oct][i] = band[i] * band[i];
                    direct[oct] = input.Direct_Sample[c];
                }
                result.Add(new Audio_Signal(bands, input.SampleFrequency, direct));
            }
            if (Combine && result.Count > 1) { var sum = result[0]; for (int c = 1; c < result.Count; c++) sum = ComponentSupport.Sum(sum, result[c]); result = new List<Audio_Signal> { sum }; }
            ComponentSupport.SetTree(access, 0, Garden.TreeFromList(result));
        }
        protected override IIcon IconInternal
        {
            get
            {
                var assembly = typeof(SPLETC).Assembly;
                var resourceName = "PachydermGH2.Resources.Energy Time Curve.png";

                using (var stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream == null) return null;

                    // FromStream reads serialized .ghicon data, not PNG/BMP images.
                    // The PixelIcon retains the bitmap for its cached lifetime.
                    return new Grasshopper2.UI.Icon.PixelIcon(new Eto.Drawing.Bitmap(stream));
                }
            }
        }

        //protected override IIcon IconInternal => new Grasshopper2.UI.Icon.PixelIcon("Energy_Time_Curve");
    }
}
