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
using Grasshopper2.Components;
using Grasshopper2.Parameters;
using Grasshopper2.UI;
using Grasshopper2.UI.Icon;
using GrasshopperIO;
using Pachyderm_Acoustic.AbsorptionModels;

namespace PachydermGH
{
    [IoId("b24a8c01-9e87-47ae-a2a8-b33683fa9422")]
    public class Mat_SolidPlate : Component
    {
        public Mat_SolidPlate()
            : base(new Nomen("Solid Plate Layer", "Elastic solid plate layer.", "Acoustics", "Materials"))
        {
            Threading = ThreadingState.SingleThreaded;
        }

        public Mat_SolidPlate(IReader reader) : base(reader) { Threading = ThreadingState.SingleThreaded; }

        protected override void AddInputs(InputAdder pManager)
        {
            pManager.AddNumber("Thickness", "D", "Layer thickness in millimetres.", Access.Item).Set(25);
            pManager.AddNumber("Density", "Rho", "Solid or frame density in kg/m³.", Access.Item).Set(100);
            pManager.AddNumber("Young's Modulus", "E", "Young's modulus in Pa.", Access.Item).Set(1000000);
            pManager.AddNumber("Poisson's Ratio", "Nu", "Poisson's ratio, greater than -1 and less than 0.5.", Access.Item).Set(0.3);
            pManager.AddText("Name", "N", "Material name retained in the layer definition.", Access.Item).Set("SolidPlate");
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
            string name = "SolidPlate";
            double carbon = 0;
            if (!DA.GetItem(0, out depth)) return;
            if (!DA.GetItem(1, out rho)) return;
            if (!DA.GetItem(2, out E)) return;
            if (!DA.GetItem(3, out nu)) return;
            if (!DA.GetItem(4, out name) || !DA.GetItem(5, out carbon)) return;
            if (!(depth > 0) || double.IsInfinity(depth))
            {
                throw new ArgumentException("Thickness must be finite and positive.");
            }
            if (!(rho > 0) || double.IsInfinity(rho) || !(E > 0) || double.IsInfinity(E) || !(nu > -1 && nu < 0.5))
            {
                throw new ArgumentException("Density and modulus must be finite and positive; -1 < Poisson's ratio < 0.5.");
            }
            if (double.IsNaN(carbon) || double.IsInfinity(carbon))
            {
                throw new ArgumentException("Embodied carbon must be finite.");
            }
            ABS_Layer layer = ABS_Layer.CreateSolid(depth / 1000, rho, E, nu, carbon, name);
            DA.SetItem(0, layer);
        }

        protected override IIcon IconInternal
        {
            get
            {
                using (var stream = typeof(Mat_SolidPlate).Assembly.GetManifestResourceStream("PachydermGH2.Resources.Solid Plate Layer.png"))
                    return stream == null ? null : new PixelIcon(new Eto.Drawing.Bitmap(stream));
            }
        }
    }
}