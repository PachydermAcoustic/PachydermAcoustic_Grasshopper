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
using System.Collections.Generic;

namespace PachydermGH
{
    [IoId("C1223793-ECAF-4E5E-9EDA-96A98A3CF0FF")]
    public class Unweave_Signal : Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public Unweave_Signal()
            : base(new Nomen("Unweave Signal",
                "Unbraids woven signal to discrete signals by channel",
                "Acoustics", "Audio"))
        {
        }

        public Unweave_Signal(IReader reader) : base(reader) { }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void AddInputs(InputAdder inputs)
        {
            inputs.AddGeneric("Input Data", "Signal", "The data to unweave...", Access.Item);
            inputs.AddInteger("Channel Selection", "Ch", "Channel to crib from...", Access.Item);
            inputs.AddInterval("Time Interval", "Ival", "The samples to crib from the signal...", Access.Item);
            inputs[2].Requirement = Requirement.MayBeMissing;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void AddOutputs(OutputAdder outputs)
        {
            outputs.AddGeneric("Unwoven Signal", "Signals", "The resulting unwoven signal", Access.Tree);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void Process(IDataAccess access)
        {
            int channel = 0;
            Audio_Signal Buffer = new Audio_Signal();
            access.GetItem<Audio_Signal>(0, out Buffer);
            access.GetItem<int>(1, out channel);
            Interval ival = new Interval();
            if (!access.GetItem<Interval>(2, out ival)) ival = new Interval(0, Buffer.Count);
            
            float[] signals = new float[(int)ival.Length];
            for (int i = 0; i < signals.Length; i++)
            {
                signals[i] = (float)Buffer[channel][(int)ival.T0 + i];
            }

            access.SetItem(0, new Audio_Signal(signals, Buffer.SampleFrequency));
        }

        protected override IIcon IconInternal
        {
            get
            {
                var assembly = typeof(SPLETC).Assembly;
                var resourceName = "Pachyderm_GH.Icons.Unweave_Signal.png";

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
    }
}