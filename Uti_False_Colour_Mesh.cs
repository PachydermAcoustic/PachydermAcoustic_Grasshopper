//'Pachyderm-Acoustic: Geometrical Acoustics for Rhinoceros (GPL) by Arthur van der Harten 
//' 
//'This file is part of Pachyderm-Acoustic. 
//' 
//'Copyright (c) 2008-2015, Arthur van der Harten 
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

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace PachydermGH
{
    public class False_Colour_Mesh : GH_Component
    {
        Mesh m = new Mesh();

        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public False_Colour_Mesh()
            : base("False Colour Mesh Mapping", "False Colour",
                "Mapping of false colors onto vertices of meshes.",
                "Acoustics", "General")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "Mesh to colour vertices of", GH_ParamAccess.item);
            pManager.AddColourParameter("Colours", "C", "Colours to assign to vertices", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddMeshParameter("Coloured Mesh", "CM", "Coloured Mesh", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            DA.GetData<Mesh>(0, ref m);
            List<System.Drawing.Color> C = new List<System.Drawing.Color>();
            try
            {
                DA.GetDataList<System.Drawing.Color>(1, C);
            }
            catch 
            {
                List<double[]> Cd = new List<double[]>();
                DA.GetDataList<double[]>(1, Cd);
            }

            if (m.Vertices.Count != C.Count) throw new Exception("Count of colours must equal count if vertices");

            for (int i = 0; i < m.Vertices.Count; i++)
            {
                m.VertexColors.SetColor(i, C[i]);
            }
            DA.SetData(0, m); 
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return null;
            }
        }

        public override void DrawViewportMeshes(IGH_PreviewArgs args)
        {
            args.Display.DrawMeshFalseColors(m);
            //base.DrawViewportMeshes(args);
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{bff7a015-2ea0-4c56-9518-8d965e6bc19a}"); }
        }
    }
}