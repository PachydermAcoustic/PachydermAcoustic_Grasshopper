//'Pachyderm-Acoustic: Geometrical Acoustics for Rhinoceros (GPL) by Arthur van der Harten 
//' 
//'This file is part of Pachyderm-Acoustic. 
//' 
//'Copyright (c) 2008-2015, Arthur van der Harten 
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
using System.Collections.Generic;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using Pachyderm_Acoustic;

namespace PachydermGH
{
    public class GeodesicSource_Component : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public GeodesicSource_Component()
            : base("GeodesicSource", "GeoSrc",
                "Geodesic Omnidirectional Source Object",
                "Acoustics", "Source Modelling")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Origin", "Or", "Acoustic Center of the Speaker", GH_ParamAccess.item);
            pManager.AddNumberParameter("Power", "P", "The power spectrum for the source(0 = 62.5, 1 = 125 ... 7 = 8000)", GH_ParamAccess.list);
            pManager.AddNumberParameter("Delay", "D", "Signal delay", GH_ParamAccess.item);
            pManager.AddNumberParameter("Phase", "PH", "The phase spectrum for the source(0 = 62.5, 1 = 125 ... 7 = 8000)", GH_ParamAccess.list);
            Grasshopper.Kernel.Parameters.Param_Number param = (pManager[1] as Grasshopper.Kernel.Parameters.Param_Number);
            if (param != null) param.SetPersistentData(new List<GH_Number> { new GH_Number(120), new GH_Number(120), new GH_Number(120), new GH_Number(120), new GH_Number(120), new GH_Number(120), new GH_Number(120), new GH_Number(120) });
            Grasshopper.Kernel.Parameters.Param_Number param2 = (pManager[2] as Grasshopper.Kernel.Parameters.Param_Number);
            if (param2 != null) param2.SetPersistentData(0);
            Grasshopper.Kernel.Parameters.Param_Number param3 = (pManager[3] as Grasshopper.Kernel.Parameters.Param_Number);
            if (param3 != null) param3.SetPersistentData(new List<GH_Number> { new GH_Number(0), new GH_Number(0), new GH_Number(0), new GH_Number(0), new GH_Number(0), new GH_Number(0), new GH_Number(0), new GH_Number(0) });

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Source", "Src", "Omnidirectional source object.", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Point3d Origin = new Point3d();
            List<double> Level = new List<double>();
            List<double> phase = new List<double>();
            double delay = 0;
            DA.GetData<Point3d>(0, ref Origin);
            DA.GetDataList<double>(1, Level);
            DA.GetData<double>(2, ref delay);
            DA.GetDataList<double>(3, phase);

            Pachyderm_Acoustic.Environment.GeodesicSource S = new Pachyderm_Acoustic.Environment.GeodesicSource(Level.ToArray(), phase.ToArray(), Origin, DA.Iteration);
            DA.SetData(0, S);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                //return 
                return null;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{DC5E7AC2-456F-4B91-A433-23B8987DD081}"); }
        }
    }
}