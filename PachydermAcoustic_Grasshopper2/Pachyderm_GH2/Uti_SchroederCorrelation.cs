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
using Pachyderm_Acoustic.Utilities;
using System.Linq;

namespace PachydermGH
{
    /// <summary>
    /// This component calculates the Schroeder correlation between two impulse responses.
    /// </summary>
    [IoId("EFCA3675-B9C5-4428-B69C-7E1CA000C86B")]
    public class Schroeder_Correlation : Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent2 class.
        /// </summary>
        public Schroeder_Correlation()
            : base(new Nomen("Schroeder Correlation",
                "Calculates the Schroeder integral of two impulse responses, and then calculates the correlation between them.",
                "Acoustics", "Utility"))
        {
            Threading = Grasshopper2.Components.ThreadingState.SingleThreaded;
        }

        public Schroeder_Correlation(IReader reader) : base(reader) { Threading = Grasshopper2.Components.ThreadingState.SingleThreaded; }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void AddInputs(InputAdder inputs)
        {
            inputs.AddGeneric("Impulse Response 1", "IR1", "Plug the audio signal impulse response in here.", Access.Item);
            inputs.AddGeneric("Impulse Response 2", "IR2", "Plug the audio signal impulse response in here.", Access.Item);
            inputs.AddNumber("Delay", "D", "Enter an offset in samples to help match the two integrals. A positive number will take samples off the front, of IR1. A negative numuber will take sampels off the front of IR2.", Access.Item).Set(0.0);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void AddOutputs(OutputAdder outputs)
        {
            outputs.AddNumber("Correlation R", "R_c", "Correlation R value for the two IRs Schroeder Integrals.", Access.Tree);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="access">The access object is used to retrieve from inputs and store in outputs.</param>
        protected override void Process(IDataAccess access)
        {
            var a = ComponentSupport.Signal(access, 0); var b = ComponentSupport.Signal(access, 1);
            access.GetItem<double>(2, out var delayValue);
            if (a.ChannelCount != b.ChannelCount || a.SampleFrequency != b.SampleFrequency) throw new ArgumentException("Signals must have equal channel counts and sample rates.");
            int delay = checked((int)Math.Round(delayValue));
            int startA = Math.Max(0, delay), startB = Math.Max(0, -delay);
            int length = Math.Min(a.Count-startA, b.Count-startB);
            if (length < 2) throw new ArgumentException("Delay leaves fewer than two overlapping samples.");
            var results = new List<double>();
            for (int c = 0; c < a.ChannelCount; c++) {
                var x = new double[length]; var y = new double[length];
                Array.Copy(a[c], startA, x, 0, length); Array.Copy(b[c], startB, y, 0, length);
                for (int i = 0; i < length; i++) { x[i] *= x[i]; y[i] *= y[i]; }
                x = AcousticalMath.Schroeder_Integral(x); y = AcousticalMath.Schroeder_Integral(y);
                results.Add(MathNet.Numerics.Statistics.Correlation.Spearman(x, y));
            }
            ComponentSupport.SetTree(access, 0, Garden.TreeFromList(results));
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
    }
}
