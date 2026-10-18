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
using Pachyderm_Acoustic.AbsorptionModels;
using Pachyderm_Acoustic.Environment;
using Pachyderm_Acoustic.Utilities;

namespace PachydermGH
{
    public class Mat_TMM : GH_Component
    {
        public Mat_TMM() : base("Transfer Matrix Material", "TMM",
            "Evaluates an ordered layer train on a rigid backing using Pachyderm's existing Smart_Material solver. " +
            "Uses the material designer's air properties and frequency/angle sampling.", "Acoustics", "Materials") { }

        private bool attachMerge = true;

        public override bool Read(GH_IO.Serialization.GH_IReader reader)
        {
            // Saved and pasted objects already carry their document connections.
            attachMerge = false;
            return base.Read(reader);
        }

        public override void AddedToDocument(GH_Document document)
        {
            base.AddedToDocument(document);
            if (!attachMerge) return;
            attachMerge = false;
            document.ScheduleSolution(1, doc =>
            {
                if (OnPingDocument() != doc || Params.Input[0].SourceCount != 0) return;
                var merge = Grasshopper.Instances.ComponentServer.EmitObject(new Guid("3cadddef-1e2b-4c09-9390-0e8f78f7609f")) as GH_Component;
                if (merge == null) return;
                merge.CreateAttributes();
                merge.Attributes.Pivot = new System.Drawing.PointF(Attributes.Pivot.X - 180, Attributes.Pivot.Y);
                doc.UndoUtil.RecordAddObjectEvent("Add TMM Merge", merge);
                doc.AddObject(merge, false);
                Params.Input[0].AddSource(merge.Params.Output[0]);
                ExpireSolution(false);
            });
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Layers", "L", "Ordered layers from exposed face to rigid backing. Use Merge; one branch is one material.", GH_ParamAccess.list);
            pManager.AddBooleanParameter("Run", "Run", "Enable calculation. Parameter changes recompute the material while true.", GH_ParamAccess.item, false);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Material", "M", "Pachyderm Smart_Material on a rigid backing.", GH_ParamAccess.item);
            pManager.AddNumberParameter("Frequency", "F", "Narrowband frequency samples in Hz, supplied by Universal.", GH_ParamAccess.list);
            pManager.AddNumberParameter("Angles", "Theta", "Signed incidence angles in degrees from the normal, in Universal's order. No exact zero-degree sample.", GH_ParamAccess.list);
            pManager.AddNumberParameter("Absorption", "A", "Random-incidence narrowband absorption, using Universal's Paris averaging.", GH_ParamAccess.list);
            pManager.AddNumberParameter("Angular Absorption", "AA", "Branch {angle index}; items follow Frequency. Uses Smart_Material reflection coefficients.", GH_ParamAccess.tree);
            pManager.AddNumberParameter("Octave Frequencies", "Fo", "Eight octave band centres in Hz.", GH_ParamAccess.list);
            pManager.AddNumberParameter("Octave Absorption", "Ao", "Eight random-incidence coefficients from Smart_Material, as fractions.", GH_ParamAccess.list);
            pManager.AddNumberParameter("Third Octave Frequencies", "Ft", "24 third-octave band centres in Hz.", GH_ParamAccess.list);
            pManager.AddNumberParameter("Third Octave Absorption", "At", "24 random-incidence coefficients from Smart_Material, as fractions.", GH_ParamAccess.list);
            pManager.AddGenericParameter("Impedance", "Z", "Complex surface impedance in Pa s/m. Branch {angle index}; items follow Frequency.", GH_ParamAccess.tree);
            pManager.AddNumberParameter("Resistance", "Re Z", "Real surface impedance in Pa s/m; branch {angle index}.", GH_ParamAccess.tree);
            pManager.AddNumberParameter("Reactance", "Im Z", "Imaginary surface impedance in Pa s/m; branch {angle index}.", GH_ParamAccess.tree);
            pManager.AddNumberParameter("Angular Octave Absorption", "AAo", "Branch {angle index}; eight octave coefficients from Smart_Material.", GH_ParamAccess.tree);
            pManager.AddNumberParameter("Angular Third Octave Absorption", "AAt", "Branch {angle index}; 24 third-octave coefficients from Smart_Material.", GH_ParamAccess.tree);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            bool run = false;
            if (!DA.GetData(1, ref run) || !run) return;
            List<ABS_Layer> layers = new List<ABS_Layer>();
            if (!DA.GetDataList(0, layers)) return;
            if (layers.Count == 0) { AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Provide at least one layer."); return; }

            // Universal expects backing-to-face order and may mutate its list and layers.
            // Copy all layer data so shared Grasshopper inputs remain safe for parameter sweeps.
            List<ABS_Layer> snapshot = new List<ABS_Layer>(layers.Count);
            for (int i = layers.Count - 1; i >= 0; i--)
            {
                ABS_Layer layer = layers[i];
                if (layer == null || !(layer.depth > 0) || double.IsInfinity(layer.depth) ||
                    !Enum.IsDefined(typeof(ABS_Layer.LayerType), layer.T) || layer.T == ABS_Layer.LayerType.ThinPlate)
                { AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Provide supported layers with finite, positive thickness. ThinPlate has no implementation in the current solver."); return; }
                bool perforated = layer.T == ABS_Layer.LayerType.Perforated_Modal || layer.T == ABS_Layer.LayerType.Slotted_Modal ||
                    layer.T == ABS_Layer.LayerType.Perforated_Panel || layer.T == ABS_Layer.LayerType.Slotted_Panel;
                if (perforated && i == layers.Count - 1)
                { AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "A perforated or slotted layer needs a cavity or material between it and the rigid backing."); return; }
                if (perforated && snapshot.Count > 0 && (int)snapshot[snapshot.Count - 1].T >= 4 && (int)snapshot[snapshot.Count - 1].T <= 7)
                { AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Separate consecutive perforated or slotted layers with an airspace or material layer."); return; }
                snapshot.Add(new ABS_Layer(layer.T, layer.depth, layer.pitch, layer.width, layer.Flow_Resist,
                    layer.porosity, layer.Embodied_Carbon, layer.Material_Name, layer.shape, layer.grid)
                {
                    density = layer.density,
                    YoungsModulus = layer.YoungsModulus,
                    PoissonsRatio = layer.PoissonsRatio,
                    tortuosity = layer.tortuosity,
                    Viscous_Characteristic_Length = layer.Viscous_Characteristic_Length,
                    Thermal_Permeability = layer.Thermal_Permeability,
                    SpeedOfSound = layer.SpeedOfSound
                });
            }

            Smart_Material material;
            try { material = new Smart_Material(false, snapshot, 44100, 1.2, 343); }
            catch (Exception error)
            { AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Universal TMM calculation failed: " + error.Message); return; }
            if (material.Z == null || material.Reflection_Coefficient == null || material.frequency == null)
            { AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Universal returned an incomplete material."); return; }

            double[][] angular = Operations.Absorption_Coef(material.Reflection_Coefficient);
            double[] absorption = Operations.Random_Incidence_Paris(angular);
            if (absorption.Any(value => double.IsNaN(value) || double.IsInfinity(value)) ||
                material.RI_Coef.Any(value => double.IsNaN(value) || double.IsInfinity(value)) ||
                material.RI_Coef_3rd.Any(value => double.IsNaN(value) || double.IsInfinity(value)))
            { AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Universal returned non-finite absorption. Check layer parameters and model applicability."); return; }

            var alpha = new GH_Structure<GH_Number>();
            var impedance = new GH_Structure<GH_ObjectWrapper>();
            var resistance = new GH_Structure<GH_Number>();
            var reactance = new GH_Structure<GH_Number>();
            var octave = new GH_Structure<GH_Number>();
            var third = new GH_Structure<GH_Number>();
            for (int a = 0; a < material.Angles.Length; a++)
            {
                GH_Path path = new GH_Path(a);
                alpha.AppendRange(angular[a].Select(value => new GH_Number(value)), path);
                impedance.AppendRange(material.Z[a].Select(value => new GH_ObjectWrapper(value)), path);
                resistance.AppendRange(material.Z[a].Select(value => new GH_Number(value.Real)), path);
                reactance.AppendRange(material.Z[a].Select(value => new GH_Number(value.Imaginary)), path);
                octave.AppendRange(material.Ang_Coef_Oct.Select(band => new GH_Number(band[a])), path);
                third.AppendRange(material.Ang_Coef_3rd.Select(band => new GH_Number(band[a])), path);
            }
            DA.SetData(0, material);
            DA.SetDataList(1, material.frequency);
            DA.SetDataList(2, material.Angles.Select(angle => angle.Real));
            DA.SetDataList(3, absorption);
            DA.SetDataTree(4, alpha);
            DA.SetDataList(5, AcousticalMath.OctaveCenters());
            DA.SetDataList(6, material.RI_Coef);
            DA.SetDataList(7, AcousticalMath.ThirdOctaveCenters());
            DA.SetDataList(8, material.RI_Coef_3rd);
            DA.SetDataTree(9, impedance);
            DA.SetDataTree(10, resistance);
            DA.SetDataTree(11, reactance);
            DA.SetDataTree(12, octave);
            DA.SetDataTree(13, third);
        }

        protected override System.Drawing.Bitmap Icon { get { return Properties.Resources.Medium_Properties; } }
        public override Guid ComponentGuid { get { return new Guid("d83ba0b8-5477-48dc-a94d-5d449f130c50"); } }
    }
}
