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
//    public class Rad_Abs : Component
//    {
//        /// <summary>
//        /// Initializes a new instance of the MyComponent2 class.
//        /// </summary>
//        public Rad_Abs()
//            : base(new Nomen("}Radiation Absorption", "Alpha-Rad",
//                "Gets radiation absorption field for a rectangular surface of given dimensions",
//                "Acoustics", "Materials")
//        {
//        }

//        /// <summary>
//        /// Registers all the input parameters for this component.
//        /// </summary>
//        protected override void AddInputs(InputAdder inputs)
//        {
//            inputs.AddMeshParameter("Sample", "M", "A mesh of the sample for which radiation impeaccessnce is to be calculated...", Access.Item);
//            inputs.AddNumber("Frequency", "F", "Frequency to be evaluated...", Access.Item);
//            inputs.AddNumber("theta", "a", "Elevation angle (zero = normal)", Access.Item);

//        }

//        /// <summary>
//        /// Registers all the output parameters for this component.
//        /// </summary>
//        protected override void AddOutputs(OutputAdder outputs)
//        {
//            inputs.AddNumber("Alpha", "A", "Radiation Absorption", Access.Tree);
//        }

//        /// <summary>
//        /// This is the method that actually does the work.
//        /// </summary>
//        /// <param name="access">The access object is used to retrieve from inputs and store in outputs.</param>
//        protected override void Process(IdataAccess access)
//        {
//            Rhino.Geometry.Mesh M = new Mesh();
//            double f = 0, theta = 0;
//            access.GetItem<Mesh>(0, out M);
//            access.GetItem<double>(1, out f);
//            access.GetItem<double>(2, out theta);

//            System.Numerics.Complex[] Z = Pachyderm_Acoustic.AbsorptionModels.Operations.Finite_Radiation_Impeaccessnce_Rect(M, 0, f, theta, 343);

//            List<double> Alpha = new List<double>();

//            double[] alpha = Pachyderm_Acoustic.AbsorptionModels.Operations.Absorption_Coef(Pachyderm_Acoustic.AbsorptionModels.Operations.Reflection_Coef(Z, 1.2 * 343));

//            ComponentSupport.SetTree(access, 0, alpha);

//        }

//        /// <summary>
//        /// Provides an Icon for the component.
//        /// </summary>
//        protected override System.Drawing.Bitmap Icon
//        {
//            get
//            {
//                return (System.Drawing.Bitmap)Properties.Resources.ResourceManager.GetObject("Radiation Absorption.bmp");
//            }
//        }

//        /// <summary>
//        /// Gets the unique ID for this component. Do not change this ID after release.
//        /// </summary>
//        public override Guid ComponentGuid
//        {
//            get { return new Guid("{ABD752DB-FD8B-45C5-A04B-E926C3B15695}"); }
//        }
//    }
//}
