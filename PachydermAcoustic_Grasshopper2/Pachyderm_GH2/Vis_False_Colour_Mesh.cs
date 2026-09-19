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
using Rhino.Display;
using Grasshopper2.Display;

namespace PachydermGH
{
    [IoId("{bff7a015-2ea0-4c56-9518-8d965e6bc19a}")]
    public class False_Colour_Mesh : Component
    {
        Mesh m = new Mesh();

        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public False_Colour_Mesh()
            : base(new Nomen("False Colour Mesh Mapping",
                "Mapping of false colors onto vertices of meshes.",
                "Acoustics", "Visualization"))
        {
        }

        public False_Colour_Mesh(IReader reader) : base(reader) { }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void AddInputs(InputAdder inputs)
        {
            inputs.AddGeneric("Mesh", "M", "Mesh to colour vertices of. If Pach_Map_Receiver is used, a map mesh will be extracted from the receiver.", Access.Item);
            inputs.AddColour("Colours", "C", "Colours to assign to vertices", Access.Tree);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void AddOutputs(OutputAdder outputs)
        {
            outputs.AddMesh("Coloured Mesh", "CM", "Coloured Mesh", Access.Item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="access">The access object is used to retrieve from inputs and store in outputs.</param>
        protected override void Process(IDataAccess access)
        {
            m = new Mesh();
            access.GetItem<Mesh>(0, out m);
            if (m.Faces.Count == 0)
            {
                Pachyderm_Acoustic.PachMapReceiver map = new Pachyderm_Acoustic.PachMapReceiver();
                access.GetItem(0, out map);
                m = Pachyderm_Acoustic.Utilities.RCPachTools.HaretoRhinoMesh(map.Map_Mesh, false);
                //if (m.Vertices.Count != 0) this.ClearRuntimeMessages();
            }

            Tree<System.Drawing.Color> C;
            //try
            //{
            access.GetTree<System.Drawing.Color>(1, out C);
            //}
            //catch 
            //{
            //    Tree<double[]> Cd;
            //    access.GetTree<double[]>(1, out Cd);
            //}

            if (m.Vertices.Count == C.ItemCount)
            {
                for (int i = 0; i < m.Vertices.Count; i++)
                {
                    m.VertexColors.SetColor(i, C.Items[i]);
                }
            }
            else if (m.Faces.Count == C.ItemCount)
            {
                Mesh m_faces = new Mesh();
                for (int i = 0; i < m.Faces.Count; i++)
                {
                    Mesh face = new Mesh();
                    Point3f a, b, c, d;
                    m.Faces.GetFaceVertices(i, out a, out b, out c, out d);
                    m_faces.Vertices.AddVertices(new Point3f[4] { a, b, c, d });
                    m_faces.Faces.AddFace(i * 4 + 0, i * 4 + 1, i * 4 + 2, i * 4 + 3);
                    for(int j = 0; j < 4; j++) m_faces.VertexColors.Add(C.Items[i]);
                }
                m = m_faces;
            }
            else throw new Exception("Count of colours must equal to count of vertices");

            access.SetItem(0, m); 
        }
        protected override IIcon IconInternal
        {
            get
            {
                var assembly = typeof(SPLETC).Assembly;
                var resourceName = "Pachyderm_GH.Icons.False_Color_Mesh_Mapping.png";

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

        public override void DisplayFaces(DisplayPipeline pipeline, Guises guises, ref BoundingBox extents)
        {
            pipeline.DrawMeshFalseColors(m);
            base.DisplayFaces(pipeline, guises, ref extents);
        }
    }
}