//'Pachyderm-Acoustic: Geometrical Acoustics for Rhinoceros (GPL) by Arthur van der Harten 
//' 
//'This file is part of Pachyderm-Acoustic. 
//' 
//'Copyright (c) 2008-2019, Arthur van der Harten 
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
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace PachydermGH
{
    public class SPLAETC : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent2 class.
        /// </summary>
        public SPLAETC()
            : base("A-weighted Sound Pressure Level", "SPL-A",
                "Computes Sound Pressure Level (A) from Energy Time Curve",
                "Acoustics", "Analysis")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Energy Time Curve", "ETC", "Energy Time Curve", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Sound Pressure Level(A)", "SPLA", "A-weighted Sound Pressure Level", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Audio_Signal ETC = new Audio_Signal();
            DA.GetData<Audio_Signal>(0, ref ETC);

            List<double> SPL = new List<double>();

            double s = 0;
            for (int i = 0; i < ETC.Value.Length; i++)
            {
                for (int j = 0; j < ETC.Value[i].Length; j++) s += (double)ETC.Value[i][j];
                SPL.Add(Pachyderm_Acoustic.Utilities.AcousticalMath.SPL_Intensity(s));
            }
            DA.SetData(0, Pachyderm_Acoustic.Utilities.AcousticalMath.Sound_Pressure_Level_A(SPL.ToArray()));

            //Grasshopper.Kernel.Data.GH_Structure<Audio_Signal> ETC = new Grasshopper.Kernel.Data.GH_Structure<Audio_Signal>();
            //Grasshopper.Kernel.Data.GH_Structure<GH_Goo<double>> spec = new Grasshopper.Kernel.Data.GH_Structure<GH_Goo<double>>();

            //if (DA.GetDataTree<GH_Goo<double>>(0, out spec))
            //{
            //    //if (spec.Branches[0].Count != 8) throw new Exception("For A-Weighted Sound Pressure Level, full spectrum data is needed. Please supply data for octaves 0 through 7.");
            //    List<double> SW = new List<double>();
            //    for (int i = 0; i < spec.Branches.Count; i++)
            //    {
            //        if (spec[i].Count < 8) SW.Add(double.NaN);
            //        double[] spl = new double[8];
            //        for (int oct = 0; oct < 8; oct++) spl[oct] = spec.Branches[i][oct].Value;
            //        SW.Add(Pachyderm_Acoustic.Utilities.AcousticalMath.Sound_Pressure_Level_A(spl));
            //    }
            //    DA.SetDataList(0, SW);
            //}
            //else if (DA.GetDataTree<Audio_Signal>(0, out ETC))
            //{
            //    for (int i = 0; i < ETC.Branches.Count; i++)
            //    {
            //        if (ETC.get_DataItem(i).ChannelCount != 8) throw new Exception("For A-Weighted Sound Pressure Level, full spectrum data is needed. Please supply data for octaves 0 through 7.");
            //        double SW = 0;
            //        double[] AFactors = new double[8] { Math.Pow(10, (-26.2 / 10)), Math.Pow(10, (-16.1 / 10)), Math.Pow(10, (-8.6 / 10)), Math.Pow(10, (-3.2 / 10)), 1, Math.Pow(10, (1.2 / 10)), Math.Pow(10, (1 / 10)), Math.Pow(10, (-1.1 / 10)) };

            //        for (int f = 0; f < ETC.get_DataItem(i).ChannelCount; f++)
            //        {
            //            double s = 0;
            //            for (int j = 0; j < ETC.Branches.Count; j++) s += ETC.get_DataItem(i)[f][j];
            //            SW += s * AFactors[f];
            //            DA.SetData(0, Pachyderm_Acoustic.Utilities.AcousticalMath.SPL_Intensity(SW));
            //        }
            //    }
            //}
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                System.Drawing.Bitmap b = Properties.Resources.SPL;
                b.MakeTransparent(System.Drawing.Color.White);
                return b;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{C33A1F42-9EEC-4D7F-AB99-13C5CF0812B2}"); }
        }
    }
}