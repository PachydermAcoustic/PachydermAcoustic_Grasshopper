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

namespace PachydermGH
{
    [IoId("E3E18F3F-687C-4E5B-95D0-D4F8508AB15B")]
    public class Signal2List : Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public Signal2List()
            : base(new Nomen("Signal2List",
                "Casts a signal to a list readable in Grasshopper",
                "Acoustics", "Audio"))
        {
        }

        public Signal2List(IReader reader) : base(reader) { }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void AddInputs(InputAdder inputs)
        {
            inputs.AddGeneric("Audio Signal", "Signal", "The data to divide...", Access.Item);
            inputs.AddInterval("Samples", "Domain", "Which samples to return", Access.Item);
            inputs.AddInteger("Channel", "Ch", "Which channel to convert...", Access.Item);

            inputs[1].Requirement = Requirement.MayBeMissing;
            inputs[2].Requirement = Requirement.MayBeMissing;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void AddOutputs(OutputAdder outputs)
        {
            outputs.AddNumber("samples", "S", "The exported signal...", Access.Tree);
            outputs.AddInteger("Sample Frequency", "FS", "Number of samples per second", Access.Item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void Process(IDataAccess access)
        {
            int chan = 0;
            Audio_Signal Buffer = new Audio_Signal();
            Interval domain = new Interval();
            access.GetItem<Audio_Signal>(0, out Buffer);
            access.GetItem<Interval>(1, out domain);
            access.GetItem<int>(2, out chan);

            double[] SignalBuffer = Buffer[chan];

            if (domain[1] - domain[0] < 1) domain[1] = Buffer.Count;

            List<double> signal = new List<double>();
            for (int i = (int)domain.Min; i < (int)domain.Max; i++) { signal.Add(SignalBuffer[i]); }
            //foreach (float s in SignalBuffer) signal.Add(s);

            access.SetItem(0, signal);
            access.SetItem(1, Buffer.SampleFrequency);
        }
        protected override IIcon IconInternal
        {
            get
            {
                var assembly = typeof(SPLETC).Assembly;
                var resourceName = "Pachyderm_GH.Icons.Signal_to_List.png";

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
        //protected override IIcon IconInternal => new Grasshopper2.UI.Icon.PixelIcon("Signal_to_List");
    }
}