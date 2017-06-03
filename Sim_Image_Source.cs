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

namespace PachydermGH
{
    public class Image_Source : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public Image_Source()
            : base("Image Source", "Pach_IS",
                "Performs Snell's Law calculations on a model, including time delays",
                "Acoustics", "Computation")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Room Model", "Room", "The Pachyderm Room Model Reference", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Reflection_Order", "Order", "The highest order or reflection to be obtained.", GH_ParamAccess.item);
            pManager.AddGenericParameter("Source", "Src", "Sound Source Objects...", GH_ParamAccess.list);
            pManager.AddGenericParameter("Receiver", "Rec", "Listening Object (Receiver_Bank)...", GH_ParamAccess.list);
            pManager.AddBooleanParameter("Edge Diffraction", "ED", "Calculates Biot Tolstoy Medwin Edge Diffraction, along with Image Source...", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Image Source Data", "IS", "The pachyderm image source data object", GH_ParamAccess.list);
            pManager.AddTextParameter("ReflectionTag", "PT", "The unique descriptive identifier for each reflection", GH_ParamAccess.list);
            pManager.AddCurveParameter("Reflections", "P", "Curves indicating the Snell's Law paths of sound.", GH_ParamAccess.list);
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
            int order = 0;
            DA.GetData<int>(1, ref order);
            List<Pachyderm_Acoustic.Environment.Source> Src = new List<Pachyderm_Acoustic.Environment.Source>();
            DA.GetDataList<Pachyderm_Acoustic.Environment.Source>(2, Src);
            List<Pachyderm_Acoustic.Environment.Receiver_Bank> Rec = new List<Pachyderm_Acoustic.Environment.Receiver_Bank>();
            DA.GetDataList<Pachyderm_Acoustic.Environment.Receiver_Bank>(3, Rec);
            Boolean Edges = false;
            DA.GetData<Boolean>(4, ref Edges);

            if (Edges) S.Register_Edges(Src, Rec[0]);

            int ct = 0;
            int s_id = 0;
            List<Grasshopper.Kernel.Types.GH_Curve> cvs = new List<Grasshopper.Kernel.Types.GH_Curve>();

            foreach (Pachyderm_Acoustic.Environment.Source Pt in Src)
            {
                //Pachyderm_Acoustic.Environment.Receiver_Bank RB = new Pachyderm_Acoustic.Environment.Receiver_Bank(Rec, Pt, S, 0, 1000, 1000, Pachyderm_Acoustic.Environment.Receiver_Bank.Type.Stationary); 
                Pachyderm_Acoustic.Direct_Sound DS = new Pachyderm_Acoustic.Direct_Sound(Pt, Rec[ct], S, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 });
                DS.Begin();
                do { System.Threading.Thread.Sleep(100); } while (DS.ThreadState() == System.Threading.ThreadState.Running);
                DS.Combine_ThreadLocal_Results();
                Pachyderm_Acoustic.ImageSourceData IS = new Pachyderm_Acoustic.ImageSourceData(Pt, Rec[ct], DS, S, order, s_id);
                IS.Begin();
                do { System.Threading.Thread.Sleep(100); } while (IS.ThreadState() == System.Threading.ThreadState.Running);
                IS.Combine_ThreadLocal_Results();
                if (Rec.Count != 0) ct++;
                s_id++;

                DA.SetData(0, IS);
                
                if (IS.Paths.Length > 0)
                {
                    for (int i = 0; i < Rec.Count; i++)
                    {
                        foreach (Pachyderm_Acoustic.Deterministic_Reflection R in IS.Paths[i])
                        {
                            Polyline[] path = new Polyline[R.Path.Length];
                            for (int j = 0; j < R.Path.Length; j++)
                            {
                                path[j] = new Polyline();
                                foreach (Hare.Geometry.Point pt in R.Path[j])
                                {
                                    path[j].Add(pt.x, pt.y, pt.z);
                                }
                                for (int k = 0; k < path.Length; k++) cvs.Add(new Grasshopper.Kernel.Types.GH_Curve(path[k].ToNurbsCurve()));
                            }
                        }
                    }
                }
            }
            DA.SetDataList(2, cvs);
        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                System.Drawing.Bitmap b = Properties.Resources.Image_Source;
                b.MakeTransparent(System.Drawing.Color.White);
                return b;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{fc74dce2-7595-497f-a8b2-a2654a735da4}"); }
        }
    }
}