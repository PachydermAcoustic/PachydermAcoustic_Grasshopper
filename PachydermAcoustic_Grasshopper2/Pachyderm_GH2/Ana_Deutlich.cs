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
using Pachyderm_Acoustic;
using Pachyderm_Acoustic.Environment;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.IO;

namespace PachydermGH
{
    [IoId("6F6911C6-00BE-4D0D-87B6-B32378C780F8")]
    public class D_X_ETC : Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent2 class.
        /// </summary>
        public D_X_ETC()
            : base(new Nomen("Definition",
                "Computes Energy Ratio (Definition style) from Energy Time Curve",
                "Acoustics", "Analysis"))
        {
        }
        
        public D_X_ETC(IReader reader) : base(reader) { }

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
            outputs.AddNumber("Definition", "D", "Definition", Access.Tree);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void Process(IDataAccess access)
        {
            Audio_Signal ETC = null;
            access.GetItem<Audio_Signal>(0, out ETC);
            int Dx = 50;

            List<double> D = new List<double>();
            foreach (double[] f in ETC.Value)
            {
                double[] s = new double[f.Length];
                int start = 0;
                if (ETC.Direct_Sample == null)
                {
                    for (int i = 0; i < f.Length; i++)
                    {
                        if (start == 0) if (f[i] != 0) start = i;
                    }
                }
                else start = ETC.Direct_Sample[0];
                D.Add(Pachyderm_Acoustic.Utilities.AcousticalMath.Definition(s, ETC.SampleFrequency, Dx/1000.0, (double)start/(double)ETC.SampleFrequency, false));
            }

            access.SetTree(0, Garden.TreeFromList(D));
        }

        protected override IIcon IconInternal
        {
            get
            {
                var assembly = typeof(SPLETC).Assembly;
                var resourceName = "Pachyderm_GH.Icons.Definition.png";

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

        //protected override IIcon IconInternal => new Grasshopper2.UI.Icon.PixelIcon("Definition");
    }
}