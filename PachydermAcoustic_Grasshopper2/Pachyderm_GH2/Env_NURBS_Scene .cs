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
using System.Collections.Generic;
using Grasshopper2.Data;

namespace PachydermGH
{
    [IoId("6EFF62BE-CE5E-45E0-93A7-AC9291B2E370")]
    public class NURBSScene_Component : Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public NURBSScene_Component()
            : base(new Nomen("NURBS Scene",
                "Constructs a scene with the existing geometry in the model and/or geometry from grasshopper definitions",
                "Acoustics", "Model"))
        {
            Threading = Grasshopper2.Components.ThreadingState.UiSingleThreaded;
        }

        public NURBSScene_Component(IReader reader) : base(reader) { Threading = Grasshopper2.Components.ThreadingState.UiSingleThreaded; }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void AddInputs(InputAdder inputs)
        {
            inputs.AddBoolean("Rhino Geometry", "RG", "Does the component obtain the geometry from the Rhinoceros Model?", Access.Item);
            inputs.AddGeneric("Grasshopper Geometry", "GG", "Add any grasshopper geometry here", Access.Tree, Requirement.MayBeMissing);
            inputs.AddInteger("Grasshopper Layers", "GL", "For each Geometry in GG, indicate what layer (by integer id) to copy acoustical properties from.", Access.Tree, Requirement.MayBeMissing);
            inputs.AddInteger("Voxel Grid Depth", "VG", "Number of voxels in each dimentions. (0 for no optimisation)", new Grasshopper2.UI.UiInteger(7), Access.Item);

            inputs[1].Requirement = Requirement.MayBeMissing;
            inputs[2].Requirement = Requirement.MayBeMissing;

            //Grasshopper.Kernel.Parameters.Param_Integer param = (inputs[3] as Grasshopper.Kernel.Parameters.Param_Integer);
            //if (param != null) param.SetPersistentData(7);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void AddOutputs(OutputAdder outputs)
        {
            outputs.AddGeneric("Scene", "S", "The completed Pachyderm Scene", Access.Item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void Process(IDataAccess access)
        {
            bool RG = false;
            access.GetItem<bool>(0, out RG);
            Tree<GeometryBase> GG_T;
            access.GetTree<GeometryBase>(1, out GG_T);
            Tree<int> GL_T;
            access.GetTree<int>(2, out GL_T);
            GG_T = GG_T ?? Garden.TreeFromList(Array.Empty<GeometryBase>());
            GL_T = GL_T ?? Garden.TreeFromList(Array.Empty<int>());
            int VG = 2;
            access.GetItem<int>(3, out VG);

            Rhino.DocObjects.ObjectEnumeratorSettings settings = new Rhino.DocObjects.ObjectEnumeratorSettings();
            settings.DeletedObjects = false;
            settings.HiddenObjects = false;
            settings.LockedObjects = true;
            settings.NormalObjects = true;
            settings.VisibleFilter = true;
            settings.ObjectTypeFilter = Rhino.DocObjects.ObjectType.Brep | Rhino.DocObjects.ObjectType.Surface | Rhino.DocObjects.ObjectType.Extrusion;
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
            else if (GG_T.ItemCount == 0)
            {
                return;
            }
            else
            {
                // Keep supplied GH geometry.
                // Keep its layer assignments.
            }

            if (RC_List.Count == 0 && GG_T.ItemCount == 0) throw new Exception("Scene could jnot be constructed because there is no geometry...");
            if (GG_T.ItemCount != GL_T.ItemCount) throw new Exception("Number of Grasshopper Objects(GG) and number of Rhino Layer(GL) indices must match (one layer per object)");

            Pachyderm_Acoustic.Environment.RhCommon_Scene PS = new Pachyderm_Acoustic.Environment.RhCommon_Scene(RC_List, 20, 50, 1031.25, 0, false, true, new List<GeometryBase>(GG_T.AllItems),new List<int>(GL_T.AllItems));
            PS.partition(VG,4);

            if (PS.hasnulllayers)
            {
                throw new Exception("Set materials to layer using the Materials tab in Pachyderm ror Rhino.");
            }
            else
            {
                access.SetItem(0, PS);
            }
        }
        protected override IIcon IconInternal
        {
            get
            {
                var assembly = typeof(SPLETC).Assembly;
                var resourceName = "PachydermGH2.Resources.Nurb Scene.png";

                using (var stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream == null) return null;

                    // FromStream reads serialized .ghicon data, not PNG/BMP images.
                    // The PixelIcon retains the bitmap for its cached lifetime.
                    return new Grasshopper2.UI.Icon.PixelIcon(new Eto.Drawing.Bitmap(stream));
                }
            }
        }

        //protected override IIcon IconInternal => new Grasshopper2.UI.Icon.PixelIcon("Nurb_Scene");
    }
}
