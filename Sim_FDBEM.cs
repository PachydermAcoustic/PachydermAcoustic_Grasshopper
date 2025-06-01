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
using System.Collections.Generic;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Pachyderm_Acoustic.Simulation;

namespace PachydermGH
{
    public class FDBEM : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public FDBEM()
            : base("FD Boundary Element Method", "Pach_FDBEM",
                "Performs the Frequency Domain Boundary Element Method on a model",
                "Acoustics", "Computation")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Room Model", "Room", "The Pachyderm Room Model Reference", GH_ParamAccess.item);
            pManager.AddGenericParameter("Source", "Src", "Sound Source Objects...", GH_ParamAccess.list);
            pManager.AddGenericParameter("Receiver", "Rec", "Listening Object (Receiver_Bank)...", GH_ParamAccess.list);
            pManager.AddNumberParameter("Frequencies", "F", "Frequencies to calculate", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddComplexNumberParameter("Pressure Values", "P", "Sound pressure at receiver points. Data is organized in the order of Source:Receiver:Frequency", GH_ParamAccess.tree);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            System.Diagnostics.Process P = System.Diagnostics.Process.GetCurrentProcess();
            switch (Pachyderm_Acoustic.UI.PachydermAc_PlugIn.Instance.TaskPriority)
            {
                case 0:
                    {
                        P.PriorityClass = System.Diagnostics.ProcessPriorityClass.High;
                        break;
                    }
                case 1:
                    {
                        P.PriorityClass = System.Diagnostics.ProcessPriorityClass.AboveNormal;
                        break;
                    }
                case 2:
                    {
                        P.PriorityClass = System.Diagnostics.ProcessPriorityClass.Normal;
                        break;
                    }
            }

            Pachyderm_Acoustic.Environment.Polygon_Scene S = null;
            DA.GetData<Pachyderm_Acoustic.Environment.Polygon_Scene>(0, ref S);
            List<Pachyderm_Acoustic.Environment.Source> Src = new List<Pachyderm_Acoustic.Environment.Source>();
            DA.GetDataList<Pachyderm_Acoustic.Environment.Source>(1, Src);
            List<Pachyderm_Acoustic.Environment.Receiver_Bank> Rec = new List<Pachyderm_Acoustic.Environment.Receiver_Bank>();
            DA.GetDataList<Pachyderm_Acoustic.Environment.Receiver_Bank>(2, Rec);
            List<double> frequencies = new List<double>();
            DA.GetDataList<double>(3, frequencies);
            
            int s_id = 0;
            Grasshopper.DataTree<Complex> results = new Grasshopper.DataTree<Complex>();
            
            foreach (Pachyderm_Acoustic.Environment.Source Pt in Src)
            {
                BoundaryElementSimulation_FreqDom BEM = new BoundaryElementSimulation_FreqDom(S, Pt, Rec[0], frequencies.ToArray());
                BEM.Begin();
                do { System.Threading.Thread.Sleep(100); } while (BEM.ThreadState() != System.Threading.ThreadState.Stopped);
                BEM.Combine_ThreadLocal_Results();
                s_id++;
                
                if (BEM.Results.Length > 0 && BEM.Results[0].Length > 0)
                {
                    for (int i = 0; i < Rec[0].Count; i++)
                    {
                        for (int f = 0; f < frequencies.Count; f++)
                        {
                            results.Add(new Complex(BEM.Results[f][i].Real, BEM.Results[f][i].Imaginary), new Grasshopper.Kernel.Data.GH_Path(new int[] { i, f}));
                        }
                    }
                }
            }
            DA.SetDataTree(0, results);
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
            get { return new Guid("{F17E825B-D49F-4C9C-B4EB-1850121E8A5E}"); }
        }
    }
}