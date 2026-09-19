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
using Pachyderm_Acoustic.Simulation;
using System.Linq;

namespace PachydermGH
{
    [IoId("F17E825B-D49F-4C9C-B4EB-1850121E8A5E")]
    public class FDBEM : Component
    {
        /// <summary>
        /// Each implementation of Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public FDBEM()
            : base(new Nomen("FD Bounaccessry Element Method",
                "Performs the Frequency Domain Bounaccessry Element Method on a model",
                "Acoustics", "Computation"))
        {
        }

        public FDBEM(IReader reader) : base(reader) { }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void AddInputs(InputAdder inputs)
        {
            inputs.AddGeneric("Room Model", "Room", "The Pachyderm Room Model Reference", Access.Item);
            inputs.AddGeneric("Source", "Src", "Sound Source Objects...", Access.Tree);
            inputs.AddGeneric("Receiver", "Rec", "Listening Object (Receiver_Bank)...", Access.Tree);
            inputs.AddNumber("Frequencies", "F", "Frequencies to calculate", Access.Tree);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void AddOutputs(OutputAdder outputs)
        {
            outputs.AddComplex("Pressure Values", "P", "Sound pressure at receiver points. data is organized in the order of Source:Receiver:Frequency", Access.Tree);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="access">The access object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void Process(IDataAccess access)
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
            access.GetItem<Pachyderm_Acoustic.Environment.Polygon_Scene>(0, out S);
            Tree<Pachyderm_Acoustic.Environment.Source> Src;
            access.GetTree<Pachyderm_Acoustic.Environment.Source>(1, out Src);
            Tree<Pachyderm_Acoustic.Environment.Receiver_Bank> Rec;
            access.GetTree<Pachyderm_Acoustic.Environment.Receiver_Bank>(2, out Rec);
            Tree<double> frequencies;
            access.GetTree<double>(3,out frequencies);
            
            int s_id = 0;
            List<System.Numerics.Complex> results = new List<System.Numerics.Complex>();
            
            foreach (Pachyderm_Acoustic.Environment.Source Pt in Src.AllItems)
            {
                BoundaryElementSimulation_FreqDom BEM = new BoundaryElementSimulation_FreqDom(S, Pt, Rec.Items[0], frequencies.AllItems.ToArray());
                BEM.Begin();
                do { System.Threading.Thread.Sleep(100); } while (BEM.ThreadState() != System.Threading.ThreadState.Stopped);
                BEM.Combine_ThreadLocal_Results();
                s_id++;
                
                if (BEM.Results.Length > 0 && BEM.Results[0].Length > 0)
                {
                    for (int i = 0; i < Rec.Items[0].Count; i++)
                    {
                        for (int f = 0; f < frequencies.ItemCount; f++)
                        {
                            results.Add(new System.Numerics.Complex(BEM.Results[f][i].Real, BEM.Results[f][i].Imaginary));
                        }
                    }
                }
            }
            access.SetTree(0, Garden.TreeFromList(results));
            P.PriorityClass = System.Diagnostics.ProcessPriorityClass.Normal;
        }
        protected override IIcon IconInternal
        {
            get
            {
                var assembly = typeof(SPLETC).Assembly;
                var resourceName = "Pachyderm_GH.Icons.Image_Source.png";

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
        //protected override IIcon IconInternal => new Grasshopper2.UI.Icon.PixelIcon("Image_Source");
    }
}