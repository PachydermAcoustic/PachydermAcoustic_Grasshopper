using Grasshopper2.Data;
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
using System.Collections.Generic;

namespace PachydermGH
{
    [IoId("6AD48AB2-08FF-4921-9B24-4F7DB16085AD")]
    public class NoiseRating : Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public NoiseRating()
            : base(new Nomen("Noise Rating",
                "Background noise curve specified in Noise Criteria",
                "Acoustics", "Model"))
        {
            Threading = Grasshopper2.Components.ThreadingState.SingleThreaded;
        }

        public NoiseRating(IReader reader) : base(reader) { Threading = Grasshopper2.Components.ThreadingState.SingleThreaded; }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void AddInputs(InputAdder inputs)
        {
            inputs.AddNumber("Noise Rating", "NR", "The number of the specified Noise Rating spectrum", new Grasshopper2.UI.UiNumber(0, 30), Access.Item);

            //Grasshopper.Kernel.Parameters.Param_Number param0 = (inputs[0] as Grasshopper.Kernel.Parameters.Param_Number);
            //if (param0 != null) param0.SetPersistentData(40);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void AddOutputs(OutputAdder outputs)
        {
            outputs.AddNumber("Noise Spectrum", "NS", "The resulting spectrum from the NR curve specification...", Access.Tree);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void Process(IDataAccess access)
        {
            double NS=40;
            if (!access.GetItem<double>(0, out NS)) return;

            double[] NR_Curve = Pachyderm_Acoustic.Utilities.AcousticalMath.Noise_Rating(NS);

            ComponentSupport.SetTree(access, 0, Garden.TreeFromList(NR_Curve));
        }
    }
}
