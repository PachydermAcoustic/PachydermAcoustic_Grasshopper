using Grasshopper2.Data;
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

using Grasshopper2.Components;
using Grasshopper2.Parameters;
using Grasshopper2.UI;
using Grasshopper2.UI.Icon;
using GrasshopperIO;
using Pachyderm_Acoustic;
using Pachyderm_Acoustic.Environment;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.IO;

namespace PachydermGH
{
    [IoId("F0CB3233-24E5-4B6A-AE3A-5C3C0FE0207D")]
    public class ITDG_ETC : Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent2 class.
        /// </summary>
        public ITDG_ETC()
        : base(new Nomen("Initial Time Delay Gap",
                "Computes ITDG from Energy Time Curve",
                "Acoustics", "Analysis"))
        {
            Threading = Grasshopper2.Components.ThreadingState.SingleThreaded;
        }
        public ITDG_ETC(IReader reader) : base(reader) { Threading = Grasshopper2.Components.ThreadingState.SingleThreaded; }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void AddInputs(InputAdder inputs)
        {
            inputs.AddGeneric("Energy Time Curve", "ETC", "Energy Time Curve", Access.Item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void AddOutputs(OutputAdder outputs)
        {
            outputs.AddNumber("ITDG", "ITDG", "Initial Time Delay Gap", Access.Tree);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void Process(IDataAccess access)
        {
            Audio_Signal ETC = null;
            ETC = ComponentSupport.Signal(access, 0);

            List<double> I = new List<double>();
            foreach (double[] f in ETC.Value)
            {
                double[] s = new double[f.Length];
                I.Add(Pachyderm_Acoustic.Utilities.AcousticalMath.InitialTimeDelayGap(f, ETC.SampleFrequency));
            }

            ComponentSupport.SetTree(access, 0, Garden.TreeFromList(I));
        }

        protected override IIcon IconInternal
        {
            get
            {
                var assembly = typeof(SPLETC).Assembly;
                var resourceName = "PachydermGH2.Resources.Delay Gap.png";

                using (var stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream == null) return null;

                    // FromStream reads serialized .ghicon data, not PNG/BMP images.
                    // The PixelIcon retains the bitmap for its cached lifetime.
                    return new Grasshopper2.UI.Icon.PixelIcon(new Eto.Drawing.Bitmap(stream));
                }
            }
        }

        //protected override IIcon IconInternal => new Grasshopper2.UI.Icon.PixelIcon("Delay_Gap");
    }
}
