////'Pachyderm-Acoustic: Geometrical Acoustics for Rhinoceros (GPL)   
////' 
////'This file is part of Pachyderm-Acoustic. 
////' 
////'Copyright (c) 2008-2025, Open Research in Acoustical Science and Education, Inc. - a 501(c)3 nonprofit 
////'Pachyderm-Acoustic is free software; you can redistribute it and/or modify 
////'it under the terms of the GNU General Public License as published 
////'by the Free Software Foundation; either version 3 of the License, or 
////'(at your option) any later version. 
////'Pachyderm-Acoustic is distributed in the hope that it will be useful, 
////'but WITHOUT ANY WARRANTY; without even the implied warranty of 
////'MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
////'GNU General Public License for more details. 
////' 
////'You should have received a copy of the GNU General Public 
////'License along with Pachyderm-Acoustic; if not, write to the Free Software 
////'Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA. 

//using System;
//using System.Collections.Generic;

//using Grasshopper.Kernel;
//using Rhino.Geometry;

//namespace PachydermGH
//{
//    public class Rad_Impedance : GH_Component
//    {
//        /// <summary>
//        /// Initializes a new instance of the MyComponent2 class.
//        /// </summary>
//        public Rad_Impedance()
//            : base("}Radiation Impedance", "Z-Rad",
//                "Gets radiation impedance field for a rectangular surface of given dimensions",
//                "Acoustics", "Materials")
//        {
//        }

//        /// <summary>
//        /// Registers all the input parameters for this component.
//        /// </summary>
//        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
//        {
//            pManager.AddMeshParameter("Sample", "M", "A mesh of the sample for which radiation impedance is to be calculated...", GH_ParamAccess.item);
//            pManager.AddNumberParameter("Frequency", "F", "Frequency to be evaluated...", GH_ParamAccess.item);
//            pManager.AddNumberParameter("theta", "a", "Elevation angle (zero = normal)", GH_ParamAccess.item);

//        }

//        /// <summary>
//        /// Registers all the output parameters for this component.
//        /// </summary>
//        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
//        {
//            pManager.AddNumberParameter("Real Z", "Zr", "Real part Radiation Impedance", GH_ParamAccess.list);
//            pManager.AddNumberParameter("Imag Z", "Zi", "Imaginary part Radiation Impedance", GH_ParamAccess.list);
//        }

//        /// <summary>
//        /// This is the method that actually does the work.
//        /// </summary>
//        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
//        protected override void SolveInstance(IGH_DataAccess DA)
//        {
//            Rhino.Geometry.Mesh M = new Mesh();
//            double f = 0, theta = 0;
//            DA.GetData<Mesh>(0, ref M);
//            DA.GetData<double>(1, ref f);
//            DA.GetData<double>(2, ref theta);

//            System.Numerics.Complex[] Z = Pachyderm_Acoustic.AbsorptionModels.Operations.Finite_Radiation_Impedance_Rect(M, 0, f, theta, 343);

//            List<double> Zr = new List<double>(), Za = new List<double>();

//            foreach(System.Numerics.Complex Zwhole in Z)
//            {
//                Zr.Add(Zwhole.Real);
//                Za.Add(Zwhole.Imaginary);
//            }

//            DA.SetDataList(0, Zr);
//            DA.SetDataList(1, Za);

//        }

//        /// <summary>
//        /// Provides an Icon for the component.
//        /// </summary>
//        protected override System.Drawing.Bitmap Icon
//        {
//            get
//            {
//                return (System.Drawing.Bitmap)Properties.Resources.ResourceManager.GetObject("Radiation Impedance.bmp");
//            }
//        }

//        /// <summary>
//        /// Gets the unique ID for this component. Do not change this ID after release.
//        /// </summary>
//        public override Guid ComponentGuid
//        {
//            get { return new Guid("{16F3599F-2073-49B0-8950-F832E96F80A5}"); }
//        }
//    }
//}