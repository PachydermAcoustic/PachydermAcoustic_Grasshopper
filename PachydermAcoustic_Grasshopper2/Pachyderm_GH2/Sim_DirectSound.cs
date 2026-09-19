using System.Linq;
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

namespace PachydermGH
{
    [IoId("24641469-B7D5-46D6-A4D7-DA194A447AFB")]
    public class Direct_Sound : Component
    {
        /// <summary>
        /// Each implementation of Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public Direct_Sound()
            : base(new Nomen("Direct Sound",
                "Calculates direct sound",
                "Acoustics", "Computation"))
        {
            Threading = Grasshopper2.Components.ThreadingState.SingleThreaded;
        }

        public Direct_Sound(IReader reader) : base(reader) { Threading = Grasshopper2.Components.ThreadingState.SingleThreaded; }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void AddInputs(InputAdder inputs)
        {
            inputs.AddGeneric("Room Model", "Room", "The Pachyderm Room Model Reference", Access.Item);
            inputs.AddGeneric("Source", "Src", "Sound Source Objects...", Access.Tree);
            inputs.AddGeneric("Receiver", "Rec", "Listening Object (Receiver_Bank)...", Access.Tree);
            inputs.AddBoolean("Screen Attenuation", "SCR", "Toggles the screen attenuation option. Obstructed receivers will receive energy based on the shortest clear path around obstructions.", Access.Item).Set(false);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void AddOutputs(OutputAdder outputs)
        {
            outputs.AddGeneric("Direct Sound data", "DS", "The pachyderm direct sound data", Access.Tree);
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
            Tree<Pachyderm_Acoustic.Environment.Source> Src;
            access.GetTree<Pachyderm_Acoustic.Environment.Source>(1, out Src);
            Tree<Pachyderm_Acoustic.Environment.Receiver_Bank> Rec;
            access.GetTree<Pachyderm_Acoustic.Environment.Receiver_Bank>(2, out Rec);
            bool screen = false;
            access.GetItem<bool>(3, out screen);
            List<Pachyderm_Acoustic.Direct_Sound> DSS = new List<Pachyderm_Acoustic.Direct_Sound>();

            
            int s_id = 0;
            foreach (Pachyderm_Acoustic.Environment.Source Pt in Src.AllItems)
            {
                var receiverBank = ComponentSupport.Bank(System.Linq.Enumerable.ToArray(Rec.AllItems), Pt, S, s_id, Src.ItemCount);
                Pachyderm_Acoustic.Direct_Sound DS = new Pachyderm_Acoustic.Direct_Sound(Pt, receiverBank, S, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, screen);
                DS.Delay_ms = ComponentSupport.GetDelay(Pt);
                DS.Begin();
                do { System.Threading.Thread.Sleep(100); } while (DS.ThreadState() == System.Threading.ThreadState.Running);
                DS.Combine_ThreadLocal_Results();
                s_id++;
                DSS.Add(DS);
            }
            ComponentSupport.SetTree(access, 0, Garden.TreeFromList(DSS));
            
        }
        protected override IIcon IconInternal
        {
            get
            {
                var assembly = typeof(SPLETC).Assembly;
                var resourceName = "PachydermGH2.Resources.Direct Sound.png";

                using (var stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream == null) return null;

                    // FromStream reads serialized .ghicon data, not PNG/BMP images.
                    // The PixelIcon retains the bitmap for its cached lifetime.
                    return new Grasshopper2.UI.Icon.PixelIcon(new Eto.Drawing.Bitmap(stream));
                }
            }
        }
    }
}
