//'Pachyderm-Acoustic: Geometrical Acoustics for Rhinoceros (GPL) by Arthur van der Harten 
//' 
//'This file is part of Pachyderm-Acoustic. 
//' 
//'Copyright (c) 2008-2018, Arthur van der Harten 
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
using Grasshopper.Kernel;

namespace PachydermGH
{
    public class Atmospheric_Properties : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public Atmospheric_Properties()
            : base("Medium Properties", "MP",
                "Acoustically significant properties of the vibrating medium.",
                "Acoustics", "Model")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Atmospheric Pressure", "AP", "Pressure of the medium. Default of 101.325 kPa (air)", GH_ParamAccess.item);
            pManager.AddNumberParameter("Temperature", "TC", "Temperature of the medium. Default of 20 degrees C (air)", GH_ParamAccess.item);
            pManager.AddNumberParameter("Relative Humidity", "H", "Humidity of the medium in percent. Default of 50% (air)", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Properties", "P", "Medium properties, including attenuation, sound speed, etc.", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            double Pa=0, Tc=0, Hr=0;
            if (!DA.GetData<double>(0, ref Pa)) Pa = 1013.25;
            if (!DA.GetData<double>(1, ref Tc)) Tc = 20;
            if (!DA.GetData<double>(2, ref Hr)) Hr = 50;

            Pachyderm_Acoustic.Environment.Medium_Properties MP = new Pachyderm_Acoustic.Environment.Uniform_Medium(0, Pa/100, Tc+273.15, Hr, false);
            DA.SetData(0, MP);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                System.Drawing.Bitmap b = Properties.Resources.Medium_Properties;
                b.MakeTransparent(System.Drawing.Color.White);
                return b;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{2da451b8-b49d-44b6-af05-acbba45f365d}"); }
        }
    }
}