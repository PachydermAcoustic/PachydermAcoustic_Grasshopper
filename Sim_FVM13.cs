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
using Pachyderm_Acoustic.Numeric.TimeDomain;
using Pachyderm_Acoustic.Environment;
namespace PachydermGH
{
    public class Sim_FVM13 : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public Sim_FVM13()
            : base("Finite Volume Method", "FVM13",
                "Performs a comprehensive Finite Volume Method Simulation.",
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
            pManager.AddNumberParameter("Frequency", "F", "Characteristic frequency of the source signal... Could be the upper limit of the sinc or noise burst, or the tonal frequency of a sine wave.", GH_ParamAccess.item);
            pManager.AddNumberParameter("CuttoffTime IN Ms", "Tms", "The total amount of time that will be simulated...", GH_ParamAccess.item);
            pManager.AddBoxParameter("Bounds", "Bds", "The extents of the simulated space. Only the portion inside this box will be simulated. Axis Aligned Bounding Boxes only please...", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Signal", "DS", "Signal data from receivers in the model.", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Polygon_Scene S = null;
            DA.GetData<Polygon_Scene>(0, ref S);
            List<Source> Src = new List<Pachyderm_Acoustic.Environment.Source>();
            DA.GetDataList<Source>(1, Src);
            List<Receiver_Bank> Rec = new List<Pachyderm_Acoustic.Environment.Receiver_Bank>();
            DA.GetDataList<Receiver_Bank>(2, Rec);
            double freq = 0;
            DA.GetData<double>(3, ref freq);
            double tmaxms = 0;
            DA.GetData<double>(4, ref tmaxms);
            //Get bounding box from interface...
            Grasshopper.Kernel.Types.GH_Box BB = new Grasshopper.Kernel.Types.GH_Box();
            DA.GetData<Grasshopper.Kernel.Types.GH_Box>(5, ref BB);

            for (int i = 0; i < Rec.Count; i++)
            {
                Signal_Driver_Compact Sig = new Signal_Driver_Compact(Signal_Driver_Compact.Signal_Type.Sine_Pulse, freq, 1, Src.ToArray());
                Microphone_Compact Mic = new Microphone_Compact(Rec[i].Origins());
                Acoustic_Compact_FDTD FVM = new Acoustic_Compact_FDTD(S, ref Sig, ref Mic, freq, tmaxms * 2, Acoustic_Compact_FDTD.GridType.TransparencyLab, Pachyderm_Acoustic.Utilities.RC_PachTools.RPttoHPt(BB.Value.Center), BB.Value.X.Length, BB.Value.Y.Length, BB.Value.Z.Length, false);
                FVM.RuntoCompletion();
                Mic.reset();
                Audio_Signal AS = new Audio_Signal(Mic.Recordings()[0], (int)FVM.SampleFrequency);
                DA.SetData(0, AS);
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
                System.Drawing.Bitmap b = Properties.Resources.Direct_Sound;
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
            get { return new Guid("{0CCF6735-411F-4833-8FB0-B73500F8C6E9}"); }
        }
    }
}