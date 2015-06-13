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
using Rhino.Geometry;

namespace MyProject1
{
    public class RayTracing : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public RayTracing()
            : base("Ray Tracing", "Pach_RT",
                "Performs Ray Tracing (Specular and Diffuse) calculations on the geometry specified.",
                "Acoustics", "Computation")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Room Model", "Room", "The Pachyderm Room Model Reference", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Number of Rays", "RayCt", "The number of rays initially used.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Cut off time", "CO_Time", "The number of miliseconds that will be calculated for.", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Image Source Order", "IS_Order", "Ray tracing will ignore specular reflections up to this order... in order to combine results with deterministic image source results.", GH_ParamAccess.item);
            pManager.AddGenericParameter("Source", "Src", "Sound Source Objects...", GH_ParamAccess.list);
            pManager.AddGenericParameter("Receiver", "Rec", "Listening Object (Receiver_Bank)...", GH_ParamAccess.list);
            pManager.AddIntervalParameter("Frequency Scope", "Oct", "An interval of the first and last octave to calculate (0 = 62.5 Hz, 1 = 125 HZ., ..., 7 = 8000 Hz.", GH_ParamAccess.item);

            Grasshopper.Kernel.Parameters.Param_Interval param = (pManager[6] as Grasshopper.Kernel.Parameters.Param_Interval);
            if (param != null) param.SetPersistentData(new Grasshopper.Kernel.Types.GH_Interval(new Interval(0,7)));
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Ray Tracing Data", "RT", "The pachyderm ray tracing data object (a receiver object)", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Pachyderm_Acoustic.Environment.Polygon_Scene S = null;
            DA.GetData<Pachyderm_Acoustic.Environment.Polygon_Scene>(0, ref S);
            int RayCt = 0;
            double CO_Time = 0;
            int IS_Order = 0;
            DA.GetData<int>(1, ref RayCt);
            DA.GetData<double>(2, ref CO_Time);
            DA.GetData<int>(3, ref IS_Order);
            List<Pachyderm_Acoustic.Environment.Source> Src = new List<Pachyderm_Acoustic.Environment.Source>();
            DA.GetDataList<Pachyderm_Acoustic.Environment.Source>(4, Src);
            List<Pachyderm_Acoustic.Environment.Receiver_Bank> Rec = new List<Pachyderm_Acoustic.Environment.Receiver_Bank>();
            DA.GetDataList<Pachyderm_Acoustic.Environment.Receiver_Bank>(5, Rec);
            Rhino.Geometry.Interval I = new Interval();
            DA.GetData<Rhino.Geometry.Interval>(6, ref I);

            List<int> scope = new List<int>();

            for(int i = (int)I.T0; i < (int)I.T1+1; i++) scope.Add(i);

            int s_id = 0;
            foreach (Pachyderm_Acoustic.Environment.Source Pt in Src)
            {
                Pachyderm_Acoustic.SplitRayTracer RT = new Pachyderm_Acoustic.SplitRayTracer(Pt, Rec[s_id], S, CO_Time, RayCt, scope.ToArray(), IS_Order);
                RT.Begin();
                do { System.Threading.Thread.Sleep(100); } while (RT.ThreadState() == System.Threading.ThreadState.Running);
                RT.Combine_ThreadLocal_Results();
                s_id++;
                if (RT.GetReceiver.GetType() == typeof(Pachyderm_Acoustic.Mapping.PachMapReceiver))
                {
                    DA.SetData(0, RT.GetReceiver as Pachyderm_Acoustic.Mapping.PachMapReceiver);
                }
                else
                {
                    DA.SetData(0, RT.GetReceiver);
                }
            }
        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                // You can add image files to your project resources and access them like this:
                //return Resources.IconForThisComponent;
                return null;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{F7C610DC-304B-4BB7-AC7E-D63079FD2B6A}"); }
        }
    }
}