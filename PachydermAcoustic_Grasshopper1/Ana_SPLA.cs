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
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
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
            pManager.AddGenericParameter("Energy Time Curve", "ETC", "Energy Time Curve", GH_ParamAccess.tree);
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
            GH_Structure<IGH_Goo> etcTree;
            if (!DA.GetDataTree(0, out etcTree)) return; // if no input, just return.
            List<double> SPLA = new List<double>();

            foreach (GH_Path path in etcTree.Paths)
            {
                IList<IGH_Goo> branchItems = etcTree[path];
                List<Audio_Signal> signals = new List<Audio_Signal>();
                List<double> Oct_SPL = new List<double>();
                foreach (IGH_Goo goo in branchItems)
                {
                    double SPL = 0;
                    Audio_Signal signal = goo as Audio_Signal;
                    if (signal != null)
                    {
                        signals.Add(signal);
                    }
                    else if (goo.CastTo(out SPL))
                    {
                        Oct_SPL.Add(SPL);
                    }
                }

                if (signals.Count > 0)
                {
                    foreach (var sig in signals)
                    {
                        Oct_SPL.Clear();
                        for (int i = 0; i < sig.Value.Length; i++)
                        {       
                            double sum = 0;
                            for (int j = 0; j < sig.Value[i].Length; j++)
                                sum += sig.Value[i][j];
                            Oct_SPL.Add(Pachyderm_Acoustic.Utilities.AcousticalMath.SPL_Intensity(sum));
                        }
                        SPLA.Add(Pachyderm_Acoustic.Utilities.AcousticalMath.Sound_Pressure_Level_A(Oct_SPL.ToArray()));
                    }
                }
                else if (Oct_SPL.Count > 0)
                {
                    for (int i = 0; i < Oct_SPL.Count; i += 8)
                        SPLA.Add(Pachyderm_Acoustic.Utilities.AcousticalMath.Sound_Pressure_Level_A(Oct_SPL.GetRange(i, 8).ToArray()));
                }
            }

            DA.SetDataList(0, SPLA);
            return;
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