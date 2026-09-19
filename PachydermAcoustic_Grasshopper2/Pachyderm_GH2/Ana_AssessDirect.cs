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
using System.IO;

namespace PachydermGH
{ 

    [IoId("E60566C0-95C5-4FC0-BEA2-EDEECD31A5FD")]
    public sealed class Assess_Direct : Component
    {
        /// <summary>
        /// Initializes a new instance of the Assess_Direct class.
        /// </summary>
        public Assess_Direct()
            : base(new Nomen("Assess Direct Time",
                "Takes an impulse response, finds the direct sound, and adds the direct time to the signal object for use in analysis.",
                "Acoustics", "Analysis"))
        {
        }

        public Assess_Direct(IReader reader) : base(reader) { }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void AddInputs(InputAdder inputs)
        {
            inputs.AddGeneric("Impulse Response", "IR", "The impulse response, or other signal for which the direct sound must be found.", Access.Item);
            inputs.AddInteger("Mod-ms", "_t", "Did we get the time wrong? This method is pretty good, but it isn't perfect. Use this to specify the number of milliseconds to modify it by.", Access.Item, 0);
            inputs[1].Requirement = Requirement.MayBeMissing;
        }

        //bool Noise_Compensation = false;

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void AddOutputs(OutputAdder outputs)
        {
            outputs.AddGeneric("Impulse Response_out", "IRout", "The modified impulse response with the direct time built in.", Access.Item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void Process(IDataAccess access)
        {
            Audio_Signal ETC = null;
            access.GetItem<Audio_Signal>(0, out ETC);
            int tx = 0;
            access.GetItem<int>(1, out tx);

            ETC.Direct_Sample = new int[ETC.ChannelCount];

            double deltaEMax = 0;
            int D_Sound = 0;
            for (int c = 0; c < ETC.ChannelCount; c++)
            { 
                for (int i = 1; i < ETC.Count; i++)
                {
                    double deltaE = ETC[c][i] * ETC[c][i] - ETC[c][i - 1] * ETC[c][i - 1];
                    if (deltaE > deltaEMax)
                    {
                        deltaEMax = deltaE;
                        D_Sound = i;
                    }
                }
                ETC.Direct_Sample[c] = D_Sound + (int)(tx * ETC.SampleFrequency);
            }

            access.SetTree(0, Garden.TreeFromList(ETC));
        }

        protected override IIcon IconInternal
        {
            get
            {
                var assembly = typeof(SPLETC).Assembly;
                var resourceName = "Pachyderm_GH.Icons.RT.png";

                using (var stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream == null) return null;

                    var ms = new MemoryStream();
                    stream.CopyTo(ms);
                    ms.Position = 0;
                    return Grasshopper2.UI.Icon.PixelIcon.FromStream(ms);
                }
            }
        }

        //protected override IIcon IconInternal => new Grasshopper2.UI.Icon.PixelIcon("RT");

    }
}