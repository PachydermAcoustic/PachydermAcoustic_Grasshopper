//'Pachyderm-Acoustic: Geometrical Acoustics for Rhinoceros (GPL)   
//' 
//'This file is part of Pachyderm-Acoustic. 
//' 
//'Copyright (c) 2008-2026, Open Research in Acoustical Science and Education, Inc. - a 501(c)3 nonprofit 
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
using Grasshopper2.Components;
using Grasshopper2.Parameters;
using Grasshopper2.UI;
using Grasshopper2.UI.Icon;
using Grasshopper2.Data;
using GrasshopperIO;
using Pachyderm_Acoustic.AbsorptionModels;
using Pachyderm_Acoustic.Environment;
using Pachyderm_Acoustic.Utilities;

namespace PachydermGH
{
    [IoId("d83ba0b8-5477-48dc-a94d-5d449f130c50")]
    public class Mat_TMM : Component
    {
        public Mat_TMM() : base(new Nomen("Transfer Matrix Material",
            "Evaluates an ordered layer train on a rigid backing using Pachyderm's existing Smart_Material solver. " +
            "Uses the material designer's air properties and frequency/angle sampling.", "Acoustics", "Materials"))
        {
            Threading = ThreadingState.SingleThreaded;
            DocumentChanged += AttachMerge;
        }
        public Mat_TMM(IReader reader) : base(reader) { Threading = ThreadingState.SingleThreaded; }

        private void AttachMerge(object sender, Grasshopper2.Doc.ObjectDocumentEventArgs args)
        {
            if (args.NewDocument == null) return;
            // Only the fresh constructor subscribes. Loading, pasting and undo
            // use the reader constructor and retain their existing connections.
            DocumentChanged -= AttachMerge;
            var document = args.NewDocument;
            Eto.Forms.Application.Instance.AsyncInvoke(() =>
            {
                if (Document != document || !document.Objects.Objects.Contains(this) || Parameters.Input(0).Inputs.Count != 0) return;
                var proxy = Grasshopper2.Framework.ObjectProxies.FindById(new Guid("00000000-9737-43b8-83bc-a52d766693ae"));
                var merge = proxy == null ? null : proxy.Emit(null) as Component;
                if (merge == null) return;
                // GH2's built-in Entwine implements Merge as a matching mode.
                // Its public mode property lives in the standard Data plugin.
                var matching = merge.GetType().GetProperty("MatchBehaviour");
                matching.SetValue(merge, Enum.Parse(matching.PropertyType, "Merge"));
                merge.UserName = "Merge";
                merge.Attributes.Pivot = new Eto.Drawing.PointF(Attributes.Pivot.X - 180, Attributes.Pivot.Y);
                if (!document.Objects.Add(merge)) return;
                var actions = new Grasshopper2.Undo.ActionList(new Grasshopper2.Undo.Actions.CreateObjectAction(merge));
                Connections.Connect(merge.Parameters.Output(0), Parameters.Input(0), actions);
                document.Undo.Do(new Grasshopper2.Undo.VerbNoun("Add", "TMM Merge"), actions);
            });
        }

        protected override void AddInputs(InputAdder pManager)
        {
            pManager.AddGeneric("Layers", "L", "Ordered layers from exposed face to rigid backing. Use Merge; one branch is one material.", Access.Twig);
            pManager.AddBoolean("Run", "Run", "Enable calculation. Parameter changes recompute the material while true.", Access.Item).Set(false);
        }

        protected override void AddOutputs(OutputAdder pManager)
        {
            pManager.AddGeneric("Material", "M", "Pachyderm Smart_Material on a rigid backing.", Access.Item);
            pManager.AddNumber("Frequency", "F", "Narrowband frequency samples in Hz, supplied by Universal.", Access.Twig);
            pManager.AddNumber("Angles", "Theta", "Signed incidence angles in degrees from the normal, in Universal's order. No exact zero-degree sample.", Access.Twig);
            pManager.AddNumber("Absorption", "A", "Random-incidence narrowband absorption, using Universal's Paris averaging.", Access.Twig);
            pManager.AddNumber("Angular Absorption", "AA", "Branch {angle index}; items follow Frequency. Uses Smart_Material reflection coefficients.", Access.Tree);
            pManager.AddNumber("Octave Frequencies", "Fo", "Eight octave band centres in Hz.", Access.Twig);
            pManager.AddNumber("Octave Absorption", "Ao", "Eight random-incidence coefficients from Smart_Material, as fractions.", Access.Twig);
            pManager.AddNumber("Third Octave Frequencies", "Ft", "24 third-octave band centres in Hz.", Access.Twig);
            pManager.AddNumber("Third Octave Absorption", "At", "24 random-incidence coefficients from Smart_Material, as fractions.", Access.Twig);
            pManager.AddGeneric("Impedance", "Z", "Complex surface impedance in Pa s/m. Branch {angle index}; items follow Frequency.", Access.Tree);
            pManager.AddNumber("Resistance", "Re Z", "Real surface impedance in Pa s/m; branch {angle index}.", Access.Tree);
            pManager.AddNumber("Reactance", "Im Z", "Imaginary surface impedance in Pa s/m; branch {angle index}.", Access.Tree);
            pManager.AddNumber("Angular Octave Absorption", "AAo", "Branch {angle index}; eight octave coefficients from Smart_Material.", Access.Tree);
            pManager.AddNumber("Angular Third Octave Absorption", "AAt", "Branch {angle index}; 24 third-octave coefficients from Smart_Material.", Access.Tree);
        }

