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
using Pachyderm_Acoustic.Utilities;
using System.Collections.Generic;
using System.Linq;
using Grasshopper2.Data;

namespace PachydermGH
{
    [IoId("57DF573D-65A7-4423-880E-AE3C0C22C174")]
    public class Signal_Correlation : Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent2 class.
        /// </summary>
        public Signal_Correlation()
            : base(new Nomen("Signal Correlation",
                "Calculates the correlation between two audio signals.",
                "Acoustics", "Utility"))
        {
        }

        public Signal_Correlation(IReader reader) : base(reader) { }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void AddInputs(InputAdder inputs)
        {
            inputs.AddGeneric("Impulse Response 1", "IR1", "Plug the audio signal impulse response in here.", Access.Item);
            inputs.AddGeneric("Impulse Response 2", "IR2", "Plug the audio signal impulse response in here.", Access.Item);
            inputs.AddNumber("Delay", "D", "Enter an offset in samples to help match the two integrals. A positive number will take samples off the front of IR1. A negative numuber will take sampels off the front of IR2.", Access.Item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void AddOutputs(OutputAdder outputs)
        {
            outputs.AddNumber("Correlation R", "R_c", "Correlation R value for the two IRs Schroeder Integrals.", Access.Item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="access">The access object is used to retrieve from inputs and store in outputs.</param>
        protected override void Process(IDataAccess access)
        {
            Audio_Signal IR1 = new Audio_Signal();
            access.GetItem<Audio_Signal>(0, out IR1);
            Audio_Signal IR2 = new Audio_Signal();
            access.GetItem<Audio_Signal>(1, out IR2);
            double delay = 0;
            access.GetItem<double>(2, out delay);

            double[][] signal1 = new double[8][];
            for (int i = 0; i < IR1.ChannelCount; i++) signal1[i] = new double[IR1.Count];
            double[][] signal2 = new double[8][];
            for (int i = 0; i < IR2.ChannelCount; i++) signal2[i] = new double[IR2.Count];

            List<double> R = new List<double>();
            
            for (int j = 0; j < IR1.ChannelCount; j++)
            {
                double[] L10_1 = new double[IR1.Value[j].Length];
                double[] L10_2 = new double[IR2.Value[j].Length];

                for(int i = 0; i < IR1.Value[j].Length; i++)
                {
                    L10_1[i] = AcousticalMath.Pressure_SPL(Math.Abs(IR1.Value[j][i]));
                }

                for (int i = 0; i < IR2.Value[j].Length; i++)
                {
                    L10_2[i] = AcousticalMath.Pressure_SPL(Math.Abs(IR2.Value[j][i]));
                }

                if (delay > 0) { L10_1.Reverse(); Array.Resize(ref L10_1, L10_1.Length - (int)delay); L10_1.Reverse(); }
                if (delay < 0) { L10_2.Reverse(); Array.Resize(ref L10_2, L10_2.Length + (int)delay); L10_2.Reverse(); }

                if (L10_1.Length < L10_2.Length) Array.Resize(ref L10_1, L10_2.Length);
                if (L10_2.Length < L10_1.Length) Array.Resize(ref L10_2, L10_1.Length);

                R.Add(MathNet.Numerics.Statistics.Correlation.Spearman(L10_1, L10_2));
                //R.Add(MathNet.Numerics.Statistics.Correlation.Spearman(IR1.Value[j], IR2.Value[j]));
            }

            access.SetTree(0, Garden.TreeFromList(R));
        }
        protected override IIcon IconInternal
        {
            get
            {
                var assembly = typeof(SPLETC).Assembly;
                var resourceName = "Pachyderm_GH.Icons.Energy_Time_Curve.png";

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

        //protected override IIcon IconInternal => new Grasshopper2.UI.Icon.PixelIcon("Energy_Time_Curve");
    }
}