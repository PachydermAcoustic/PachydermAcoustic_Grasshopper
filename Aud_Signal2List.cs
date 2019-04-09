//'Pachyderm-Acoustic: Geometrical Acoustics for Rhinoceros (GPL) by Arthur van der Harten 
//' 
//'This file is part of Pachyderm-Acoustic. 
//' 
//'Copyright (c) 2008-2019, Arthur van der Harten 
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
    public class Signal2List : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public Signal2List()
            : base("Signal2List", "SignalOut",
                "Casts a signal to a list readable in Grasshopper",
                "Acoustics", "Audio")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Audio Signal", "Signal", "The data to divide...", GH_ParamAccess.item);
            pManager.AddIntervalParameter("Samples", "Domain", "Which samples to return", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Channel", "Ch", "Which channel to convert...", GH_ParamAccess.item);

            pManager[1].Optional = true;
            pManager[2].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("samples", "S", "The exported signal...", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Sample Frequency", "FS", "Number of samples per second", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            int chan = 0;
            Audio_Signal Buffer = new Audio_Signal();
            Interval domain = new Interval();
            DA.GetData<Audio_Signal>(0, ref Buffer);
            DA.GetData<Interval>(1, ref domain);
            DA.GetData<int>(2, ref chan);

            double[] SignalBuffer = Buffer[chan];

            List<double> signal = new List<double>();
            for (int i = (int)domain.Min; i < (int)domain.Max; i++) { signal.Add(SignalBuffer[i]); }
            //foreach (float s in SignalBuffer) signal.Add(s);

            DA.SetDataList(0, signal);
            DA.SetData(1, Buffer.SampleFrequency);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                System.Drawing.Bitmap b = Properties.Resources.Signal_to_List;
                b.MakeTransparent(System.Drawing.Color.White);
                return b;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{E3E18F3F-687C-4E5B-95D0-D4F8508AB15B}"); }
        }
    }
}