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

using Grasshopper2.Components;
using Grasshopper2.Data;
using Grasshopper2.Parameters;
using Grasshopper2.UI;
using Grasshopper2.UI.Icon;
using GrasshopperIO;
using Pachyderm_Acoustic;
using Pachyderm_Acoustic.Environment;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.IO;

namespace PachydermGH
{
    [IoId("2E62F853-A1FB-4A56-9A28-DEF485E9B722")]
    public class RT_Eyring : Component
    {
        /// <summary>
        /// Initializes a new instance of the RT_Eyring class.
        /// </summary>
        public RT_Eyring()
            : base(new Nomen("Eyring Reverberation",
                "Computes reverberation time from the model using the Eyring method",
                "Acoustics", "Analysis"))
        {
        }
        public RT_Eyring(IReader reader) : base(reader) { }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void AddInputs(InputAdder inputs)
        {
            inputs.AddGeneric("Room", "R", "Pachyderm Scene", Access.Item);
            inputs.AddNumber("Volume", "V", "Room volume in cubic meters.", Access.Item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void AddOutputs(OutputAdder outputs)
        {
            outputs.AddNumber("Reverberation Time", "RT", "Reverberation Time", Access.Tree);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void Process(IDataAccess access)
        {
            Pachyderm_Acoustic.Environment.Scene Room = null;
            access.GetItem<Pachyderm_Acoustic.Environment.Scene>(0, out Room);
            double Volume = 0;
            access.GetItem<double>(1, out Volume);

            double[] RT = new double[8];
            Pachyderm_Acoustic.Utilities.AcousticalMath.Eyring(Room, Volume, ref RT);

            access.SetTree(0, Garden.TreeFromList(RT));
        }

        protected override IIcon IconInternal
        {
            get
            {
                var assembly = typeof(SPLETC).Assembly;
                var resourceName = "Pachyderm_GH.Icons.RT.png";

                using (var stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream == null) return null;

                    var ms = new MemoryStream();
                    stream.CopyTo(ms);
                    ms.Position = 0;
                    return Grasshopper2.UI.Icon.PixelIcon.FromStream(ms);
                }
            }
        }
        //protected override IIcon IconInternal => new Grasshopper2.UI.Icon.PixelIcon("RT");
    }
}