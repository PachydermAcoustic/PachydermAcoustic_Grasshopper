//'Pachyderm-Acoustic: Geometrical Acoustics for Rhinoceros (GPL)   
//' 
//'This file is part of Pachyderm-Acoustic. 
//' 
//'Copyright (c) 2008-2026, Open Research in Acoustical Science and Education, Inc. - a 501(c)3 nonprofit 
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

using Grasshopper2.Data;
using Grasshopper2.Components;
using Grasshopper2.Parameters;
using Grasshopper2.UI;
using GrasshopperIO;

namespace PachydermGH
{
    [IoId("D609D0BF-4237-4FA5-A5EE-46FB376D0B65")]
    public class RoomCriteria : Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public RoomCriteria()
            : base(new Nomen("Room Criteria",
                "Background noise curve specified in Room Criteria",
                "Acoustics", "Model"))
        {
            Threading = Grasshopper2.Components.ThreadingState.SingleThreaded;
        }

        public RoomCriteria(IReader reader) : base(reader) { Threading = Grasshopper2.Components.ThreadingState.SingleThreaded; }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void AddInputs(InputAdder inputs)
        {
            inputs.AddNumber("Room Criteria", "RC", "The number of the specified Room Criteria spectrum", new Grasshopper2.UI.UiNumber(0, 40), Access.Item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void AddOutputs(OutputAdder outputs)
        {
            outputs.AddNumber("Noise Spectrum", "NS", "The resulting spectrum from the RC curve specification...", Access.Tree);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void Process(IDataAccess access)
        {
            double NS=40;
            if (!access.GetItem<double>(0, out NS)) return;

            double[] RC_Curve = Pachyderm_Acoustic.Utilities.AcousticalMath.Room_Criteria(NS);

            ComponentSupport.SetTree(access, 0, Garden.TreeFromList(RC_Curve));
        }

        protected override Grasshopper2.UI.Icon.IIcon IconInternal
        {
            get
            {
                var assembly = typeof(RoomCriteria).Assembly;
                var resourceName = "PachydermGH2.Resources.Room Criteria.png";

                using (var stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream == null) return null;

                    // The PixelIcon retains the bitmap for its cached lifetime.
                    return new Grasshopper2.UI.Icon.PixelIcon(new Eto.Drawing.Bitmap(stream));
                }
            }
        }
    }
}
