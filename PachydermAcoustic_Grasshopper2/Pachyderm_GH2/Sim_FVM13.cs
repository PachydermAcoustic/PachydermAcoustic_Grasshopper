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
using Pachyderm_Acoustic.UI;
using System.Linq;
using Pachyderm_Acoustic.Numeric.TimeDomain;

namespace PachydermGH
{
    [IoId("0CCF6735-411F-4833-8FB0-B73500F8C6E9")]
    public class Sim_FVM13 : Component
    {
        /// <summary>
        /// Each implementation of Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public Sim_FVM13()
            : base(new Nomen("Finite Volume Method",
                "Performs a comprehensive Finite Volume Method Simulation.",
                "Acoustics", "Computation"))
        {
        }

        public Sim_FVM13(IReader reader) : base(reader) { }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void AddInputs(InputAdder inputs)
        {
            inputs.AddGeneric("Room Model", "Room", "The Pachyderm Room Model Reference", Access.Item);
            inputs.AddGeneric("Source", "Src", "Sound Source Objects...", Access.Tree);
            inputs.AddGeneric("Receiver", "Rec", "Listening Object (Receiver_Bank)...", Access.Tree);
            inputs.AddNumber("Frequency", "F", "Characteristic frequency of the source signal... Could be the upper limit of the sinc or noise burst, or the tonal frequency of a sine wave.", Access.Item);
            inputs.AddNumber("CuttoffTime IN Ms", "Tms", "The total amount of time that will be simulated...", Access.Item);
            inputs.AddBox("Bounds", "Bds", "The extents of the simulated space. Only the portion inside this box will be simulated. Axis Aligned Bounding Boxes only please...", Access.Item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void AddOutputs(OutputAdder outputs)
        {
            outputs.AddGeneric("Signal", "DS", "Signal data from receivers in the model.", Access.Tree);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="access">The access object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void Process(IDataAccess access)
        {
            System.Diagnostics.Process P = System.Diagnostics.Process.GetCurrentProcess();
            switch (PachydermAc_PlugIn.Instance.TaskPriority)
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

            Polygon_Scene S = null;
            access.GetItem<Polygon_Scene>(0, out S);
            Tree<Source> Src;
            access.GetTree<Source>(1,out Src);
            Tree<Receiver_Bank> Rec;
            access.GetTree<Receiver_Bank>(2,out Rec);
            double freq = 0;
            access.GetItem<double>(3, out freq);
            double tmaxms = 0;
            access.GetItem<double>(4, out tmaxms);
            //Get bounding box from interface...
            Box BB;
            access.GetItem<Box>(5, out BB);
            Pachyderm_Acoustic.Numeric.TimeDomain.Signal_Driver_Compact Sig = new Pachyderm_Acoustic.Numeric.TimeDomain.Signal_Driver_Compact(Pachyderm_Acoustic.Numeric.TimeDomain.Signal_Driver_Compact.Signal_Type.Sine_Pulse, freq, 1, Src.AllItems.ToArray());

            for (int i = 0; i < Rec.ItemCount; i++)
            {
                Microphone_Compact Mic = new Microphone_Compact(Rec.Items[i].Origins());
                Acoustic_Compact_FDTD FVM = new Acoustic_Compact_FDTD(S, ref Sig, ref Mic, freq, tmaxms * 2, Acoustic_Compact_FDTD.GridType.TransparencyLab, Pachyderm_Acoustic.Utilities.RCPachTools.RPttoHPt(BB.Center), BB.X.Length, BB.Y.Length, BB.Z.Length, false);
                FVM.RuntoCompletion();
                Mic.reset();
                Sig.reset(freq, Signal_Driver_Compact.Signal_Type.Sine_Pulse);
                Audio_Signal AS = new Audio_Signal(Mic.Recordings()[0], (int)FVM.SampleFrequency);
                access.SetItem(0, AS);
            }
            P.PriorityClass = System.Diagnostics.ProcessPriorityClass.Normal;
        }
        protected override IIcon IconInternal
        {
            get
            {
                var assembly = typeof(SPLETC).Assembly;
                var resourceName = "Pachyderm_GH.Icons.Direct_Sound.png";

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
    }
}