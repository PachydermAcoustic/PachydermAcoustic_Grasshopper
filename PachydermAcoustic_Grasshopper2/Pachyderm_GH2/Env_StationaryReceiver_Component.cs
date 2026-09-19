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
    [IoId("7F75FF09-FFE7-4D4B-B9D1-F7D92B3B396C")]
    public class StationaryReceiver_Component : Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public StationaryReceiver_Component()
            : base(new Nomen("Stationary Receiver",
                "Non-growing 1 meter wide spherical receiver object",
                "Acoustics", "Model"))
        {
            Threading = Grasshopper2.Components.ThreadingState.SingleThreaded;
        }

        public StationaryReceiver_Component(IReader reader) : base(reader) { Threading = Grasshopper2.Components.ThreadingState.SingleThreaded; }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void AddInputs(InputAdder inputs)
        {
            inputs.AddPoint("Origin", "Or", "Acoustic Center of the Speaker", Access.Tree);
            inputs.AddGeneric("Sources", "Srcs", "Source Objects to be used", Access.Tree);
            inputs.AddGeneric("Room Model", "Room", "The Pachyderm Room Model Reference", Access.Item);
            inputs.AddInteger("Sample Frequency", "Fs", "Rate of samples per second to collect data in...", new Grasshopper2.UI.UiInteger(44100), Access.Item);
            inputs.AddNumber("Cut off time", "CO_Time", "The number of miliseconds that will be calculated for.", new Grasshopper2.UI.UiNumber(0, 1000), Access.Item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void AddOutputs(OutputAdder outputs)
        {
            outputs.AddGeneric("Receiver", "Rec", "Stationary Receiver object.", Access.Tree);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void Process(IDataAccess access)
        {
            Tree<Point3d> Origin;
            List<Hare.Geometry.Point> H_Origin = new List<Hare.Geometry.Point>();
            access.GetTree<Point3d>(0, out Origin);
            foreach (Point3d p in Origin.AllItems) H_Origin.Add(new Hare.Geometry.Point(p.X, p.Y, p.Z));
            Tree<Pachyderm_Acoustic.Environment.Source> Srcs;
            access.GetTree<Pachyderm_Acoustic.Environment.Source>(1, out Srcs);
            Pachyderm_Acoustic.Environment.Polygon_Scene S = null;
            access.GetItem<Pachyderm_Acoustic.Environment.Polygon_Scene>(2, out S);
            int Fs = 0;
            access.GetItem<int>(3, out Fs);
            double COTime = 0;
            access.GetItem<double>(4, out COTime);

            var banks = new List<Pachyderm_Acoustic.Environment.Receiver_Bank>();
            for(int i = 0; i < Srcs.ItemCount; i++)
            {
                Pachyderm_Acoustic.Environment.Receiver_Bank RB = new Pachyderm_Acoustic.Environment.Receiver_Bank(H_Origin, Srcs.Items[i], S, Fs, COTime, Pachyderm_Acoustic.Environment.Receiver_Bank.Type.Stationary, false); 
                RB.delay_ms = ComponentSupport.GetDelay(Srcs.Items[i]);
                banks.Add(RB);
            }
            ComponentSupport.SetTree(access, 0, Garden.TreeFromList(banks));
        }
        protected override IIcon IconInternal
        {
            get
            {
                var assembly = typeof(SPLETC).Assembly;
                var resourceName = "PachydermGH2.Resources.Microphone.png";

                using (var stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream == null) return null;

                    // FromStream reads serialized .ghicon data, not PNG/BMP images.
                    // The PixelIcon retains the bitmap for its cached lifetime.
                    return new Grasshopper2.UI.Icon.PixelIcon(new Eto.Drawing.Bitmap(stream));
                }
            }
        }
        //protected override IIcon IconInternal => new Grasshopper2.UI.Icon.PixelIcon("Microphone");
    }
}
