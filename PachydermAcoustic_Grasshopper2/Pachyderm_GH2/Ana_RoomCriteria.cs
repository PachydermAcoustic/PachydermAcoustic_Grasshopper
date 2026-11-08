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
using Grasshopper2.UI;
using GrasshopperIO;
using Pachyderm_Acoustic.Utilities;

namespace PachydermGH
{
    public abstract class EvaluateRoomCriteriaBase : Component, IContextMenuAware
    {
        public bool IncludesLowFrequency { get; set; }
        protected abstract bool MarkII { get; }
        protected EvaluateRoomCriteriaBase(string name) : base(new Nomen(name,
            "Evaluate RC from unweighted SPL. Context menu declares whether 16 and 31.5 Hz bands are included; absent by default.", "Acoustics", "Analysis"))
        {
            Threading = ThreadingState.UiSingleThreaded;
        }
        protected EvaluateRoomCriteriaBase(IReader reader) : base(reader)
        {
            Threading = ThreadingState.UiSingleThreaded;
            if (reader.HasItem("IncludesLowFrequency")) IncludesLowFrequency = reader.Boolean("IncludesLowFrequency");
        }
        protected override void AddInputs(InputAdder inputs)
        {
            inputs.AddNumber("Sound pressure spectrum", "SPL", "Ascending unweighted SPL, dB re 20 micropascals. Default: 8 octaves (63 Hz to 8 kHz) or 24 thirds (50 Hz to 10 kHz). Context-menu LF option: 10 octaves (16 Hz to 8 kHz) or 30 thirds (12.5 Hz to 10 kHz). One spectrum per twig.", Access.Twig);
        }
        protected override void AddOutputs(OutputAdder outputs)
        {
            outputs.AddNumber("RC", "RC", "Arithmetic mean at 500, 1000 and 2000 Hz, rounded to the nearest integer. Reference-curve equations are extrapolated below RC-25 or above RC-50.", Access.Item);
            outputs.AddText("Quality", "Q", "RC: N/R/H/RV; RC Mark II: N/LF/MF/HF with vibration qualifiers. Empty without low-frequency bands.", Access.Item);
            outputs.AddText("Vibration", "V", "Low-frequency vibration/rattle risk. Empty without low-frequency bands.", Access.Item);
            outputs.AddBoolean("Complete", "C", "True when full low-frequency data were supplied using the context-menu option. The RC number remains available when false.", Access.Item);
            outputs.AddText("Status", "S", "Rating designation and missing-band information.", Access.Item);
            if (MarkII)
            {
                outputs.AddNumber("Quality assessment index", "QAI", "Range of energy-averaged LF/MF/HF deviations, dB. Empty without low-frequency bands.", Access.Item);
                outputs.AddNumber("Spectral deviations", "SD", "Energy-averaged LF (16–63 Hz), MF (125–500 Hz), HF (1–4 kHz) deviations, in that order. Empty without low-frequency bands.", Access.Twig);
            }
        }
        public void AppendToMenu(Eto.Forms.ContextMenu menu)
        {
            var item = new Eto.Forms.CheckMenuItem { Text = "Includes 16 and 31.5 Hz bands", Checked = IncludesLowFrequency };
            item.Click += delegate
            {
                AddUndoRecord(new Grasshopper2.Undo.VerbNoun("Change", "criteria spectrum bands"),
                    new Grasshopper2.Undo.Actions.PropertyAction(this, nameof(IncludesLowFrequency), Grasshopper2.Undo.Effects.ExpireSolution));
                IncludesLowFrequency = !IncludesLowFrequency;
                Expire();
                Document?.Solution.Start();
            };
            menu.Items.Add(item);
        }
        public override void Store(IWriter writer)
        {
            base.Store(writer);
            writer.Boolean("IncludesLowFrequency", IncludesLowFrequency);
        }
        protected override void Process(IDataAccess access)
        {
            if (!access.GetItemArray<double>(0, out var spectrum)) return;
            if (access.GetNullArray(0, out var nulls) && Array.Exists(nulls, value => value))
                throw new ArgumentException("The sound pressure spectrum contains missing values.");
            string quality, vibration;
            double qai;
            double[] deviations;
            double rating = AcousticalMath.Room_Criteria(spectrum, MarkII, IncludesLowFrequency,
                out quality, out qai, out deviations, out vibration);
            string label = (MarkII ? "RC II " : "RC ") + rating.ToString("0", System.Globalization.CultureInfo.InvariantCulture);
            if (rating < 25 || rating > 50)
            {
                label += " (extrapolated)";
                access.AddMessage(Grasshopper2.Doc.Message.Warning("Extrapolated RC", "Reference-curve equations are extrapolated outside RC-25 to RC-50; the numerical rating remains the rounded speech-band average."));
            }
            access.SetItem(0, rating);
            access.SetItem(3, IncludesLowFrequency);
            if (IncludesLowFrequency)
            {
                access.SetItem(1, quality);
                access.SetItem(2, vibration);
                access.SetItem(4, label + " (" + quality + ")");
                if (MarkII) { access.SetItem(5, qai); access.SetTwig(6, deviations); }
            }
            else
            {
                string status = label + "; incomplete: 16 and 31.5 Hz bands missing. Quality and vibration are unassessed.";
                access.SetItem(4, status);
                access.AddMessage(Grasshopper2.Doc.Message.Warning("Incomplete spectrum", status));
            }
        }
        protected override Grasshopper2.UI.Icon.IIcon IconInternal
        {
            get
            {
                using (var stream = typeof(EvaluateRoomCriteriaBase).Assembly.GetManifestResourceStream("PachydermGH2.Resources.Room Criteria.png"))
                    return stream == null ? null : new Grasshopper2.UI.Icon.PixelIcon(new Eto.Drawing.Bitmap(stream));
            }
        }
    }

    [IoId("6d898a65-6256-4357-9578-02447d0bd3d3")]
    public sealed class EvaluateRoomCriteria : EvaluateRoomCriteriaBase
    {
        public EvaluateRoomCriteria() : base("Evaluate Room Criteria") { }
        public EvaluateRoomCriteria(IReader reader) : base(reader) { }
        protected override bool MarkII => false;
    }

    [IoId("d9d38c02-1f2c-4b03-bd74-88ee5ac8976c")]
    public sealed class EvaluateRoomCriteriaMarkII : EvaluateRoomCriteriaBase
    {
        public EvaluateRoomCriteriaMarkII() : base("Evaluate Room Criteria Mark II") { }
        public EvaluateRoomCriteriaMarkII(IReader reader) : base(reader) { }
        protected override bool MarkII => true;
    }
}
