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
    [IoId("0F1433F5-AED9-470D-8FDA-BD40A9DB5B59")]
    public class Spectrum2List : Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public Spectrum2List()
            : base(new Nomen("Spectrum2List",
                "Casts a signal to a list readable in Grasshopper",
                "Acoustics", "Audio"))
        {
        }

        public Spectrum2List(IReader reader) : base(reader) { }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void AddInputs(InputAdder inputs)
        {
            inputs.AddGeneric("Input Data", "Signal", "The data to divide...", Access.Item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void AddOutputs(OutputAdder outputs)
        {
            outputs.AddNumber("Magnitude", "M", "The exported Frequency Magnitude in pressure...", Access.Tree);
            outputs.AddComplex("Spectrum", "SC", "The complex power at each frequency in the spectrum...", Access.Tree);
            outputs.AddNumber("Frequency", "F", "The exported Frequency Domain...", Access.Tree);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void Process(IDataAccess access)
        {
            //int chan = 0;
            Frequency_Spectrum Buffer = new Frequency_Spectrum();
            access.GetItem<Frequency_Spectrum>(0, out Buffer);
            //access.GetItem<int>(1, out chan);

            List<double[]> signal = new List<double[]>();
            List<System.Numerics.Complex> spectrum = new List<System.Numerics.Complex>();
            List<double[]> Freq = new List<double[]>();
            for (int s = 0; s < Buffer.Magnitude.Length; s++) signal.Add(new double[] { Buffer.Magnitude[s] });
            for (int s = 0; s < Buffer.Value.Count; s++ ) spectrum.Add(new System.Numerics.Complex(Buffer.Value[s].Real, Buffer.Value[s].Imaginary));
            for (int s = 0; s < Buffer.Frequency.Length; s++ ) Freq.Add(new double[] { Buffer.Frequency[s] });

            access.SetTree(0, Garden.TreeFromList(signal));
            access.SetTree(1, Garden.TreeFromList(spectrum));
            access.SetTree(2, Garden.TreeFromList(Freq));
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