        protected override void Process(IDataAccess DA)
        {
            bool run = false;
            if (!DA.GetItem(1, out run) || !run) return;
            if (!DA.GetTwig<ABS_Layer>(0, out var train)) return;
            List<ABS_Layer> layers = train.ToItemArray(ToArrayMethod.Always).ToList();
            if (layers.Count == 0) { throw new ArgumentException("Provide at least one layer."); }

            // Universal expects backing-to-face order and may mutate its list and layers.
            // Copy all layer data so shared Grasshopper inputs remain safe for parameter sweeps.
            List<ABS_Layer> snapshot = new List<ABS_Layer>(layers.Count);
            for (int i = layers.Count - 1; i >= 0; i--)
            {
                ABS_Layer layer = layers[i];
                if (layer == null || !(layer.depth > 0) || double.IsInfinity(layer.depth) ||
                    !Enum.IsDefined(typeof(ABS_Layer.LayerType), layer.T) || layer.T == ABS_Layer.LayerType.ThinPlate)
                { throw new ArgumentException("Provide supported layers with finite, positive thickness. ThinPlate has no implementation in the current solver."); }
                bool perforated = layer.T == ABS_Layer.LayerType.Perforated_Modal || layer.T == ABS_Layer.LayerType.Slotted_Modal ||
                    layer.T == ABS_Layer.LayerType.Perforated_Panel || layer.T == ABS_Layer.LayerType.Slotted_Panel;
                if (perforated && i == layers.Count - 1)
                { throw new ArgumentException("A perforated or slotted layer needs a cavity or material between it and the rigid backing."); }
                if (perforated && snapshot.Count > 0 && (int)snapshot[snapshot.Count - 1].T >= 4 && (int)snapshot[snapshot.Count - 1].T <= 7)
                { throw new ArgumentException("Separate consecutive perforated or slotted layers with an airspace or material layer."); }
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
            { throw new ArgumentException("Universal TMM calculation failed: " + error.Message); }
            if (material.Z == null || material.Reflection_Coefficient == null || material.frequency == null)
            { throw new ArgumentException("Universal returned an incomplete material."); }

            double[][] angular = Operations.Absorption_Coef(material.Reflection_Coefficient);
            double[] absorption = Operations.Random_Incidence_Paris(angular);
            if (absorption.Any(value => double.IsNaN(value) || double.IsInfinity(value)) ||
                material.RI_Coef.Any(value => double.IsNaN(value) || double.IsInfinity(value)) ||
                material.RI_Coef_3rd.Any(value => double.IsNaN(value) || double.IsInfinity(value)))
            { throw new ArgumentException("Universal returned non-finite absorption. Check layer parameters and model applicability."); }

            var resistance = material.Z.Select(row => row.Select(value => value.Real).ToArray()).ToArray();
            var reactance = material.Z.Select(row => row.Select(value => value.Imaginary).ToArray()).ToArray();
            var octave = Enumerable.Range(0, material.Angles.Length)
                .Select(a => material.Ang_Coef_Oct.Select(band => band[a]).ToArray()).ToArray();
            var third = Enumerable.Range(0, material.Angles.Length)
                .Select(a => material.Ang_Coef_3rd.Select(band => band[a]).ToArray()).ToArray();
            DA.SetItem(0, material);
            DA.SetTwig(1, (material.frequency).ToArray());
            DA.SetTwig(2, (material.Angles.Select(angle => angle.Real)).ToArray());
            DA.SetTwig(3, (absorption).ToArray());
            ComponentSupport.SetTree(DA, 4, Garden.TreeFromArrays(angular));
            DA.SetTwig(5, (AcousticalMath.OctaveCenters()).ToArray());
            DA.SetTwig(6, (material.RI_Coef).ToArray());
            DA.SetTwig(7, (AcousticalMath.ThirdOctaveCenters()).ToArray());
            DA.SetTwig(8, (material.RI_Coef_3rd).ToArray());
            ComponentSupport.SetTree(DA, 9, Garden.TreeFromArrays(material.Z));
            ComponentSupport.SetTree(DA, 10, Garden.TreeFromArrays(resistance));
            ComponentSupport.SetTree(DA, 11, Garden.TreeFromArrays(reactance));
            ComponentSupport.SetTree(DA, 12, Garden.TreeFromArrays(octave));
            ComponentSupport.SetTree(DA, 13, Garden.TreeFromArrays(third));
        }

        protected override IIcon IconInternal
        {
            get
            {
                using (var stream = typeof(Mat_Airspace).Assembly.GetManifestResourceStream("PachydermGH2.Resources.Transfer Matrix Material.png"))
                    return stream == null ? null : new PixelIcon(new Eto.Drawing.Bitmap(stream));
            }
        }

    }
}


