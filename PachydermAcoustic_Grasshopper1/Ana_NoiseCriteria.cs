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

using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Pachyderm_Acoustic.Utilities;

namespace PachydermGH
{
    public sealed class EvaluateNoiseCriteria : GH_Component
    {
        public EvaluateNoiseCriteria() : base("Evaluate Noise Criteria", "EvalNC",
            "Evaluate the historic NC tangency rating of an unweighted sound pressure spectrum.", "Acoustics", "Analysis") { }
        protected override void RegisterInputParams(GH_InputParamManager inputs)
        {
            inputs.AddNumberParameter("Sound pressure spectrum", "SPL", "Unweighted SPL in dB re 20 micropascals: 8 octaves (63 Hz to 8 kHz) or 24 thirds (50 Hz to 10 kHz), in ascending frequency order. Supply one spectrum per branch.", GH_ParamAccess.list);
        }
        protected override void RegisterOutputParams(GH_OutputParamManager outputs)
        {
            outputs.AddNumberParameter("NC", "NC", "NC tangency rating, interpolated within NC-15 to NC-70 and linearly extrapolated below or above using the first or last table segment.", GH_ParamAccess.item);
            outputs.AddTextParameter("Designation", "D", "NC rating, labeled extrapolated when outside NC-15 to NC-70.", GH_ParamAccess.item);
            outputs.AddNumberParameter("Limiting frequency", "F", "A controlling octave-band frequency in Hz.", GH_ParamAccess.item);
        }
        protected override void SolveInstance(IGH_DataAccess access)
        {
            Message = null;
            var spectrum = new List<double>();
            if (!access.GetDataList(0, spectrum)) return;
            try
            {
                double frequency;
                string designation;
                double rating = AcousticalMath.Noise_Criteria(spectrum.ToArray(), out designation, out frequency);
                access.SetData(0, rating);
                if (rating < 15 || rating > 70)
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, designation + "; linearly extrapolated outside NC-15 to NC-70 using the first or last segment of the existing table.");
                access.SetData(1, designation);
                access.SetData(2, frequency);
                Message = designation;
            }
            catch (ArgumentException error) { AddRuntimeMessage(GH_RuntimeMessageLevel.Error, error.Message); }
        }
        protected override System.Drawing.Bitmap Icon => Properties.Resources.Noise_Criteria;
        public override Guid ComponentGuid => new Guid("4d919aec-8d66-4ac5-9cf9-a7638dcbe1ab");
    }
}
