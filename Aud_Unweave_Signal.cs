//'Pachyderm-Acoustic: Geometrical Acoustics for Rhinoceros (GPL) by Arthur van der Harten 
//' 
//'This file is part of Pachyderm-Acoustic. 
//' 
//'Copyright (c) 2008-2018, Arthur van der Harten 
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
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace PachydermGH
{
    public class Unweave_Signal : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public Unweave_Signal()
            : base("Unweave Signal", "Unweave",
                "Unbraids woven signal to discrete signals by channel",
                "Acoustics", "Audio")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Input Data", "Signal", "The data to unweave...", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Channel Selection", "Ch", "Channel to crib from...", GH_ParamAccess.item);
            pManager.AddIntervalParameter("Time Interval", "Ival", "The samples to crib from the signal...", GH_ParamAccess.item);
            pManager[2].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Unwoven Signal", "Signals", "The resulting unwoven signal", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            int channel = 0;
            Audio_Signal Buffer = new Audio_Signal();
            DA.GetData<Audio_Signal>(0, ref Buffer);
            DA.GetData<int>(1, ref channel);
            Interval ival = new Interval();
            if (!DA.GetData<Interval>(2, ref ival)) ival = new Interval(0, Buffer.Count);
            
            float[] signals = new float[(int)ival.Length];
            for (int i = 0; i < signals.Length; i++)
            {
                signals[i] = (float)Buffer[channel][(int)ival.T0 + i];
            }

            DA.SetData(0, new Audio_Signal(signals, Buffer.SampleFrequency));
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                System.Drawing.Bitmap b = Properties.Resources.Unweave_Signal;
                b.MakeTransparent(System.Drawing.Color.White);
                return b;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{C1223793-ECAF-4E5E-9EDA-96A98A3CF0FF}"); }
        }
    }
}