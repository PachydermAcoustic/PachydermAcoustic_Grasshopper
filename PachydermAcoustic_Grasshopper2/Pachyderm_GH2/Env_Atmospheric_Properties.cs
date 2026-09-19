using Grasshopper2.Data;
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
using System.Collections.Generic;

namespace PachydermGH
{
    [IoId("2da451b8-b49d-44b6-af05-acbba45f365d")]
    public class Atmospheric_Properties : Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public Atmospheric_Properties()
            : base(new Nomen("Medium Properties",
                "Acoustically significant properties of the vibrating medium.",
                "Acoustics", "Model"))
        {
            Threading = Grasshopper2.Components.ThreadingState.SingleThreaded;
        }

        public Atmospheric_Properties(IReader reader) : base(reader) { Threading = Grasshopper2.Components.ThreadingState.SingleThreaded; }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void AddInputs(InputAdder inputs)
        {
            inputs.AddNumber("Atmospheric Pressure", "AP", "Pressure of the medium in kPa (not Pa). Default of 101.325 kPa (air)", Access.Item).Set(101.325);
            inputs.AddNumber("Temperature", "TC", "Temperature of the medium. Default of 20 degrees C (air)", Access.Item).Set(20.0);
            inputs.AddNumber("Relative Humidity", "H", "Humidity of the medium in percent. Default of 50% (air)", Access.Item).Set(50.0);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void AddOutputs(OutputAdder outputs)
        {
            outputs.AddGeneric("Properties", "P", "Medium properties, including attenuation, sound speed, etc.", Access.Item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void Process(IDataAccess access)
        {
            double Pa=0, Tc=0, Hr=0;
            if (!access.GetItem<double>(0, out Pa)) Pa = 101.325;
            if (!access.GetItem<double>(1, out Tc)) Tc = 20;
            if (!access.GetItem<double>(2, out Hr)) Hr = 50;

            Pachyderm_Acoustic.Environment.Medium_Properties MP = new Pachyderm_Acoustic.Environment.Uniform_Medium(0, Pa*10, Tc+273.15, Hr, false);
            access.SetItem(0, MP);
        }
        protected override IIcon IconInternal
        {
            get
            {
                var assembly = typeof(SPLETC).Assembly;
                var resourceName = "PachydermGH2.Resources.Medium Properties.png";

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
