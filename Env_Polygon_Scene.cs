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
using Pachyderm_Acoustic.Environment;
using Rhino.Geometry;

namespace PachydermGH
{
    public class PolygonScene_Component : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public PolygonScene_Component()
            : base("Polygon Scene", "Poly Scene",
                "Constructs a scene with the existing geometry in the model and/or geometry from grasshopper definitions",
                "Acoustics", "Model")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("Rhino Geometry", "RG", "Does the component obtain the geometry from the Rhinoceros Model?", GH_ParamAccess.item);
            pManager.AddGeometryParameter("Grasshopper Geometry", "GG", "Add any grasshopper geometry here", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Grasshopper Layers", "GL", "For each Geometry in GG, indicate what layer (by integer id) to copy acoustical properties from.", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Voxel Grid Depth", "VG", "Number of voxels in each dimentions. (0 for no optimisation)", GH_ParamAccess.item);
            pManager.AddGenericParameter("Medium Properties", "MP", "Atmospheric properties (see 'Medium Propeties') according to atmospheric pressure, termperature, and relative humidity.", GH_ParamAccess.item); 

            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[4].Optional = true;
            Grasshopper.Kernel.Parameters.Param_Integer param = (pManager[3] as Grasshopper.Kernel.Parameters.Param_Integer);
            if (param != null) param.SetPersistentData(7);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Scene", "S", "The completed Pachyderm Scene", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            bool RG = false;
            DA.GetData<bool>(0, ref RG);
            List<GeometryBase> GG = new List<GeometryBase>();
            DA.GetDataList<GeometryBase>(1, GG);
            List<int> GL = new List<int>();
            DA.GetDataList<int>(2, GL);
            int VG = 2;
            DA.GetData<int>(3, ref VG);
            Pachyderm_Acoustic.Environment.Medium_Properties MP = new Uniform_Medium(0, 1031.25, 293.15, 50, false);
            DA.GetData<Medium_Properties>(4, ref MP);

            Rhino.DocObjects.ObjectEnumeratorSettings settings = new Rhino.DocObjects.ObjectEnumeratorSettings();
            settings.DeletedObjects = false;
            settings.HiddenObjects = false;
            settings.LockedObjects = true;
            settings.NormalObjects = true;
            settings.VisibleFilter = true;
            settings.ObjectTypeFilter = Rhino.DocObjects.ObjectType.Brep & Rhino.DocObjects.ObjectType.Surface & Rhino.DocObjects.ObjectType.Extrusion;
            List<Rhino.DocObjects.RhinoObject> RC_List = new List<Rhino.DocObjects.RhinoObject>();

            if (RG)
            {
                foreach (Rhino.DocObjects.RhinoObject RHobj in Rhino.RhinoDoc.ActiveDoc.Objects.GetObjectList(settings))
                {
                    if (RHobj.ObjectType == Rhino.DocObjects.ObjectType.Brep || RHobj.ObjectType == Rhino.DocObjects.ObjectType.Surface || RHobj.ObjectType == Rhino.DocObjects.ObjectType.Extrusion)
                    {
                        RC_List.Add(RHobj);
                    }
                }
            }
            else if (GG.Count == 0)
            {
                return;
            }

            if (RC_List.Count == 0 && GG.Count == 0) throw new Exception("Scene could not be constructed because there is no geometry...");
            if (GG.Count != GL.Count) throw new Exception("Number of Grasshopper Objects(GG) and number of Rhino Layer(GL) indices must match (one layer per object)");

            List<Brep> RhG = new List<Brep>();
            foreach(Rhino.Geometry.GeometryBase G in GG)
            {
                Brep B = G as Brep;
                if (B == null) throw new Exception("at least one entry in GG is not a Brep...");
                RhG.Add(B);
            }

            Pachyderm_Acoustic.Environment.RhCommon_PolygonScene PS = new Pachyderm_Acoustic.Environment.RhCommon_PolygonScene(RC_List, RhG, GL.ToArray(), false, MP.Tk - 273.15, MP.hr, MP.Pa, 0, false, true);
            PS.partition(VG);

            if (PS.hasnulllayers)
            {
                throw new Exception("Set materials to layer using the Materials tab in Pachyderm for Rhino.");
            }
            else
            {
                DA.SetData(0, PS);
            }
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                System.Drawing.Bitmap b = Properties.Resources.Polygon_Scene;
                b.MakeTransparent(System.Drawing.Color.White);
                return b;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{d70814c5-95a6-41ef-acec-c602dcf6fb55}"); }
        }
    }
}