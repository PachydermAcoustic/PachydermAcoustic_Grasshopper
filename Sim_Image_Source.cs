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
            pManager.AddTextParameter("ReflectionTag", "PT", "The unique descriptive identifier for each reflection", GH_ParamAccess.tree);
            pManager.AddCurveParameter("Reflections", "P", "Curves indicating the Snell's Law paths of sound.", GH_ParamAccess.tree);
            pManager.AddNumberParameter("Intensity", "I", "Sound Intensity of each reflection. Data is organized in the order of Source:Receiver:Octave Band", GH_ParamAccess.tree);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            System.Diagnostics.Process P = System.Diagnostics.Process.GetCurrentProcess();
            P.PriorityClass = System.Diagnostics.ProcessPriorityClass.High;

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
            Grasshopper.DataTree<Grasshopper.Kernel.Types.GH_Curve> cvs = new Grasshopper.DataTree<Grasshopper.Kernel.Types.GH_Curve>();
            Grasshopper.DataTree<string> txt = new Grasshopper.DataTree<string>();
            Grasshopper.DataTree<double> I = new Grasshopper.DataTree<double>();
            Grasshopper.DataTree<Pachyderm_Acoustic.ImageSourceData> ISS = new Grasshopper.DataTree<Pachyderm_Acoustic.ImageSourceData>();

            foreach (Pachyderm_Acoustic.Environment.Source Pt in Src)
            {
                Pachyderm_Acoustic.Direct_Sound DS = new Pachyderm_Acoustic.Direct_Sound(Pt, Rec[0], S, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 });
                DS.Begin();
                do { System.Threading.Thread.Sleep(100); } while (DS.ThreadState() == System.Threading.ThreadState.Running);
                DS.Combine_ThreadLocal_Results();
                Pachyderm_Acoustic.ImageSourceData IS = new Pachyderm_Acoustic.ImageSourceData(Pt, Rec[0], DS, S, order, s_id);
                IS.Begin();
                do { System.Threading.Thread.Sleep(100); } while (IS.ThreadState() == System.Threading.ThreadState.Running);
                IS.Combine_ThreadLocal_Results();
                if (Rec.Count != 0) ct++;
                s_id++;

                ISS.Add(IS);
                
                if (IS.Paths.Length > 0)
                {
                    for (int i = 0; i < Rec[0].Count; i++)
                    {
                        for(int h = 0; h < IS.Paths[i].Count; h++)
                        {
                            Polyline[] path = new Polyline[IS.Paths[i][h].Path.Length];
                            for (int j = 0; j < IS.Paths[i][h].Path.Length; j++)
                            {
                                path[j] = new Polyline();
                                foreach (Hare.Geometry.Point pt in IS.Paths[i][h].Path[j])
                                {
                                    path[j].Add(pt.x, pt.y, pt.z);
                                }
                                List<double> I_oct = new List<double>();
                                for (int oct = 0; oct < 8; oct++) I.Add(IS.Paths[i][h].Energy(oct, 44100)[0], new Grasshopper.Kernel.Data.GH_Path(new int[] {i, h, oct}));
                                txt.Add(IS.Paths[i][h].ToString(), new Grasshopper.Kernel.Data.GH_Path(new int[] {i ,h}));
                                for (int k = 0; k < path.Length; k++)if (path[k] != null) cvs.Add(new Grasshopper.Kernel.Types.GH_Curve(path[k].ToNurbsCurve()), new Grasshopper.Kernel.Data.GH_Path(new int[] { i, h }));
                            }
                        }
                    }
                }
            }
            DA.SetDataTree(0, ISS);
            DA.SetDataTree(1, txt);
            DA.SetDataTree(2, cvs);
            DA.SetDataTree(3, I);
            P.PriorityClass = System.Diagnostics.ProcessPriorityClass.Normal;
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