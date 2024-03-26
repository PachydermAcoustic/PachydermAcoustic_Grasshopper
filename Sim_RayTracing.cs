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
using Pachyderm_Acoustic;
using Pachyderm_Acoustic.Environment;
using Pachyderm_Acoustic.UI;
using Rhino.Geometry;
using System.Windows.Forms;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Linq;

namespace PachydermGH
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
            pManager[1].Optional = true;
        }

        protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu)
        {
            Menu_AppendItem(menu, "Trace Specified Number of Rays", RayNo_Click, true, ByRayNo);
            Menu_AppendItem(menu, "Minimum Convergence", MinCon_Click, true, MinConvergence);
            Menu_AppendItem(menu, "Detailed Convergence", DetCon_Click, true, DetConvergence);
            base.AppendAdditionalComponentMenuItems(menu);
        }

        private void RayNo_Click(object sender, EventArgs e)
        {
            ByRayNo = true;
            MinConvergence = false;
            DetConvergence = false;
        }
        private void MinCon_Click(object sender, EventArgs e)
        {
            ByRayNo = false;
            MinConvergence = true;
            DetConvergence = false;
        }
        private void DetCon_Click(object sender, EventArgs e)
        {
            ByRayNo = false;
            MinConvergence = false;
            DetConvergence = true;
        }
        public bool ByRayNo = false;
        public bool MinConvergence = true;
        public bool DetConvergence = false;

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Ray Tracing Data", "Tr", "The pachyderm ray tracing data object (a receiver object)", GH_ParamAccess.list);
        }

        bool CancelCalc = false;

        private void Escape(object sender, System.EventArgs e)
        {
            CancelCalc = true;
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override async void SolveInstance(IGH_DataAccess DA)
        {
            System.Diagnostics.Process P = System.Diagnostics.Process.GetCurrentProcess();
            P.PriorityClass = System.Diagnostics.ProcessPriorityClass.High;

            Pachyderm_Acoustic.Environment.Scene S = null;
            DA.GetData<Pachyderm_Acoustic.Environment.Scene>(0, ref S);
            int RayCt = 0;
            double CO_Time = 0;
            int IS_Order = 0;

            if (ByRayNo) DA.GetData<int>(1, ref RayCt);
            else if (MinConvergence) RayCt = -1;
            else if (DetConvergence) RayCt = 0;
            else throw new Exception("Ray count inappropriately specified. Try right clicking this component for options, or specify a ray count.");
            DA.GetData<double>(2, ref CO_Time);
            DA.GetData<int>(3, ref IS_Order);
            List<Pachyderm_Acoustic.Environment.Source> Src = new List<Pachyderm_Acoustic.Environment.Source>();
            DA.GetDataList<Pachyderm_Acoustic.Environment.Source>(4, Src);
            List<Pachyderm_Acoustic.Environment.Receiver_Bank> Rec = new List<Pachyderm_Acoustic.Environment.Receiver_Bank>();
            DA.GetDataList<Pachyderm_Acoustic.Environment.Receiver_Bank>(5, Rec);
            Rhino.Geometry.Interval I = new Interval();
            DA.GetData<Rhino.Geometry.Interval>(6, ref I);
            List<int> scope = new List<int>();
            scope.Add((int)I.T0);
            scope.Add((int)I.T1);
            Rhino.RhinoApp.EscapeKeyPressed += Escape;
            CancelCalc = false;
            Rhino.ApplicationSettings.FileSettings.AutoSaveEnabled = false;

            Grasshopper.DataTree<Pachyderm_Acoustic.Environment.Receiver_Bank> RTS = new Grasshopper.DataTree<Pachyderm_Acoustic.Environment.Receiver_Bank>();

            int s_id = 0;
            try
            {
                for (int i = 0; i < Src.Count; i++)
                {
                     User_Feedback Form = new User_Feedback();
                    Form.Display("Starting ray-tracing simulation...", string.Format("Ray-tracing source {0} of {1}", i, Src.Count) );
                    Form.Show();

                    ConvergenceProgress CP = new ConvergenceProgress();

                    Pachyderm_Acoustic.SplitRayTracer RT = new Pachyderm_Acoustic.SplitRayTracer(Src[i], Rec.Count == Src.Count ? Rec[s_id]: Rec[0].Duplicate(Src[i], S), S, CO_Time, scope.ToArray(), IS_Order, RayCt, CP);
                    if (!ByRayNo) CP.Show();
                    TaskAwaiter<Simulation_Type> TRTA = Pachyderm_Acoustic.Utilities.RCPachTools.RunSimulation(RT, false).GetAwaiter();
                    while (!TRTA.IsCompleted) System.Threading.Thread.Sleep(3000);

                    RT = TRTA.GetResult() as SplitRayTracer;

                    //RT.Begin();
                    //do
                    //{
                    //    if (CancelCalc)
                    //    {
                    //        //TODO - create terms for termination of simulation
                    //        //RT.Abort_Calculation();
                    //        Rhino.ApplicationSettings.FileSettings.AutoSaveEnabled = true;
                    //        throw new Exception("Simulation Canceled");
                    //    }
                    //    if (RT.ThreadState() != System.Threading.ThreadState.Running)
                    //    {
                    //        break;
                    //    }
                    //    System.Threading.Thread.Sleep(3000);
                    //    Form.Display(RT.ProgressMsg(), string.Format("Ray-tracing source {0} of {1}", i, Src.Count));
                    //} while (true);

                    //RT.Combine_ThreadLocal_Results();
                    //do
                    //{
                    //    System.Threading.Thread.Sleep(3000);
                    //    if (RT.ThreadState() != System.Threading.ThreadState.Running)
                    //    {
                    //        break;
                    //    }
                    //   Form.Display(RT.ProgressMsg(), string.Format("Ray-tracing source {0} of {1}", i, Src.Count));
                    //} while (true);

                    s_id++;
                    if (RT.GetReceiver.GetType() == typeof(Pachyderm_Acoustic.PachMapReceiver))
                    {
                        RTS.Add(RT.GetReceiver as Pachyderm_Acoustic.PachMapReceiver);
                    }
                    else
                    {
                        RTS.Add(RT.GetReceiver);
                    }

                    this.Message = string.Format("{0} Rays ({1} sub-rays) cast in {2} hours, {3} minutes, {4} seconds.", RT._currentRay.Sum(), RT._rayTotal.Sum(), RT._ts.Hours, RT._ts.Minutes, RT._ts.Seconds);

                    Form.Close();
                    Form.Dispose();
                }

                DA.SetDataTree(0,RTS);
            }
            catch
            (System.IndexOutOfRangeException)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Raytracing operation failed. This can be due to an unsuitable scene object. For example, did you set materials on all layers referenced by Rhinoceros Geometry?");
            }
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
                System.Drawing.Bitmap b = Properties.Resources.Ray_Tracing;
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
            get { return new Guid("{F7C610DC-304B-4BB7-AC7E-D63079FD2B6A}"); }
        }
    }
}