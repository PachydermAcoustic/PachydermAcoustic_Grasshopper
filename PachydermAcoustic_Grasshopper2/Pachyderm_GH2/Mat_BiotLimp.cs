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
using Pachyderm_Acoustic.AbsorptionModels;

namespace PachydermGH
{
    [IoId("9f019ea4-fbe9-4629-a43c-99cb64f4ee1e")]
    public class Mat_BiotLimp : Component
    {
        public Mat_BiotLimp()
            : base(new Nomen("Biot Limp Layer", "Porous layer using Pachyderm's limp Biot model.", "Acoustics", "Materials"))
        {
            Threading = ThreadingState.SingleThreaded;
        }

        public Mat_BiotLimp(IReader reader) : base(reader) { Threading = ThreadingState.SingleThreaded; }

        protected override void AddInputs(InputAdder pManager)
        {
            pManager.AddNumber("Thickness", "D", "Layer thickness in millimetres.", Access.Item).Set(25);
            pManager.AddNumber("Density", "Rho", "Solid or frame density in kg/m³.", Access.Item).Set(100);
            pManager.AddNumber("Young's Modulus", "E", "Young's modulus in Pa.", Access.Item).Set(1000000);
            pManager.AddNumber("Poisson's Ratio", "Nu", "Poisson's ratio, greater than -1 and less than 0.5.", Access.Item).Set(0.3);
            pManager.AddNumber("Tortuosity", "T", "Tortuosity, at least 1.", Access.Item).Set(1.05);
            pManager.AddNumber("Flow Resistivity", "Sigma", "Flow resistivity in Pa s/m².", Access.Item).Set(10000);
            pManager.AddNumber("Porosity", "Phi", "Open porosity as a fraction, greater than 0 and less than 1.", Access.Item).Set(0.98);
            pManager.AddNumber("Viscous Characteristic Length", "Lambda", "Viscous characteristic length in metres; retained in the layer definition.", Access.Item).Set(0.0001);
            pManager.AddNumber("Thermal Permeability", "K0", "Thermal permeability in m²; retained in the layer definition.", Access.Item).Set(0.00000001);
            pManager.AddText("Name", "N", "Material name retained in the layer definition.", Access.Item).Set("BiotPorousAbsorber_Limp");
            pManager.AddNumber("Embodied Carbon", "EC", "Embodied carbon coefficient in kgCO2e/m³; metadata only.", Access.Item).Set(0);
        }

        protected override void AddOutputs(OutputAdder pManager)
        {
            pManager.AddGeneric("Layer", "L", "Pachyderm ABS_Layer. Merge layers in exposed-face-to-backing order and connect to TMM.", Access.Item);
        }

        protected override void Process(IDataAccess DA)
        {
            double depth = 0;
            double rho = 0;
            double E = 0;
            double nu = 0;
            double tortuosity = 0;
            double resistivity = 0;
            double porosity = 0;
            double viscousLength = 0;
            double thermalPermeability = 0;
            string name = "BiotPorousAbsorber_Limp";
            double carbon = 0;
            if (!DA.GetItem(0, out depth)) return;
            if (!DA.GetItem(1, out rho)) return;
            if (!DA.GetItem(2, out E)) return;
            if (!DA.GetItem(3, out nu)) return;
            if (!DA.GetItem(4, out tortuosity)) return;
            if (!DA.GetItem(5, out resistivity)) return;
            if (!DA.GetItem(6, out porosity)) return;
            if (!DA.GetItem(7, out viscousLength)) return;
            if (!DA.GetItem(8, out thermalPermeability)) return;
            if (!DA.GetItem(9, out name) || !DA.GetItem(10, out carbon)) return;
            if (!(depth > 0) || double.IsInfinity(depth))
            {
                throw new ArgumentException("Thickness must be finite and positive.");
            }
            if (!(rho > 0) || double.IsInfinity(rho) || !(E > 0) || double.IsInfinity(E) || !(nu > -1 && nu < 0.5))
            {
                throw new ArgumentException("Density and modulus must be finite and positive; -1 < Poisson's ratio < 0.5.");
            }
            if (!(tortuosity >= 1) || double.IsInfinity(tortuosity) || !(resistivity > 0) || double.IsInfinity(resistivity) || !(porosity > 0 && porosity < 1) || !(viscousLength > 0) || double.IsInfinity(viscousLength) || !(thermalPermeability > 0) || double.IsInfinity(thermalPermeability))
            {
                throw new ArgumentException("Biot parameters must be finite and positive, with tortuosity >= 1 and 0 < porosity < 1.");
            }
            if (double.IsNaN(carbon) || double.IsInfinity(carbon))
            {
                throw new ArgumentException("Embodied carbon must be finite.");
            }
            ABS_Layer layer = ABS_Layer.CreateBiot(false, depth / 1000, rho, E, nu,
                tortuosity, resistivity, porosity, viscousLength, thermalPermeability, carbon, name);
            DA.SetItem(0, layer);
        }

        protected override IIcon IconInternal
        {
            get
            {
                using (var stream = typeof(Mat_BiotLimp).Assembly.GetManifestResourceStream("PachydermGH2.Resources.Medium Properties.png"))
                    return stream == null ? null : new PixelIcon(new Eto.Drawing.Bitmap(stream));
            }
        }
    }
}