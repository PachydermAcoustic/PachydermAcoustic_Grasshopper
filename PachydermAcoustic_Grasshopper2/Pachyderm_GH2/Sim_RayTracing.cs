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
            Threading = Grasshopper2.Components.ThreadingState.SingleThreaded;
        }

        public RayTracing(IReader reader) : base(reader) { Threading = Grasshopper2.Components.ThreadingState.SingleThreaded; }

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
            inputs.AddInterval("Frequency Scope", "Oct", "An interval of the first and last octave to calculate (0 = 62.5 Hz, 1 = 125 HZ., ..., 7 = 8000 Hz.", Access.Item).Set(new Rhino.Geometry.Interval(0,7));
            
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

        protected override void Process(IDataAccess access)
        {
            if(!access.GetItem<Pachyderm_Acoustic.Environment.Scene>(0,out var scene) || scene==null) throw new ArgumentException("Provide a room scene.");
            access.GetItem<int>(1,out int rays); access.GetItem<double>(2,out double cutoff); access.GetItem<int>(3,out int order);
            var sources=ComponentSupport.Items<Pachyderm_Acoustic.Environment.Source>(access,4);
            var banks=ComponentSupport.Items<Receiver_Bank>(access,5);
            access.GetItem<Interval>(6,out var interval); ComponentSupport.Octaves(interval,out int first,out int last);
            if(sources.Length==0 || banks.Length==0 || cutoff<=0 || order<0) throw new ArgumentException("Provide sources, receivers, positive cutoff and nonnegative image order.");
            var result=new List<Receiver_Bank>();
            using(var cancellation=CancellationTokenSource.CreateLinkedTokenSource(access.Solution.Token)) {
                EventHandler escape=(sender,args)=>cancellation.Cancel();
                ConvergenceProgress progress=null;
                Rhino.RhinoApp.EscapeKeyPressed+=escape;
                try {
                    Rhino.RhinoApp.InvokeAndWait(()=>{progress=new ConvergenceProgress(cancellation,banks[0].SampleRate); progress.Show();});
                    for(int s=0;s<sources.Length;s++) {
                        cancellation.Token.ThrowIfCancellationRequested();
                        var bank=ComponentSupport.Bank(banks,sources[s],scene,s,sources.Length);
                        var tracer=new Pachyderm_Acoustic.SplitRayTracer(sources[s],bank,scene,cutoff,new[]{first,last},order,rays<0?-1:rays,progress);
                        var task=Pachyderm_Acoustic.Utilities.RCPachTools.RunSimulation(tracer,false);
                        // The core owns its worker threads; do not dispose its progress UI until it exits.
                        var completed=task.GetAwaiter().GetResult() as Pachyderm_Acoustic.SplitRayTracer;
                        cancellation.Token.ThrowIfCancellationRequested();
                        if(completed==null) throw new InvalidOperationException("Ray tracing returned no result.");
                        result.Add(completed.GetReceiver);
                    }
                    ComponentSupport.SetTree(access, 0,Garden.TreeFromList(result));
                } finally {
                    Rhino.RhinoApp.EscapeKeyPressed-=escape;
                    if(progress!=null) Rhino.RhinoApp.InvokeAndWait(()=>{progress.Close();progress.Dispose();});
                }
            }
        }
        protected override IIcon IconInternal
        {
            get
            {
                var assembly = typeof(SPLETC).Assembly;
                var resourceName = "PachydermGH2.Resources.Ray Tracing.png";

                using (var stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream == null) return null;

                    // FromStream reads serialized .ghicon data, not PNG/BMP images.
                    // The PixelIcon retains the bitmap for its cached lifetime.
                    return new Grasshopper2.UI.Icon.PixelIcon(new Eto.Drawing.Bitmap(stream));
                }
            }
        }

        //protected override IIcon IconInternal => new Grasshopper2.UI.Icon.PixelIcon("Ray_Tracing");
    }
}
