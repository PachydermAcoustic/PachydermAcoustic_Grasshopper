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
    [IoId("5BAC4E49-137F-4BE2-9469-C1E7D8E4A463")]
    public class SPLETC : Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent2 class.
        /// </summary>
        public SPLETC()
            : base(new Nomen("Sound Pressure Level",
                "Computes Sound Pressure Level from Energy Time Curve",
                "Acoustics", "Analysis"))
        {
            Threading = Grasshopper2.Components.ThreadingState.SingleThreaded;
        }
        public SPLETC(IReader reader) : base(reader) { Threading = Grasshopper2.Components.ThreadingState.SingleThreaded; }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void AddInputs(InputAdder inputs)
        {
            inputs.AddGeneric("Energy Time Curve", "ETC", "Energy signals or numeric intensities. Signals yield one SPL per channel; numbers are converted individually. Input branches are preserved.", Access.Tree);
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
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void Process(IDataAccess access)
        {
            if (!access.GetTree<object>(0, out var tree) || tree == null) return;
            tree.ToArrays(out object[][] branches);
            var rows = new double[branches.Length][];
            for (int b = 0; b < branches.Length; b++)
            {
                var levels = new List<double>();
                for (int i = 0; i < branches[b].Length; i++)
                {
                    var item = branches[b][i];
                    if (item is Audio_Signal signal)
                    {
                        if (signal.Value == null || signal.Value.Length == 0)
                            throw new ArgumentException("Provide a nonempty energy signal.");
                        foreach (var channel in signal.Value)
                        {
                            if (channel == null || channel.Length == 0)
                                throw new ArgumentException("Energy signal channels must not be empty.");
                            double sum = 0;
                            foreach (double sample in channel) sum += sample;
                            levels.Add(Pachyderm_Acoustic.Utilities.AcousticalMath.SPL_Intensity(sum));
                        }
                    }
                    else if (item is double || item is float || item is decimal || item is int || item is long || item is short || item is byte || item is uint || item is ulong || item is ushort || item is sbyte)
                    {
                        double intensity = Convert.ToDouble(item);
                        if (double.IsNaN(intensity) || double.IsInfinity(intensity) || intensity < 0)
                            throw new ArgumentException($"Intensity at branch {tree.Paths[b]}, item {i} must be finite and nonnegative.");
                        levels.Add(Pachyderm_Acoustic.Utilities.AcousticalMath.SPL_Intensity(intensity));
                    }
                    else
                        throw new ArgumentException($"Expected an energy signal or numeric intensity at branch {tree.Paths[b]}, item {i}.");
                }
                rows[b] = levels.ToArray();
            }
            ComponentSupport.SetTree(access, 0, Garden.TreeFromArrays(tree.Paths, rows));
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
