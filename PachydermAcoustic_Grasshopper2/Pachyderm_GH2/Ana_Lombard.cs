//'Pachyderm-Acoustic: Geometrical Acoustics for Rhinoceros (GPL)   
//' C:\Users\Arthu\Desktop\DEV\PachydermAcoustic_Grasshopper\Ana_Strength.cs
//'This file is part of Pachyderm-Acoustic. 
//' 
//'Copyright (c) 2008-2025, Arthur van der aHrten 
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
using System.Linq;

namespace PachydermGH
{
    [IoId("769386FB-9B85-45F7-90AC-03C194A39CAB")]
    public class Lombard : Component
    {
        MathNet.Numerics.Interpolation.CubicSpline[][] Speech = new MathNet.Numerics.Interpolation.CubicSpline[3][];

        /// <summary>
        /// Initializes a new instance of the MyComponent2 class.
        /// </summary>
        public Lombard()
            : base(new Nomen("Lombard",
                "Iteratively raises level in order to account for the Lombard Effect - the effect of human voice adjusting to background noise.",
                "Acoustics", "Analysis"))
        {
            double[] femaleDBA = new double[5];
            double[] maleDBA = new double[5];
            double[] childrenDBA = new double[5];

            Speech[0] = new MathNet.Numerics.Interpolation.CubicSpline[8];
            Speech[1] = new MathNet.Numerics.Interpolation.CubicSpline[8];
            Speech[2] = new MathNet.Numerics.Interpolation.CubicSpline[8];

            for (int i = 0; i < 5; i++)
            {
                femaleDBA[i] = Pachyderm_Acoustic.Utilities.AcousticalMath.Sound_Pressure_Level_A(Pachyderm_Acoustic.Utilities.AcousticalMath.Females[i]);
                maleDBA[i] = Pachyderm_Acoustic.Utilities.AcousticalMath.Sound_Pressure_Level_A(Pachyderm_Acoustic.Utilities.AcousticalMath.Males[i]);
                childrenDBA[i] = Pachyderm_Acoustic.Utilities.AcousticalMath.Sound_Pressure_Level_A(Pachyderm_Acoustic.Utilities.AcousticalMath.Children[i]);
            }

            for (int oct = 0; oct < 8; oct++)
            {
                double[] levelsfemale = new double[5];
                double[] levelsmale = new double[5];
                double[] levelschild = new double[5];

                for (int i = 0; i < 5; i++)
                {
                    levelsfemale[i] = Pachyderm_Acoustic.Utilities.AcousticalMath.Females[i][oct];
                    levelsmale[i] = Pachyderm_Acoustic.Utilities.AcousticalMath.Males[i][oct];
                    levelschild[i] = Pachyderm_Acoustic.Utilities.AcousticalMath.Children[i][oct];
                }

                Speech[0][oct] = MathNet.Numerics.Interpolation.CubicSpline.InterpolateAkima(femaleDBA, levelsfemale);
                Speech[1][oct] = MathNet.Numerics.Interpolation.CubicSpline.InterpolateAkima(maleDBA, levelsfemale);
                Speech[2][oct] = MathNet.Numerics.Interpolation.CubicSpline.InterpolateAkima(childrenDBA, levelschild);
            }

        }
        public Lombard(IReader reader) : base(reader) { }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void AddInputs(InputAdder inputs)
        {
            inputs.AddGeneric("Room", "Rm", "The Scene Object", Access.Item);
            inputs.AddNumber("Speaking People", "#P", "The number of speaking people in the room.", Access.Item);
            inputs.AddNumber("Background Noise", "N", "The ambient noise level of the room at rest.", Access.Tree);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void AddOutputs(OutputAdder outputs)
        {
            outputs.AddNumber("Average Level", "SPL", "Level", Access.Tree);
            outputs.AddNumber("Typical Source Level", "SWL", "Sound Power Level of each human speaker.", Access.Tree);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void Process(IDataAccess access)
        {
            Pachyderm_Acoustic.Environment.Scene Room = null;
            access.GetItem<Pachyderm_Acoustic.Environment.Scene>(0, out Room);
            double No_of_People = 0;
            access.GetItem<double>(1, out No_of_People);
            Tree<double> Noise;
            access.GetTree<double>(2, out Noise);
            if (Noise.ItemCount != 8) throw new Exception("Noise should be specified by octave band, 0 for 63 Hz. through 7 for 8000 Hz.");

            double[] A = new double[8];
            Pachyderm_Acoustic.Utilities.AcousticalMath.Absorption_Total(Room, out A);
            double[] Knoise = new double[8] { 26, 51, 79, 86, 90, 86, 78, 69 };

            double Lna0 = Pachyderm_Acoustic.Utilities.AcousticalMath.Sound_Pressure_Level_A(Noise.AllItems.ToArray());

            int People_Apparent = (int)Math.Round(Math.Pow(10, (Lna0 - 93 + 20 * Math.Log10(A[4])) / 20));

            double Lna = 93 - 20 * Math.Log10(A[4] / (No_of_People + People_Apparent));
            double SPL1m = 55 + 0.5 * (Lna - 45);

            double diff = Lna - SPL1m;

            double[] SWL = new double[8];
            double[] Lna_spectrum = new double[8];
            for (int oct = 0; oct < 8; oct++)
            {
                SWL[oct] = Speech[1][oct].Interpolate(SPL1m) + 11;
                Lna_spectrum[oct] = SWL[oct] + diff - 11;
            }
        
            access.SetItem(0, Lna_spectrum);
            access.SetItem(1, SWL);
        }

        protected override IIcon IconInternal
        {
            get
            {
                var assembly = typeof(SPLETC).Assembly;
                var resourceName = "PachydermGH2.Resources.Speech Transmission Index 1.png";

                using (var stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream == null)
                    {
                        //Debugging helper: Write all valid resource names to the Rhino command line to find the correct one.
                         foreach (var name in assembly.GetManifestResourceNames()) Rhino.RhinoApp.WriteLine("Resource: " + name);

                        return null;
                    }
                    var ms = new System.IO.MemoryStream();
                    stream.CopyTo(ms);
                    ms.Position = 0;
                    return Grasshopper2.UI.Icon.PixelIcon.FromStream(ms);
                }
            }
        }
        //protected override IIcon IconInternal => new Grasshopper2.UI.Icon.PixelIcon("Speech_Transmission_Index_1");
    }
}