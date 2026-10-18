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
using Grasshopper.Kernel;
using Pachyderm_Acoustic.AbsorptionModels;

namespace PachydermGH
{
    public class Mat_BiotRigid : GH_Component
    {
        public Mat_BiotRigid()
            : base("Biot Rigid Layer", "Biot R", "Porous layer using Pachyderm's rigid Biot model.", "Acoustics", "Materials")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Thickness", "D", "Layer thickness in millimetres.", GH_ParamAccess.item, 25);
            pManager.AddNumberParameter("Density", "Rho", "Solid or frame density in kg/m³.", GH_ParamAccess.item, 100);
            pManager.AddNumberParameter("Young's Modulus", "E", "Young's modulus in Pa.", GH_ParamAccess.item, 1000000);
            pManager.AddNumberParameter("Poisson's Ratio", "Nu", "Poisson's ratio, greater than -1 and less than 0.5.", GH_ParamAccess.item, 0.3);
            pManager.AddNumberParameter("Tortuosity", "T", "Tortuosity, at least 1.", GH_ParamAccess.item, 1.05);
            pManager.AddNumberParameter("Flow Resistivity", "Sigma", "Flow resistivity in Pa s/m².", GH_ParamAccess.item, 10000);
            pManager.AddNumberParameter("Porosity", "Phi", "Open porosity as a fraction, greater than 0 and less than 1.", GH_ParamAccess.item, 0.98);
            pManager.AddNumberParameter("Viscous Characteristic Length", "Lambda", "Viscous characteristic length in metres; retained in the layer definition.", GH_ParamAccess.item, 0.0001);
            pManager.AddNumberParameter("Thermal Permeability", "K0", "Thermal permeability in m²; retained in the layer definition.", GH_ParamAccess.item, 0.00000001);
            pManager.AddTextParameter("Name", "N", "Material name retained in the layer definition.", GH_ParamAccess.item, Name);
            pManager.AddNumberParameter("Embodied Carbon", "EC", "Embodied carbon coefficient in kgCO2e/m³; metadata only.", GH_ParamAccess.item, 0);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Layer", "L", "Pachyderm ABS_Layer. Merge layers in exposed-face-to-backing order and connect to TMM.", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
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
            string name = Name;
            double carbon = 0;
            if (!DA.GetData(0, ref depth)) return;
            if (!DA.GetData(1, ref rho)) return;
            if (!DA.GetData(2, ref E)) return;
            if (!DA.GetData(3, ref nu)) return;
            if (!DA.GetData(4, ref tortuosity)) return;
            if (!DA.GetData(5, ref resistivity)) return;
            if (!DA.GetData(6, ref porosity)) return;
            if (!DA.GetData(7, ref viscousLength)) return;
            if (!DA.GetData(8, ref thermalPermeability)) return;
            if (!DA.GetData(9, ref name) || !DA.GetData(10, ref carbon)) return;
            if (!(depth > 0) || double.IsInfinity(depth))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Thickness must be finite and positive.");
                return;
            }
            if (!(rho > 0) || double.IsInfinity(rho) || !(E > 0) || double.IsInfinity(E) || !(nu > -1 && nu < 0.5))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Density and modulus must be finite and positive; -1 < Poisson's ratio < 0.5.");
                return;
            }
            if (!(tortuosity >= 1) || double.IsInfinity(tortuosity) || !(resistivity > 0) || double.IsInfinity(resistivity) || !(porosity > 0 && porosity < 1) || !(viscousLength > 0) || double.IsInfinity(viscousLength) || !(thermalPermeability > 0) || double.IsInfinity(thermalPermeability))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Biot parameters must be finite and positive, with tortuosity >= 1 and 0 < porosity < 1.");
                return;
            }
            if (double.IsNaN(carbon) || double.IsInfinity(carbon))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Embodied carbon must be finite.");
                return;
            }
            ABS_Layer layer = ABS_Layer.CreateBiot(true, depth / 1000, rho, E, nu,
                tortuosity, resistivity, porosity, viscousLength, thermalPermeability, carbon, name);
            DA.SetData(0, layer);
        }

        protected override System.Drawing.Bitmap Icon
        {
            get { return Properties.Resources.Medium_Properties; }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("c0b979da-e3fa-40e4-9d07-3eb6a423030b"); }
        }
    }
}