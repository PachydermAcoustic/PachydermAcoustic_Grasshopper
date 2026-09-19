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
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;

namespace PachydermGH
{
    public class Lombard_RT : GH_Component
    {
        MathNet.Numerics.Interpolation.CubicSpline[][] Speech = new MathNet.Numerics.Interpolation.CubicSpline[3][];

        /// <summary>
        /// Initializes a new instance of the MyComponent2 class.
        /// </summary>
        public Lombard_RT()
            : base("Lombard Effect (RT,V)", "Lombard/RT",
                "Iteratively raises level in order to account for the Lombard Effect - the effect of human voice adjusting to background noise. This version gets total absorption out of ",
                "Acoustics", "Analysis")
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

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Reverb Time", "RT", "Reverberation Time", GH_ParamAccess.list);
            pManager.AddNumberParameter("Volume", "V", "Volume of the room", GH_ParamAccess.item);
            pManager.AddNumberParameter("Speaking People", "#P", "The number of speaking people in the room.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Background Noise", "N", "The ambient noise level of the room at rest.", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Average Level", "SPL", "Level", GH_ParamAccess.list);
            pManager.AddNumberParameter("Typical Source Level", "SWL", "Sound Power Level of each human speaker.", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<double> RT = new List<double>();
            DA.GetDataList<double>(0, RT);
            double Volume = 0;
            DA.GetData<double>(1, ref Volume);
            double No_of_People = 0;
            DA.GetData<double>(2, ref No_of_People);
            List<double> Noise = new List<double>();
            DA.GetDataList<double>(3, Noise);
            if (Noise.Count != 8) throw new Exception("Noise should be specified by octave band, 0 for 63 Hz. through 7 for 8000 Hz.");

            double ChosenRT = 0;

            for(int i = 0; i < RT.Count; i+=8)
            {
                ChosenRT = Math.Max(ChosenRT, Math.Max(RT[i + 3], Math.Max(RT[i + 4], RT[i + 5])));
            }

            double[] Knoise = new double[8] { 26, 51, 79, 86, 90, 86, 78, 69 };

            double Lna0 = Pachyderm_Acoustic.Utilities.AcousticalMath.Sound_Pressure_Level_A(Noise.ToArray());

            double A = 0.16 * Volume / ChosenRT;

            int People_Apparent = (int)Math.Round(Math.Pow(10, (Lna0 - 93 + 20 * Math.Log10(A)) / 20));

            double Lna = 93 - 20 * Math.Log10(A / (No_of_People + People_Apparent));
            double SPL1m = 55 + 0.5 * (Lna - 45);

            double diff = Lna - SPL1m;

            double[] SWL = new double[8];
            double[] Lna_spectrum = new double[8];
            for (int oct = 0; oct < 8; oct++)
            {
                SWL[oct] = Speech[1][oct].Interpolate(SPL1m) + 11;
                Lna_spectrum[oct] = SWL[oct] + diff - 11;
            }
        
            DA.SetDataList(0, Lna_spectrum);
            DA.SetDataList(1, SWL);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                System.Drawing.Bitmap b = Properties.Resources.Speech_Transmission_Index_1;
                b.MakeTransparent(System.Drawing.Color.White);
                return b;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{903EC2CF-FD15-4A55-8A08-E34A0322A3EE}"); }
        }
    }
}