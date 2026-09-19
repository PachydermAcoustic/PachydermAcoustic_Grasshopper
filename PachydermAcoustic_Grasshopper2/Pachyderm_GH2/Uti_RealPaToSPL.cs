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
using Grasshopper2.Extensions;
using System.Linq;

namespace PachydermGH
{
    [IoId("1b1f9d4c-ae74-480b-b4be-41423aafa517")]
    public class RealPaToSPL : Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent2 class.
        /// </summary>
        public RealPaToSPL()
            : base(new Nomen("Real Pressure to SPL",
                "Converts real pressure values to Sound Pressure Level",
                "Acoustics", "Utility"))
        {
            Threading = Grasshopper2.Components.ThreadingState.SingleThreaded;
        }

        public RealPaToSPL(IReader reader) : base(reader) { Threading = Grasshopper2.Components.ThreadingState.SingleThreaded; }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void AddInputs(InputAdder inputs)
        {
            inputs.AddNumber("Magnitude Pressure", "P", "Real-valued pressure", Access.Tree);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void AddOutputs(OutputAdder outputs)
        {
            outputs.AddNumber("Sound Pressure Level", "SPL", "Sound Pressure Level", Access.Tree);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="access">The access object is used to retrieve from inputs and store in outputs.</param>
        protected override void Process(IDataAccess access)
        {
            if(!access.GetTree<double>(0,out var tree) || tree==null) return;
            // Convert each numeric leaf while retaining paths, metadata and null leaves.
            tree.ToArrays(out double[][] values, out Grasshopper2.Data.Meta.MetaData[][] metadata, out bool[][] nulls);
            values = values.Select(row => (double[])row.Clone()).ToArray();
            for(int branch=0;branch<values.Length;branch++)
                for(int item=0;item<values[branch].Length;item++)
                    if(nulls == null || nulls[branch] == null || !nulls[branch][item])
                        values[branch][item]=Pachyderm_Acoustic.Utilities.AcousticalMath.SPL_Pressure(values[branch][item]);
            ComponentSupport.SetTree(access, 0,Garden.TreeFromArrays(tree.Paths,values,metadata,nulls));
        }
        protected override IIcon IconInternal
        {
            get
            {
                var assembly = typeof(SPLETC).Assembly;
                var resourceName = "PachydermGH2.Resources.Real Pressure to SPL.png";

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
