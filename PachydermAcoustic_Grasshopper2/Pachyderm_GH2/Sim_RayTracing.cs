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
using Grasshopper2.Components;
using Grasshopper2.Parameters;
using Grasshopper2.UI;
using Grasshopper2.UI.Icon;
using GrasshopperIO;
using Pachyderm_Acoustic;
using Pachyderm_Acoustic.Environment;
using Rhino.Geometry;
using Grasshopper2.Data;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Linq;
using Pachyderm_Acoustic.UI;
using Grasshopper2.UI.InputPanel;
using System.Threading;

namespace PachydermGH
{
    [IoId("F7C610DC-304B-4BB7-AC7E-D63079FD2B6A")]
    public class RayTracing : Component
    {
        /// <summary>
        /// Each implementation of Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public RayTracing()
            : base(new Nomen("Ray Tracing",
                "Performs Ray Tracing (Specular and Diffuse) calculations on the geometry specified.",
                "Acoustics", "Computation"))
        {
        }

        public RayTracing(IReader reader) : base(reader) { }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void AddInputs(InputAdder inputs)
        {
            inputs.AddGeneric("Room Model", "Room", "The Pachyderm Room Model Reference", Access.Item);
            inputs.AddInteger("Number of Rays", "RayCt", "The number of rays initially used. Alternatively, a zero value will use minimum convergence, and a value less than zero will use detailed convergence.", new Grasshopper2.UI.UiInteger(0), Access.Item);
            inputs.AddNumber("Cut off time", "CO_Time", "The number of miliseconds that will be calculated for.", Access.Item);
            inputs.AddInteger("Image Source Order", "IS_Order", "Ray tracing will ignore specular reflections up to this order... in order to combine results with deterministic image source results.", Access.Item);
            inputs.AddGeneric("Source", "Src", "Sound Source Objects...", Access.Tree);
            inputs.AddGeneric("Receiver", "Rec", "Listening Object (Receiver_Bank)...", Access.Tree);
            inputs.AddInterval("Frequency Scope", "Oct", "An interval of the first and last octave to calculate (0 = 62.5 Hz, 1 = 125 HZ., ..., 7 = 8000 Hz.", Access.Item);
            
            //Grasshopper.Kernel.Parameters.Param_Interval param_I = (inputs[6] as Grasshopper.Kernel.Parameters.Param_Interval);
            //if (param_I != null) param_I.SetPersistentdata(new Grasshopper.Kernel.Types.GH_Interval(new Interval(0,7)));
            //inputs[1].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void AddOutputs(OutputAdder outputs)
        {
            outputs.AddGeneric("Ray Tracing data", "Tr", "The pachyderm ray tracing data object (a receiver object)", Access.Tree);
        }

        CancellationTokenSource CTS = new CancellationTokenSource();

        bool CancelCalc = false;

        private void Escape(object sender, System.EventArgs e)
        {
            CancelCalc = true;
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="access">The access object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void Process(IDataAccess access)
        {
            Pachyderm_Acoustic.Environment.Scene S = null;
            access.GetItem<Pachyderm_Acoustic.Environment.Scene>(0, out S);
            int RayCt = 0;
            double CO_Time = 0;
            int IS_Order = 0;

            access.GetItem<int>(1, out RayCt);

            //Set to conform to Pachyderm's conventions.
            if (RayCt == 0) RayCt = 0;
            else if (RayCt < 0) RayCt = -1;

            access.GetItem<double>(2, out CO_Time);
            access.GetItem<int>(3, out IS_Order);
            Tree<Pachyderm_Acoustic.Environment.Source> Src;
            access.GetTree<Pachyderm_Acoustic.Environment.Source>(4, out Src);
            Tree<Pachyderm_Acoustic.Environment.Receiver_Bank> Rec;
            access.GetTree<Pachyderm_Acoustic.Environment.Receiver_Bank>(5, out Rec);
            Rhino.Geometry.Interval I = new Interval();
            access.GetItem<Rhino.Geometry.Interval>(6, out I);
            List<int> scope = new List<int>();
            scope.Add((int)I.T0);
            scope.Add((int)I.T1);
            Rhino.RhinoApp.EscapeKeyPressed += Escape;
            CancelCalc = false;
            Rhino.ApplicationSettings.FileSettings.AutoSaveEnabled = false;

            List<Pachyderm_Acoustic.Environment.Receiver_Bank> RTS = new List<Receiver_Bank>();

            ConvergenceProgress CPS = null;

            Rhino.RhinoApp.InvokeAndWait(() => { CPS = new ConvergenceProgress(CTS, Rec.Items[0].SampleRate); CPS.Show();});


            int s_id = 0;
            try
            {
                for (int i = 0; i < Src.ItemCount; i++)
                {
                    Pachyderm_Acoustic.SplitRayTracer RT = new Pachyderm_Acoustic.SplitRayTracer(Src.Items[i], Rec.ItemCount == Src.ItemCount ? Rec.Items[s_id] : Rec.Items[0].Duplicate(Src.Items[i], S), S, CO_Time, scope.ToArray(), IS_Order, RayCt, CPS);
                    TaskAwaiter<Simulation_Type> TRTA = Pachyderm_Acoustic.Utilities.RCPachTools.RunSimulation(RT, false).GetAwaiter();
                    while (!TRTA.IsCompleted) System.Threading.Thread.Sleep(3000);

                    RT = TRTA.GetResult() as SplitRayTracer;

                    s_id++;
                    if (RT.GetReceiver.GetType() == typeof(Pachyderm_Acoustic.PachMapReceiver))
                    {
                        RTS.Add(RT.GetReceiver as Pachyderm_Acoustic.PachMapReceiver);
                    }
                    else
                    {
                        RTS.Add(RT.GetReceiver);
                    }

                    access.AddMessage(Grasshopper2.Doc.Message.Remark(string.Format("{0} Rays ({1} sub-rays) cast in {2} hours, {3} minutes, {4} seconds.", RT._currentRay.Sum(), RT._rayTotal.Sum(), RT._ts.Hours, RT._ts.Minutes, RT._ts.Seconds), ""));
               }

                access.SetTree(0,Garden.TreeFromList(RTS));
            }
            catch
            (System.IndexOutOfRangeException)
            {
                access.AddMessage(Grasshopper2.Doc.Message.Error("Raytracing operation failed. This can be due to an unsuitable scene object. For example, did you set materials on all layers referenced by Rhinoceros Geometry?", ""));
            }
        }
        protected override IIcon IconInternal
        {
            get
            {
                var assembly = typeof(SPLETC).Assembly;
                var resourceName = "Pachyderm_GH.Icons.Ray_Tracing.png";

                using (var stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream == null) return null;

                    var ms = new System.IO.MemoryStream();
                    stream.CopyTo(ms);
                    ms.Position = 0;
                    return Grasshopper2.UI.Icon.PixelIcon.FromStream(ms);
                }
            }
        }

        //protected override IIcon IconInternal => new Grasshopper2.UI.Icon.PixelIcon("Ray_Tracing");
    }
}