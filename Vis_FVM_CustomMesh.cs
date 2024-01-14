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
using System.Linq;

namespace PachydermGH
{
    public class FVM_InsertMesh : GH_Component
    {
        Pachyderm_Acoustic.UI.Pach_TD_Numeric_Control Sim;

        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public FVM_InsertMesh()
            : base("Custom Mesh for Finite Volume Method", "FVM Custom Mesh",
                "Allows you to specify a custom mesh for use with the display conduit of the Finite Volume Method in Pachyderm.",
                "Acoustics", "Visualization")
        {
            Sim = Pachyderm_Acoustic.UI.Pach_TD_Numeric_Control.Instance;
            if (Sim != null) Sim.Incremented += Sim_Incremented;
        }

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
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Mesh", "M", "Mesh to insert into the display conduit. For best results, use a mesh with a similar step size between vertices to that of dx, dy, or dz between model nodes.", GH_ParamAccess.tree);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "The mesh shown in the Rhino simulation.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Pressure", "P", "The pressure at each face in the mesh.", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Pachyderm_Acoustic.UI.Pach_TD_Numeric_Control Sim = Pachyderm_Acoustic.UI.Pach_TD_Numeric_Control.Instance;
            if (Sim.FDTD == null) return;
            this.Message = "dX = " + Math.Round(Sim.FDTD.dx, 3) + ", dY = " + Math.Round(Sim.FDTD.dy, 3) + ", dZ = " + Math.Round(Sim.FDTD.dz, 3);

            Grasshopper.Kernel.Data.GH_Structure<Grasshopper.Kernel.Types.IGH_Goo> m;
            if (!DA.GetDataTree(0,out m)) return;

            List<Grasshopper.Kernel.Types.IGH_Goo> M = m.AllData(true).ToList();

            List<Mesh> AllMesh = new List<Mesh>();

            foreach (Grasshopper.Kernel.Types.IGH_Goo ms in M)
            {
                Mesh mesh = new Mesh();   
                if (Grasshopper.Kernel.GH_Convert.ToMesh(ms, ref mesh, GH_Conversion.Primary)) AllMesh.Add(mesh);
            }

            Sim.FDTD.Insert_Mesh_Sections(AllMesh.ToArray());

            DA.SetData(0, Sim.FDTD.m_templateC);

            double[] p = new double[Sim.FDTD.m_referenceC.Count];
            for (int i = 0; i < p.Length; i++) p[i] = Sim.FDTD.m_referenceC[i].P;
            DA.SetDataList(1, p);
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

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{0D22141C-8833-4941-BB80-F3034EDC1175}"); }
        }
    }
}