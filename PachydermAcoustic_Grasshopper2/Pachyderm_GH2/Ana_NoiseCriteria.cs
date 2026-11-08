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
using Grasshopper2.Components;
using Grasshopper2.Parameters;
using GrasshopperIO;
using Pachyderm_Acoustic.Utilities;

namespace PachydermGH
{
    [IoId("4d919aec-8d66-4ac5-9cf9-a7638dcbe1ab")]
    public sealed class EvaluateNoiseCriteria : Component
    {
        public EvaluateNoiseCriteria() : base(new Grasshopper2.UI.Nomen("Evaluate Noise Criteria",
            "Evaluate the historic NC tangency rating of an unweighted sound pressure spectrum.", "Acoustics", "Analysis")) { }
        public EvaluateNoiseCriteria(IReader reader) : base(reader) { }
        protected override void AddInputs(InputAdder inputs)
        {
            inputs.AddNumber("Sound pressure spectrum", "SPL", "Unweighted SPL in dB re 20 micropascals: 8 octaves (63 Hz to 8 kHz) or 24 thirds (50 Hz to 10 kHz), in ascending order. One spectrum per twig.", Access.Twig);
        }
        protected override void AddOutputs(OutputAdder outputs)
        {
            outputs.AddNumber("NC", "NC", "NC tangency rating, interpolated within NC-15 to NC-70 and linearly extrapolated below or above using the first or last table segment.", Access.Item);
            outputs.AddText("Designation", "D", "NC rating, labeled extrapolated when outside NC-15 to NC-70.", Access.Item);
            outputs.AddNumber("Limiting frequency", "F", "A controlling octave-band frequency in Hz.", Access.Item);
        }
        protected override void Process(IDataAccess access)
        {
            if (!access.GetItemArray<double>(0, out var spectrum)) return;
            if (access.GetNullArray(0, out var nulls) && Array.Exists(nulls, value => value))
                throw new ArgumentException("The sound pressure spectrum contains missing values.");
            double frequency;
            string designation;
            double rating = AcousticalMath.Noise_Criteria(spectrum, out designation, out frequency);
            access.SetItem(0, rating);
            if (rating < 15 || rating > 70)
                access.AddMessage(Grasshopper2.Doc.Message.Warning("Extrapolated NC", designation + "; linearly extrapolated outside NC-15 to NC-70 using the first or last segment of the existing table."));
            access.SetItem(1, designation);
            access.SetItem(2, frequency);
        }
        protected override Grasshopper2.UI.Icon.IIcon IconInternal
        {
            get
            {
                using (var stream = typeof(EvaluateNoiseCriteria).Assembly.GetManifestResourceStream("PachydermGH2.Resources.Noise Criteria.png"))
                    return stream == null ? null : new Grasshopper2.UI.Icon.PixelIcon(new Eto.Drawing.Bitmap(stream));
            }
        }
    }
}
