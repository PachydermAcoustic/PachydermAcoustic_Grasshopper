//'Pachyderm-Acoustic: Geometrical Acoustics for Rhinoceros (GPL) by Arthur van der Harten 
//' 
//'This file is part of Pachyderm-Acoustic. 
//' 
//'Copyright (c) 2008-2024, Arthur van der Harten 
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
                "Acoustics", "Visualization")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Mesh", "M", "Mesh to colour vertices of. If Pach_Map_Receiver is used, a map mesh will be extracted from the receiver.", GH_ParamAccess.item);
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
            m = new Mesh();
            DA.GetData<Mesh>(0, ref m);
            if (m.Faces.Count == 0)
            {
                Pachyderm_Acoustic.PachMapReceiver map = new Pachyderm_Acoustic.PachMapReceiver();
                DA.GetData(0, ref map);
                m = Pachyderm_Acoustic.Utilities.RCPachTools.HaretoRhinoMesh(map.Map_Mesh, false);
                if (m.Vertices.Count != 0) this.ClearRuntimeMessages();
            }

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

            if (m.Vertices.Count == C.Count)
            {
                for (int i = 0; i < m.Vertices.Count; i++)
                {
                    m.VertexColors.SetColor(i, C[i]);
                }
            }
            else if (m.Faces.Count == C.Count)
            {
                Mesh m_faces = new Mesh();
                for (int i = 0; i < m.Faces.Count; i++)
                {
                    Mesh face = new Mesh();
                    Point3f a, b, c, d;
                    m.Faces.GetFaceVertices(i, out a, out b, out c, out d);
                    m_faces.Vertices.AddVertices(new Point3f[4] { a, b, c, d });
                    m_faces.Faces.AddFace(i * 4 + 0, i * 4 + 1, i * 4 + 2, i * 4 + 3);
                    for(int j = 0; j < 4; j++) m_faces.VertexColors.Add(C[i]);
                }
                m = m_faces;
            }
            else throw new Exception("Count of colours must equal to count of vertices");

            DA.SetData(0, m); 
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                System.Drawing.Bitmap b = Properties.Resources.False_Color_Mesh_Mapping;
                b.MakeTransparent(System.Drawing.Color.White);
                return b;
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