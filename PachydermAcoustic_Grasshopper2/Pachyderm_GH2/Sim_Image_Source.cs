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
using System.Linq;

namespace PachydermGH
{
    [IoId("fc74dce2-7595-497f-a8b2-a2654a735DA4")]
    public class Image_Source : Component
    {
        /// <summary>
        /// Each implementation of Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public Image_Source()
            : base(new Nomen("Image Source",
                "Performs Snell's Law calculations on a model, including time delays",
                "Acoustics", "Computation"))
        {
            Threading = Grasshopper2.Components.ThreadingState.SingleThreaded;
        }

        public Image_Source(IReader reader) : base(reader) { Threading = Grasshopper2.Components.ThreadingState.SingleThreaded; }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void AddInputs(InputAdder inputs)
        {
            inputs.AddGeneric("Room Model", "Room", "The Pachyderm Room Model Reference", Access.Item);
            inputs.AddInteger("Reflection_Order", "Order", "The highest order or reflection to be obtained.", Access.Item);
            inputs.AddGeneric("Source", "Src", "Sound Source Objects...", Access.Tree);
            inputs.AddGeneric("Receiver", "Rec", "Listening Object (Receiver_Bank)...", Access.Tree);
            inputs.AddBoolean("Edge Diffraction", "ED", "Calculates Biot Tolstoy Medwin Edge Diffraction, along with Image Source...", Access.Item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void AddOutputs(OutputAdder outputs)
        {
            outputs.AddGeneric("Image Source data", "IS", "The pachyderm image source data object", Access.Tree);
            outputs.AddText("ReflectionTag", "PT", "The unique descriptive identifier for each reflection", Access.Tree);
            outputs.AddCurve("Reflections", "P", "Curves indicating the Snell's Law paths of sound.", Access.Tree);
            outputs.AddNumber("Intensity", "I", "Sound Intensity of each reflection. data is organized in the order of Source:Receiver:Octave Band", Access.Tree);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="access">The access object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void Process(IDataAccess access)
        {
            Pachyderm_Acoustic.Environment.Polygon_Scene S = null;
            access.GetItem<Pachyderm_Acoustic.Environment.Polygon_Scene>(0, out S);
            int order = 0;
            access.GetItem<int>(1, out order);
            Tree<Pachyderm_Acoustic.Environment.Source> Src;
            access.GetTree<Pachyderm_Acoustic.Environment.Source>(2, out Src);
            Tree<Pachyderm_Acoustic.Environment.Receiver_Bank> Rec;
            access.GetTree<Pachyderm_Acoustic.Environment.Receiver_Bank>(3, out Rec);
            Boolean Edges = false;
            access.GetItem<Boolean>(4, out Edges);

            if (Edges) { if (Rec.ItemCount==0) throw new ArgumentException("Provide receiver banks."); S.Register_Edges(Src.AllItems, Rec.Items[0]); }

            int ct = 0;
            int s_id = 0;
            List<Curve> cvs = new List<Curve>();
            List<string> txt = new List<string>();
            List<double> I = new List<double>();
            List<Pachyderm_Acoustic.ImageSourceData> ISS = new List<Pachyderm_Acoustic.ImageSourceData>();

            foreach (Pachyderm_Acoustic.Environment.Source Pt in Src.AllItems)
            {
                var receiverBank = ComponentSupport.Bank(System.Linq.Enumerable.ToArray(Rec.AllItems), Pt, S, s_id, Src.ItemCount);
                Pachyderm_Acoustic.Direct_Sound DS = new Pachyderm_Acoustic.Direct_Sound(Pt, receiverBank, S, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 });
                DS.Delay_ms = ComponentSupport.GetDelay(Pt);
                DS.Begin();
                do { System.Threading.Thread.Sleep(100); } while (DS.ThreadState() == System.Threading.ThreadState.Running);
                DS.Combine_ThreadLocal_Results();
                Pachyderm_Acoustic.ImageSourceData IS = new Pachyderm_Acoustic.ImageSourceData(Pt, receiverBank, DS, S, order, Edges, s_id);
                IS.Begin();
                do { System.Threading.Thread.Sleep(100); } while (IS.ThreadState() == System.Threading.ThreadState.Running);
                IS.Combine_ThreadLocal_Results();
                if (Rec.ItemCount != 0) ct++;
                s_id++;

                ISS.Add(IS);
                
                if (IS.Paths.Length > 0)
                {
                    for (int i = 0; i < receiverBank.Count; i++)
                    {
                        for (int h = 0; h < IS.Paths[i].Count; h++)
                        {
                            Polyline[] path = new Polyline[(int)Math.Ceiling(IS.Paths[i][h].Path.Length / 100.0)];//IS.Paths[i][h].Path.Length];

                            int step = 100;// (int)Math.Ceiling((double)(IS.Paths[i][h].Path.Length / 50))-1;
                            for (int j = 0; j < path.Length; j++)
                            {
                                path[j] = new Polyline();
                                foreach (Hare.Geometry.Point pt in IS.Paths[i][h].Path[j*step])
                                {
                                    path[j].Add(pt.x, pt.y, pt.z);
                                }
                            }
                            List<double> I_oct = new List<double>();
                            IS.Paths[i][h].Create_Filter(16384, 0);
                            for (int oct = 0; oct < 8; oct++) I.Add(IS.Paths[i][h].Energy(oct, 44100)[0]);
                            txt.Add(IS.Paths[i][h].ToString());
                            for (int k = 0; k < path.Length; k++) if (path[k] != null) cvs.Add(path[k].ToNurbsCurve());
                        }
                    }
                }
            }
            ComponentSupport.SetTree(access, 0, Garden.TreeFromList(ISS));
            ComponentSupport.SetTree(access, 1, Garden.TreeFromList(txt));
            ComponentSupport.SetTree(access, 2, Garden.TreeFromList(cvs));
            ComponentSupport.SetTree(access, 3, Garden.TreeFromList(I));
            
        }
        protected override IIcon IconInternal
        {
            get
            {
                var assembly = typeof(SPLETC).Assembly;
                var resourceName = "PachydermGH2.Resources.Image Source.png";

                using (var stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream == null) return null;

                    // FromStream reads serialized .ghicon data, not PNG/BMP images.
                    // The PixelIcon retains the bitmap for its cached lifetime.
                    return new Grasshopper2.UI.Icon.PixelIcon(new Eto.Drawing.Bitmap(stream));
                }
            }
        }
        //protected override IIcon IconInternal => new Grasshopper2.UI.Icon.PixelIcon("Image_Source");
    }
}
