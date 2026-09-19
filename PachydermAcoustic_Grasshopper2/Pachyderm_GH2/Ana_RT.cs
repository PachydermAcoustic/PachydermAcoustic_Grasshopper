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
using Grasshopper2.Data;
using System.Collections.Generic;
using Grasshopper2.UI.InputPanel;

namespace PachydermGH
{
    [IoId("575F7509-97DE-4021-8A46-B72BA96E6531")]
    public class RT_X_ETC : Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent2 class.
        /// </summary>
        public RT_X_ETC()
            : base(new Nomen("Reverberation Time",
                "Computes reverberation time from Energy Time Curve",
                "Acoustics", "Analysis"))
        {
            Threading = Grasshopper2.Components.ThreadingState.SingleThreaded;
        }
        public RT_X_ETC(IReader reader) : base(reader) { Threading = Grasshopper2.Components.ThreadingState.SingleThreaded; Noise_Compensation=reader.TryRead<bool>("Noise_Compensation",false);}

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        public override void Store(IWriter writer) { base.Store(writer); writer.Boolean("Noise_Compensation",Noise_Compensation); }
        protected override void AddInputs(InputAdder inputs)
        {
            inputs.AddGeneric("Energy Time Curve", "ETC", "Energy Time Curve", Access.Item);
            inputs.AddInteger("Decay Index", "t_X", "A linear regression will be performed on the schroeder integral from -5 dB of decay to -X-5 dB of decay", Access.Item).Set(30);

            //Grasshopper.Kernel.Parameters.Param_Integer param = (inputs[1] as Grasshopper.Kernel.Parameters.Param_Integer);
            //if (param != null) param.SetPersistentData(30);
        }

        bool Noise_Compensation = false;

        public override void AppendToInputPanel(InputPanel panel)
        {
            panel.AddCheck("Noise Compensation (Not needed for Simulated IRs)", Noise_Compensation, Comp_click);
            base.AppendToInputPanel(panel);
        }

        public void Comp_click(bool set)
        {
            Noise_Compensation = set;
            // Expire schedules a fresh solution after changing a setting.
            this.Expire();
            Document.Solution.Start();
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void AddOutputs(OutputAdder outputs)
        {
            outputs.AddNumber("Rerberation Time", "RT", "Reverberation Time", Access.Tree);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void Process(IDataAccess access)
        {
            Audio_Signal ETC = null;
            ETC = ComponentSupport.Signal(access, 0);
            int tx = 30;
            access.GetItem<int>(1, out tx);

            List<double> RT = new List<double>();
            foreach (double[] f in ETC.Value)
            {
                double[] s = new double[f.Length];
                for (int i = 0; i < f.Length; i++) s[i] += (double)f[i];
                double[] si = Pachyderm_Acoustic.Utilities.AcousticalMath.Schroeder_Integral(s);
                if (Noise_Compensation)
                {
                    double EDT = Pachyderm_Acoustic.Utilities.AcousticalMath.EarlyDecayTime(si, ETC.SampleFrequency);
                    si = Pachyderm_Acoustic.Utilities.AcousticalMath.Schroeder_Integral(s, EDT * 1000 * (tx + 10) / 60);
                }
                RT.Add(Pachyderm_Acoustic.Utilities.AcousticalMath.T_X(si, tx, ETC.SampleFrequency));
            }
            ComponentSupport.SetTree(access, 0, Garden.TreeFromList(RT));
        }
        protected override IIcon IconInternal
        {
            get
            {
                var assembly = typeof(SPLETC).Assembly;
                var resourceName = "PachydermGH2.Resources.RT.png";

                using (var stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream == null) return null;

                    // FromStream reads serialized .ghicon data, not PNG/BMP images.
                    // The PixelIcon retains the bitmap for its cached lifetime.
                    return new Grasshopper2.UI.Icon.PixelIcon(new Eto.Drawing.Bitmap(stream));
                }
            }
        }
        //protected override IIcon IconInternal => new Grasshopper2.UI.Icon.PixelIcon("RT");
    }
}
