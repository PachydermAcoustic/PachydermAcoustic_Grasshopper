//'Pachyderm-Acoustic: Geometrical Acoustics for Rhinoceros (GPL)   
//' C:\Users\Arthu\Desktop\DEV\PachydermAcoustic_Grasshopper\Ana_Strength.cs
//'This file is part of Pachyderm-Acoustic. 
//' 
//'Copyright (c) 2008-2025, Arthur van der aHrten 
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

    [IoId("565C3285-FE22-4750-9C0C-2742BEAF2724")]
    public class G_ETC : Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent2 class.
        /// </summary>
        public G_ETC()
            : base(new Nomen("Strength/G",
                "Computes Strength from Energy Time Curve",
                "Acoustics", "Analysis"))
        {
        }

        public G_ETC(IReader reader) : base(reader) { }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void AddInputs(InputAdder inputs)
        {
            inputs.AddGeneric("Energy Time Curve", "ETC", "Energy Time Curve", Access.Item);
            inputs.AddNumber("Source Power", "SWL", "sound power of the source object", Access.Tree);
            inputs.AddBoolean("Pressure?", "P/I", "True to use pressure (Coherent mixing) or false for intensity (incoherent mixing)...", Access.Item);
        }

        /// <summary>
        /// Registers all the output parameters for this c                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                -2omponent.
        /// </summary>
        protected override void AddOutputs(OutputAdder outputs)
        {
            outputs.AddNumber("Strength", "G", "Strength", Access.Tree);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void Process(IDataAccess access)
        {
            Audio_Signal ETC;
            access.GetItem<Audio_Signal>(0, out ETC);
            Tree<double> SWL_T;
            access.GetTree<double>(1, out SWL_T);
            bool pres = false;
            access.GetItem<bool>(2, out pres);
            List<double> SWL = new List<double>(SWL_T.AllItems);

            List<double> G = new List<double>();
            for (int i = 0; i < ETC.ChannelCount; i++)
            {
                double[] s = new double[ETC[i].Length];
                for (int j = 0; j < ETC[i].Length; j++) s[j] = (double)ETC[i][j];
                    G.Add(Pachyderm_Acoustic.Utilities.AcousticalMath.Strength(s, SWL[i], pres));
            }

            access.SetItem(0, G);
        }
        protected override IIcon IconInternal
        {
            get
            {
                var assembly = typeof(SPLETC).Assembly;
                var resourceName = "PachydermGH2.Resources.Strength.png";
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

        //protected override IIcon IconInternal => new Grasshopper2.UI.Icon.PixelIcon("Strength");
    }
}