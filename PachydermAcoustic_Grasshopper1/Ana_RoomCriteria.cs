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
using System.Windows.Forms;
using Grasshopper.Kernel;
using Pachyderm_Acoustic.Utilities;

namespace PachydermGH
{
    public abstract class EvaluateRoomCriteriaBase : GH_Component
    {
        private bool includesLowFrequency;
        protected abstract bool MarkII { get; }
        protected EvaluateRoomCriteriaBase(string name, string nickname) : base(name, nickname,
            "Evaluate RC from an unweighted sound pressure spectrum. Use the context menu to declare whether 16 and 31.5 Hz bands are present; they are absent by default.", "Acoustics", "Analysis") { }
        protected override void RegisterInputParams(GH_InputParamManager inputs)
        {
            inputs.AddNumberParameter("Sound pressure spectrum", "SPL", "Ascending unweighted SPL, dB re 20 micropascals. Default: 8 octaves (63 Hz to 8 kHz) or 24 thirds (50 Hz to 10 kHz). Context-menu low-frequency option: 10 octaves (16 Hz to 8 kHz) or 30 thirds (12.5 Hz to 10 kHz). One spectrum per branch.", GH_ParamAccess.list);
        }
        protected override void RegisterOutputParams(GH_OutputParamManager outputs)
        {
            outputs.AddNumberParameter("RC", "RC", "Arithmetic mean at 500, 1000 and 2000 Hz, rounded to the nearest integer. Reference-curve equations are extrapolated below RC-25 or above RC-50.", GH_ParamAccess.item);
            outputs.AddTextParameter("Quality", "Q", "RC: N/R/H/RV; RC Mark II: N/LF/MF/HF with vibration qualifiers. Empty when the two low-frequency bands are absent.", GH_ParamAccess.item);
            outputs.AddTextParameter("Vibration", "V", "Low-frequency vibration/rattle risk. Empty when low-frequency bands are absent.", GH_ParamAccess.item);
            outputs.AddBooleanParameter("Complete", "C", "True when the full low-frequency spectrum was supplied using the context-menu option. The RC number remains available when false.", GH_ParamAccess.item);
            outputs.AddTextParameter("Status", "S", "Rating designation and missing-band information.", GH_ParamAccess.item);
            if (MarkII)
            {
                outputs.AddNumberParameter("Quality assessment index", "QAI", "Range of energy-averaged LF/MF/HF deviations, in dB. Empty without the low-frequency bands.", GH_ParamAccess.item);
                outputs.AddNumberParameter("Spectral deviations", "SD", "Energy-averaged deviations in LF (16–63 Hz), MF (125–500 Hz), HF (1–4 kHz) order. Empty without the low-frequency bands.", GH_ParamAccess.list);
            }
        }
        protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu)
        {
            base.AppendAdditionalComponentMenuItems(menu);
            Menu_AppendItem(menu, "Includes 16 and 31.5 Hz bands", (sender, args) =>
            {
                RecordUndoEvent("Change criteria spectrum bands");
                includesLowFrequency = !includesLowFrequency;
                ExpireSolution(true);
            }, true, includesLowFrequency);
        }
        public override bool Write(GH_IO.Serialization.GH_IWriter writer)
        {
            writer.SetBoolean("IncludesLowFrequency", includesLowFrequency);
            return base.Write(writer);
        }
        public override bool Read(GH_IO.Serialization.GH_IReader reader)
        {
            includesLowFrequency = false;
            reader.TryGetBoolean("IncludesLowFrequency", ref includesLowFrequency);
            return base.Read(reader);
        }
        protected override void SolveInstance(IGH_DataAccess access)
        {
            Message = includesLowFrequency ? "16 Hz–8 kHz" : "63 Hz–8 kHz";
            var spectrum = new List<double>();
            if (!access.GetDataList(0, spectrum)) return;
            try
            {
                string quality, vibration;
                double qai;
                double[] deviations;
                double rating = AcousticalMath.Room_Criteria(spectrum.ToArray(), MarkII, includesLowFrequency,
                    out quality, out qai, out deviations, out vibration);
                string label = (MarkII ? "RC II " : "RC ") + rating.ToString("0", System.Globalization.CultureInfo.InvariantCulture);
                if (rating < 25 || rating > 50)
                {
                    label += " (extrapolated)";
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Reference-curve equations are extrapolated outside RC-25 to RC-50; the numerical rating remains the rounded speech-band average.");
                }
                access.SetData(0, rating);
                access.SetData(3, includesLowFrequency);
                if (includesLowFrequency)
                {
                    access.SetData(1, quality);
                    access.SetData(2, vibration);
                    access.SetData(4, label + " (" + quality + ")");
                    if (MarkII) { access.SetData(5, qai); access.SetDataList(6, deviations); }
                }
                else
                {
                    string status = label + "; incomplete: 16 and 31.5 Hz bands missing. Quality and vibration are unassessed.";
                    access.SetData(4, status);
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, status);
                }
            }
            catch (ArgumentException error) { AddRuntimeMessage(GH_RuntimeMessageLevel.Error, error.Message); }
        }
        protected override System.Drawing.Bitmap Icon => Properties.Resources.Room_Criteria;
    }

    public sealed class EvaluateRoomCriteria : EvaluateRoomCriteriaBase
    {
        public EvaluateRoomCriteria() : base("Evaluate Room Criteria", "EvalRC") { }
        protected override bool MarkII => false;
        public override Guid ComponentGuid => new Guid("6d898a65-6256-4357-9578-02447d0bd3d3");
    }

    public sealed class EvaluateRoomCriteriaMarkII : EvaluateRoomCriteriaBase
    {
        public EvaluateRoomCriteriaMarkII() : base("Evaluate Room Criteria Mark II", "EvalRCII") { }
        protected override bool MarkII => true;
        public override Guid ComponentGuid => new Guid("d9d38c02-1f2c-4b03-bd74-88ee5ac8976c");
    }
}
