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
    [IoId("C33A1F42-9EEC-4D7F-AB99-13C5CF0812B2")]
    public class SPLAETC : Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent2 class.
        /// </summary>
        public SPLAETC()
            : base(new Nomen("A-weighted Sound Pressure Level",
                "Computes Sound Pressure Level (A) from Energy Time Curve",
                "Acoustics", "Analysis"))
        {
        }

        public SPLAETC(IReader reader) : base(reader) { }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void AddInputs(InputAdder inputs)
        {
            inputs.AddGeneric("Energy Time Curve", "ETC", "Energy Time Curve", Access.Tree);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void AddOutputs(OutputAdder outputs)
        {
            outputs.AddNumber("Sound Pressure Level(A)", "SPLA", "A-weighted Sound Pressure Level", Access.Tree);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void Process(IDataAccess access)
        {
            Tree<object> etcTree;
            if (!access.GetTree(0, out etcTree)) return; // if no input, just return.
            List<double> SPLA = new List<double>();

            object[][] AS; 
            etcTree.ToArrays(out AS);  

            foreach (object[] a in AS)
            {
                List<Audio_Signal> signals = new List<Audio_Signal>();
                List<double> Oct_SPL = new List<double>();
                Audio_Signal signal = a[0] as Audio_Signal;
                    if (signal != null)
                    {
                        for(int i = 0; i < a.Length; i++) signals.Add(a[i] as Audio_Signal);
                    }
                    else if (a[0] is double && a.Length == 8)
                    {
                        for (int i = 0; i < a.Length; i++) Oct_SPL.Add((double)a[i]);
                    }
       
                if (signals.Count > 0)
                {
                    foreach (var sig in signals)
                    {
                        Oct_SPL.Clear();
                        for (int i = 0; i < sig.Value.Length; i++)
                        {       
                            double sum = 0;
                            for (int j = 0; j < sig.Value[i].Length; j++)
                                sum += sig.Value[i][j];
                            Oct_SPL.Add(Pachyderm_Acoustic.Utilities.AcousticalMath.SPL_Intensity(sum));
                        }
                        SPLA.Add(Pachyderm_Acoustic.Utilities.AcousticalMath.Sound_Pressure_Level_A(Oct_SPL.ToArray()));
                    }
                }
                else if (Oct_SPL.Count > 0)
                {
                    for (int i = 0; i < Oct_SPL.Count; i += 8)
                        SPLA.Add(Pachyderm_Acoustic.Utilities.AcousticalMath.Sound_Pressure_Level_A(Oct_SPL.GetRange(i, 8).ToArray()));
                }
            }

            access.SetTree(0, Garden.TreeFromList(SPLA));
            return;
        }
        protected override IIcon IconInternal
        {
            get
            {
                var assembly = typeof(SPLETC).Assembly;
                var resourceName = "Pachyderm_GH.Icons.SPL.png";

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