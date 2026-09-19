using Grasshopper2.Components;
using Grasshopper2.Parameters;
using Grasshopper2.UI;
using Grasshopper2.UI.Icon;
using GrasshopperIO;
using Pachyderm_Acoustic;
using Pachyderm_Acoustic.Environment;
using PachydermGH;
using Rhino.Geometry;
using System;
using System.Collections.Generic;

namespace Pachyderm_GH
{
    [IoId("49d98751-62dc-4368-bff3-bfa95794a0c0")]
    public sealed class Polygon_Scene_Component : Component
    {
        public Polygon_Scene_Component() : base(new Nomen(
            "Polygon_Scene",
            "Constructs a scene with the existing geometry in the model and/or geometry from grasshopper definitions.",
            "Acoustics",
            "Model"))
        {
        }

        public Polygon_Scene_Component(IReader reader) : base(reader) { }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void AddInputs(InputAdder inputs)
        {
            inputs.AddBoolean("Rhino Geometry", "RG", "Does the component obtain the geometry from the Rhinoceros Model?");
            inputs.AddSurface("Grasshopper Geometry", "GG", "Add any grasshopper geometry here", Access.Tree, Requirement.MayBeNull).Requirement = Requirement.MayBeMissing;
            inputs.AddInteger("Grasshopper Layers", "GL", "For each Geometry in GG, indicate what layer (by integer id) to copy acoustical properties from.", Access.Tree).Requirement = Requirement.MayBeMissing;
            inputs.AddInteger("Voxel Grid Depth", "VG", "Number of voxels in each dimentions. (0 for no optimisation)").Set(7);
            inputs.AddGeneric("Medium Properties", "MP", "Atmospheric properties (see 'Medium Propeties') according to atmospheric pressure, termperature, and relative humidity.").Set(new Uniform_Medium(0, 1000, 293.15, 50, false));
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void AddOutputs(OutputAdder outputs)
        {
            outputs.AddGeneric("Scene", "S", "The completed Pachyderm Scene");
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="access">The IDataAccess object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void Process(IDataAccess access)
        {
            bool RG = false;
            access.GetItem<bool>(0, out RG);

            Grasshopper2.Data.Tree<GeometryBase> GG;
            access.GetTree<GeometryBase>(1, out GG);
            Grasshopper2.Data.Tree<int> GL;
            access.GetTree<int>(2, out GL);

            // Materialize trees to flat lists for robust counting and passing to APIs
            List<GeometryBase> ggItems = new List<GeometryBase>(GG?.AllItems ?? Array.Empty<GeometryBase>());
            List<int> glItems = new List<int>(GL?.AllItems ?? Array.Empty<int>());

            // Keep GH1 default of 7 unless user overrides
            int VG = 7;
            access.GetItem<int>(3, out VG);
            Pachyderm_Acoustic.Environment.Medium_Properties MP = new Uniform_Medium(0, 1031.25, 293.15, 50, false);
            access.GetItem<Medium_Properties>(4, out MP);

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
            else if (ggItems.Count == 0)
            {
                return;
            }

            if (RC_List.Count == 0 && GG.LeafCount == 0) throw new Exception("Scene could not be constructed because there is no geometry...");
            if (GG.LeafCount != GL.LeafCount) throw new Exception("Number of Grasshopper Objects(GG) and number of Rhino Layer(GL) indices must match (one layer per object)");

            List<Brep> RhG = new List<Brep>();
            foreach (Rhino.Geometry.GeometryBase G in GG.AllItems)
            {
                Brep B = G as Brep;
                if (B == null) throw new Exception("at least one entry in GG is not a Brep...");
                RhG.Add(B);
            }

            //Can we register edges later?
            Pachyderm_Acoustic.Environment.RhCommon_PolygonScene PS = new Pachyderm_Acoustic.Environment.RhCommon_PolygonScene(RC_List, RhG, new List<int>(GL.AllItems).ToArray(), false, MP.Tk - 273.15, MP.hr, MP.Pa, 0, false, true);
            PS.partition(VG, 4);

            if (PS.hasnulllayers)
            {
                throw new Exception("Set materials to layer using the Materials tab in Pachyderm for Rhino.");
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
                var resourceName = "Pachyderm_GH.Icons.Polygon_Scene.png";

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
        //protected override IIcon IconInternal => new Grasshopper2.UI.Icon.PixelIcon("Polygon_Scene");
    }
}
