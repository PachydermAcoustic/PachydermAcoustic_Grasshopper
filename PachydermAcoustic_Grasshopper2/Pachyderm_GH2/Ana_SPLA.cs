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
            Threading = Grasshopper2.Components.ThreadingState.SingleThreaded;
        }

        public SPLAETC(IReader reader) : base(reader) { Threading = Grasshopper2.Components.ThreadingState.SingleThreaded; }

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
            if(!access.GetTree<object>(0,out var tree) || tree==null) return;
            tree.ToArrays(out object[][] branches);
            var rows=new List<double[]>();
            foreach(var branch in branches) {
                var values=new List<double>();
                if(branch.Length==0) { rows.Add(Array.Empty<double>()); continue; }
                if(branch.All(x=>x is double)) {
                    if(branch.Length!=8) throw new ArgumentException("Provide eight octave-band levels per branch.");
                    values.Add(Pachyderm_Acoustic.Utilities.AcousticalMath.Sound_Pressure_Level_A(branch.Cast<double>().ToArray()));
                } else {
                    foreach(var item in branch) {
                        if(!(item is Audio_Signal signal) || signal.ChannelCount!=8) throw new ArgumentException("Provide an eight-band energy signal or eight numeric octave levels.");
                        var levels=signal.Value.Select(channel=>Pachyderm_Acoustic.Utilities.AcousticalMath.SPL_Intensity(channel.Sum())).ToArray();
                        values.Add(Pachyderm_Acoustic.Utilities.AcousticalMath.Sound_Pressure_Level_A(levels));
                    }
                }
                rows.Add(values.ToArray());
            }
            ComponentSupport.SetTree(access, 0,Garden.TreeFromArrays(tree.Paths,rows.ToArray()));
        }
        protected override IIcon IconInternal
        {
            get
            {
                var assembly = typeof(SPLETC).Assembly;
                var resourceName = "PachydermGH2.Resources.SPL.png";

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
