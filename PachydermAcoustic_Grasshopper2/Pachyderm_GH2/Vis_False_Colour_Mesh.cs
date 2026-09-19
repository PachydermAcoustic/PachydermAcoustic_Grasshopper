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
            Threading = Grasshopper2.Components.ThreadingState.SingleThreaded;
        }

        public False_Colour_Mesh(IReader reader) : base(reader) { Threading = Grasshopper2.Components.ThreadingState.SingleThreaded; }

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
            if (!access.GetItem<object>(0, out var input)) return;
            Mesh mesh;
            if (input is Mesh supplied) mesh=supplied.DuplicateMesh();
            else if (input is Pachyderm_Acoustic.PachMapReceiver map) mesh=Pachyderm_Acoustic.Utilities.RCPachTools.HaretoRhinoMesh(map.Map_Mesh,false);
            else throw new ArgumentException("Provide a mesh or mapping receiver.");
            var colors=ComponentSupport.Items<System.Drawing.Color>(access,1);
            if(colors.Length==mesh.Vertices.Count) {
                mesh.VertexColors.Clear(); foreach(var color in colors) mesh.VertexColors.Add(color);
            } else if(colors.Length==mesh.Faces.Count) {
                var faces=new Mesh();
                for(int i=0;i<mesh.Faces.Count;i++) {
                    var face=mesh.Faces[i]; int offset=faces.Vertices.Count;
                    faces.Vertices.Add(mesh.Vertices[face.A]); faces.Vertices.Add(mesh.Vertices[face.B]); faces.Vertices.Add(mesh.Vertices[face.C]);
                    if(face.IsQuad) { faces.Vertices.Add(mesh.Vertices[face.D]); faces.Faces.AddFace(offset,offset+1,offset+2,offset+3); }
                    else faces.Faces.AddFace(offset,offset+1,offset+2);
                    for(int j=0;j<(face.IsQuad?4:3);j++) faces.VertexColors.Add(colors[i]);
                }
                mesh=faces;
            } else throw new ArgumentException("Provide one colour per vertex or per face.");
            m=mesh; access.SetItem(0,mesh);
        }
        protected override IIcon IconInternal
        {
            get
            {
                var assembly = typeof(SPLETC).Assembly;
                var resourceName = "PachydermGH2.Resources.False Color Mesh Mapping.png";

                using (var stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream == null) return null;

                    // FromStream reads serialized .ghicon data, not PNG/BMP images.
                    // The PixelIcon retains the bitmap for its cached lifetime.
                    return new Grasshopper2.UI.Icon.PixelIcon(new Eto.Drawing.Bitmap(stream));
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
