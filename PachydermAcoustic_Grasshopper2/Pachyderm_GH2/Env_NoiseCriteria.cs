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
using Grasshopper2.Data;

namespace PachydermGH
{
    [IoId("5C321318-1C0D-4CC4-A1E6-B312FB3A6962")]
    public class NoiseCriteria : Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public NoiseCriteria()
            : base(new Nomen("Noise Criteria",
                "Background noise curve specified in Noise Criteria",
                "Acoustics", "Model"))
        {
        }

        public NoiseCriteria(IReader reader) : base(reader) { }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void AddInputs(InputAdder inputs)
        {
            inputs.AddNumber("Noise Criteria", "NC", "The number of the specified Noise Criteria spectrum", new Grasshopper2.UI.UiNumber(0,40), Access.Item);

            //Grasshopper.Kernel.Parameters.Param_Number param0 = (inputs[0] as Grasshopper.Kernel.Parameters.Param_Number);
            //if (param0 != null) param0.SetPersistentData(40);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void AddOutputs(OutputAdder outputs)
        {
            outputs.AddNumber("Noise Spectrum", "NS", "The resulting spectrum from the NC curve specification...", Access.Tree);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void Process(IDataAccess access)
        {
            double NS=40;
            if (!access.GetItem<double>(0, out NS)) return;

            double[] NC_Curve = Pachyderm_Acoustic.Utilities.AcousticalMath.Noise_Criteria(NS);

            access.SetItem(0, NC_Curve);
        }
    }
}