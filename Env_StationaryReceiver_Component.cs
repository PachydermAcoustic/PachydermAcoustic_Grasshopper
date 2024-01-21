//'Pachyderm-Acoustic: Geometrical Acoustics for Rhinoceros (GPL) by Arthur van der Harten 
//' 
//'This file is part of Pachyderm-Acoustic. 
//' 
//'Copyright (c) 2008-2024, Arthur van der Harten 
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
using Rhino.Geometry;

namespace PachydermGH
{
    public class StationaryReceiver_Component : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public StationaryReceiver_Component()
            : base("Stationary Receiver", "StatRec",
                "Non-growing 1 meter wide spherical receiver object",
                "Acoustics", "Model")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Origin", "Or", "Acoustic Center of the Speaker", GH_ParamAccess.list);
            pManager.AddGenericParameter("Sources", "Srcs", "Source Objects to be used", GH_ParamAccess.list);
            pManager.AddGenericParameter("Room Model", "Room", "The Pachyderm Room Model Reference", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Sample Frequency", "Fs", "Rate of samples per second to collect data in...", GH_ParamAccess.item);
            pManager.AddNumberParameter("Cut off time", "CO_Time", "The number of miliseconds that will be calculated for.", GH_ParamAccess.item);
            
            Grasshopper.Kernel.Parameters.Param_Integer param = (pManager[3] as Grasshopper.Kernel.Parameters.Param_Integer);
            if (param != null) param.SetPersistentData(44100);
            Grasshopper.Kernel.Parameters.Param_Number param2 = (pManager[4] as Grasshopper.Kernel.Parameters.Param_Number);
            if (param2 != null) param2.SetPersistentData(1000);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Receiver", "Rec", "Stationary Receiver object.", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Point3d> Origin = new List<Point3d>();
            List<Hare.Geometry.Point> H_Origin = new List<Hare.Geometry.Point>();
            DA.GetDataList<Point3d>(0, Origin);
            foreach (Point3d p in Origin) H_Origin.Add(new Hare.Geometry.Point(p.X, p.Y, p.Z));
            List<Pachyderm_Acoustic.Environment.Source> Srcs = new List<Pachyderm_Acoustic.Environment.Source>();
            DA.GetDataList<Pachyderm_Acoustic.Environment.Source>(1, Srcs);
            Pachyderm_Acoustic.Environment.Polygon_Scene S = null;
            DA.GetData<Pachyderm_Acoustic.Environment.Polygon_Scene>(2, ref S);
            int Fs = 0;
            DA.GetData<int>(3, ref Fs);
            double COTime = 0;
            DA.GetData<double>(4, ref COTime);

            for(int i = 0; i < Srcs.Count; i++)
            {
                Pachyderm_Acoustic.Environment.Receiver_Bank RB = new Pachyderm_Acoustic.Environment.Receiver_Bank(H_Origin, Srcs[i], S, Fs, COTime, Pachyderm_Acoustic.Environment.Receiver_Bank.Type.Stationary, false); 
                DA.SetData(0, RB);
            }
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                System.Drawing.Bitmap b = Properties.Resources.Microphone;
                b.MakeTransparent(System.Drawing.Color.White);
                return b;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{7F75FF09-FFE7-4D4B-B9D1-F7D92B3B396C}"); }
        }
    }
}