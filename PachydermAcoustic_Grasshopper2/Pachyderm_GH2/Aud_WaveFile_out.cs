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
        }

        public ExportWaveFile(IReader reader) : base(reader) { }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void AddInputs(InputAdder inputs)
        {
            inputs.AddGeneric("Signal Buffer", "Signal", "All input signals to be written to wave file. Signals will be written in channels according to their order at input.", Access.Item);
            inputs.AddText("Wave File Path", "Path", "The location of the wave file.", Access.Item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void AddOutputs(OutputAdder outputs)
        {
        }

        Eto.Forms.CheckBox bitrate16, bitrate32, Normalize;

        public override void AppendToInputPanel(InputPanel panel)
        {
            panel.AddCheck("16 bit audio", true, bitrate16_click);
            panel.AddCheck("32 bit audio", false, bitrate32_click);
            panel.AddCheck("Normalize", true, Normalize_click);
            base.AppendToInputPanel(panel);
        }

        public void bitrate16_click(bool set)
        {
            bitrate32.Checked = false;
            Document.Solution.ReleaseExpirationBlock();
            this.Expire();
            Document.Solution.Start();
        }

        public void bitrate32_click(bool set)
        {
            bitrate16.Checked = false;
            Document.Solution.ReleaseExpirationBlock();
            this.Expire();
            Document.Solution.Start();
        }

        public void Normalize_click(bool set)
        {
            Normalize.Checked = set;
            Document.Solution.ReleaseExpirationBlock();
            this.Expire();
            Document.Solution.Start();
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void Process(IDataAccess access)
        {   
            Audio_Signal S = new Audio_Signal();
            access.GetItem<Audio_Signal>(0, out S);
            if (S.Count < 1) throw new Exception("Signals should be type of Audio Signal.");

            string path = "";
            access.GetItem(1, out path);
            if (!path.EndsWith(".wav")) throw new Exception("Path invalid. Make sure that directory exists, and that the ");

            if (Normalize.Checked == true)
            {
                for (int a = 0; a < S.Count; a++)
                {
                    double m = S[a].Max();
                    for (int i = 0; i < S[a].Length; i++) S[a][i] /= m;
                }
            }

            float[][] data = new float[S.ChannelCount][];
            for (int i = 0; i < S.ChannelCount; i++) data[i] = S.toFloat(i);
            Pachyderm_Acoustic.Audio.Pach_SP.Wave.Write(data, S.SampleFrequency, path, bitrate16.Checked.Value ? 16 : 32);
        }
        protected override IIcon IconInternal
        {
            get
            {
                var assembly = typeof(SPLETC).Assembly;
                var resourceName = "Pachyderm_GH.Icons.Wave_File.png";

                using (var stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream == null) return null;

                    var ms = new System.IO.MemoryStream();
                    stream.CopyTo(ms);
                    ms.Position = 0;
                    return Grasshopper2.UI.Icon.PixelIcon.FromStream(ms);
                }
            }
        }
    }
}