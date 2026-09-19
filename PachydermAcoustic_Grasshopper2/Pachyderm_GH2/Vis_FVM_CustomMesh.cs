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
using System.Collections.Generic;
using Grasshopper2.Data;
using Grasshopper2.Interop;

namespace PachydermGH
{
    [IoId("0D22141C-8833-4941-BB80-F3034EDC1175")]
    public class FVM_InsertMesh : Component
    {
        Pachyderm_Acoustic.UI.PachTDNumericControl Sim;

        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public FVM_InsertMesh()
            : base(new Nomen("FVM Custom Mesh",
                "Allows you to specify a custom mesh for use with the display conduit of the Finite Volume Method in Pachyderm.",
                "Acoustics", "Visualization"))
        {
            Sim = Pachyderm_Acoustic.UI.PachTDNumericControl.Instance;
            if (Sim != null) Sim.Incremented += Sim_Incremented;
        }

        public FVM_InsertMesh(IReader reader) : base(reader) { }

        private void Sim_Incremented(object sender, EventArgs e)
        {
            if (Sim.FDTD == null && Sim.FDTD.m_referenceC == null) return;
            //GH_Document doc = OnPingDocument();
            //if (doc != null)
            //{
            //    Rhino.RhinoApp.MainApplicationWindow.Invoke(() => { doc.ScheduleSolution(10) });
            //}
            //else
            //{
            //this.ExpireSolution(true);
            //}
            //Rhino.RhinoApp.MainApplicationWindow.Invoke({ this.ExpireSolution(true); });
            //Grasshopper.Instances.ActiveCanvas.Invoke(ScheduleSolution(0));
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void AddInputs(InputAdder inputs)
        {
            inputs.AddMesh("Mesh", "M", "Mesh to insert into the display conduit. For best results, use a mesh with a similar step size between vertices to that of dx, dy, or dz between model nodes.", Access.Tree);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void AddOutputs(OutputAdder outputs)
        {
            outputs.AddMesh("Mesh", "M", "The mesh shown in the Rhino simulation.", Access.Item);
            outputs.AddNumber("Pressure", "P", "The pressure at each face in the mesh.", Access.Tree);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="access">The access object is used to retrieve from inputs and store in outputs.</param>
        protected override void Process(IDataAccess access)
        {
            Pachyderm_Acoustic.UI.PachTDNumericControl Sim = Pachyderm_Acoustic.UI.PachTDNumericControl.Instance;
            if (Sim.FDTD == null) return;
            //access.AddMessage("dX = " + Math.Round(Sim.FDTD.dx, 3) + ", dY = " + Math.Round(Sim.FDTD.dy, 3) + ", dZ = " + Math.Round(Sim.FDTD.dz, 3));

            Grasshopper2.Data.Tree<Mesh> m;
            
            if (!access.GetTree(0,out m)) return;
            List<Mesh> MItems = new List<Mesh>(m?.AllItems ?? Array.Empty<Mesh>());
            
            List<Mesh> AllMesh = new List<Mesh>();

            Sim.FDTD.Insert_Mesh_Sections(AllMesh.ToArray());

            access.SetItem(0, Sim.FDTD.m_templateC);
            
            double[] p = new double[Sim.FDTD.m_referenceC.Count];
            for (int i = 0; i < p.Length; i++) p[i] = Sim.FDTD.m_referenceC[i].P;

            List<double[]> pressureTree = new List<double[]>();
            pressureTree.Add(p);

            access.SetTree(1, Garden.TreeFromArrays(pressureTree.ToArray()));
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

        //protected override IIcon IconInternal => new Grasshopper2.UI.Icon.PixelIcon("False_Color_Mesh_Mapping");
    }
}