using Grasshopper2.Data;
using System.Collections.Generic;
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
using System.Linq;
using Grasshopper2.UI.InputPanel;

namespace PachydermGH
{
    [IoId("F3098E20-508B-4ED3-BE41-5BDDDE87E71D")]
    public class ExportWaveFile : Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent2 class.
        /// </summary>
        public ExportWaveFile()
            : base(new Nomen("Write Wave File",
                "Writes a signal to a wave file.",
                "Acoustics", "Audio"))
        {
            Threading = Grasshopper2.Components.ThreadingState.SingleThreaded;
        }

        public ExportWaveFile(IReader reader) : base(reader) { Threading = Grasshopper2.Components.ThreadingState.SingleThreaded; use16Bit=reader.TryRead<bool>("use16Bit",true); normalize=reader.TryRead<bool>("normalize",true);}

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        public override void Store(IWriter writer) { base.Store(writer); writer.Boolean("use16Bit",use16Bit); writer.Boolean("normalize",normalize); }
        protected override void AddInputs(InputAdder inputs)
        {
            inputs.AddGeneric("Signal Buffer", "Signal", "All input signals to be written to wave file. Signals will be written in channels according to their order at input.", Access.Item);
            inputs.AddText("Wave File Path", "Path", "The location of the wave file.", Access.Item);
            inputs.AddBoolean("Write", "Write", "Enable write for this solution.", Access.Item).Set(false);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void AddOutputs(OutputAdder outputs)
        {
        }

        bool use16Bit = true, normalize = true;

        public override void AppendToInputPanel(InputPanel panel)
        {
            panel.AddCheck("16 bit audio", use16Bit, bitrate16_click);
            panel.AddCheck("32 bit audio", !use16Bit, bitrate32_click);
            panel.AddCheck("Normalize", normalize, Normalize_click);
            base.AppendToInputPanel(panel);
        }

        public void bitrate16_click(bool set)
        {
            use16Bit = set;
            // Expire schedules a fresh solution after changing a setting.
            this.Expire();
            Document.Solution.Start();
        }

        public void bitrate32_click(bool set)
        {
            use16Bit = !set;
            // Expire schedules a fresh solution after changing a setting.
            this.Expire();
            Document.Solution.Start();
        }

        public void Normalize_click(bool set)
        {
            normalize = set;
            // Expire schedules a fresh solution after changing a setting.
            this.Expire();
            Document.Solution.Start();
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void Process(IDataAccess access)
        {
            if(!access.GetItem<bool>(2,out var enabled) || !enabled) return;
            var signal = ComponentSupport.Signal(access, 0).Duplicate();
            if (!access.GetItem<string>(1, out var path) || !path.EndsWith(".wav", StringComparison.OrdinalIgnoreCase)) throw new ArgumentException("Provide a .wav output path.");
            if (normalize) {
                double peak = 0;
                foreach (var channel in signal.Value) foreach (double sample in channel) peak = Math.Max(peak, Math.Abs(sample));
                if (peak > 0) foreach (var channel in signal.Value) for (int i = 0; i < channel.Length; i++) channel[i] /= peak;
            }
            Pachyderm_Acoustic.Audio.Pach_SP.Wave.Write(signal.toFloat(), signal.SampleFrequency, path, use16Bit ? 16 : 32);
        }
        protected override IIcon IconInternal
        {
            get
            {
                var assembly = typeof(SPLETC).Assembly;
                var resourceName = "PachydermGH2.Resources.Wave File.png";

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
