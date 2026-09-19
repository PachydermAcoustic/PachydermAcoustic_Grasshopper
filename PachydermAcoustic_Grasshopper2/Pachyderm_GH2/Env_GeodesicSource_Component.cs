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
using System.Linq;

namespace PachydermGH
{
    [IoId("DC5E7AC2-456F-4B91-A433-23B8987DD081")]
    public class GeodesicSource_Component : Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public GeodesicSource_Component()
            : base(new Nomen("GeodesicSource",
                "Geodesic Omnidirectional Source Object",
                "Acoustics", "Model"))
        {
        }

        public GeodesicSource_Component(IReader reader) : base(reader) { }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void AddInputs(InputAdder inputs)
        {
            List<double> SWL_Default = new List<double> { 120, 120, 120, 120, 120, 120, 120, 120 };
            inputs.AddPoint("Origin", "Or", "Acoustic Center of the Sound Source", Access.Item);
            inputs.AddNumber("Power", "P", "The power spectrum for the source(0 = 62.5, 1 = 125 ... 7 = 8000)", Access.Tree, Requirement.MustExist);
            inputs.AddNumber("Delay", "D", "Signal delay", new Grasshopper2.UI.UiNumber(0,0), Access.Item, Requirement.MustExist);
            //Grasshopper.Kernel.Parameters.Param_Number param = (inputs[1] as Grasshopper.Kernel.Parameters.Param_Number);
            //if (param != null) param.SetPersistentData(new List<GH_Number> { new GH_Number(120), new GH_Number(120), new GH_Number(120), new GH_Number(120), new GH_Number(120), new GH_Number(120), new GH_Number(120), new GH_Number(120) });
            //Grasshopper.Kernel.Parameters.Param_Number param2 = (inputs[2] as Grasshopper.Kernel.Parameters.Param_Number);
            //if (param2 != null) param2.SetPersistentData(0);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void AddOutputs(OutputAdder outputs)
        {
            outputs.AddGeneric("Source", "Src", "Omnidirectional source object.", Access.Item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void Process(IDataAccess access)
        {
            Point3d Origin = new Point3d();
            Tree<double> Level_T;
            double delay = 0;
            access.GetItem<Point3d>(0, out Origin);
            access.GetTree<double>(1, out Level_T);
            access.GetItem<double>(2, out delay);

            Pachyderm_Acoustic.Environment.GeodesicSource S = new Pachyderm_Acoustic.Environment.GeodesicSource(Level_T.AllItems.ToArray(), new Hare.Geometry.Point(Origin.X, Origin.Y, Origin.Z), 0, Level_T.ItemCount > 8);
            access.SetItem(0, S);
        }
        protected override IIcon IconInternal
        {
            get
            {
                var assembly = typeof(SPLETC).Assembly;
                var resourceName = "Pachyderm_GH.Icons.Geodesic_Source.png";

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

        //protected override IIcon IconInternal => new Grasshopper2.UI.Icon.PixelIcon("Geodesic_Source");
    }
}