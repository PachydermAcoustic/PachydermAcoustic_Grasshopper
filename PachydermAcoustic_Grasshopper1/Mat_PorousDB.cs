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
    public class Mat_PorousDB : GH_Component
    {
        public Mat_PorousDB()
            : base("Delaney-Bazley Layer", "DB", "Porous layer using Pachyderm's Delaney-Bazley model.", "Acoustics", "Materials")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Thickness", "D", "Layer thickness in millimetres.", GH_ParamAccess.item, 25);
            pManager.AddNumberParameter("Flow Resistivity", "Sigma", "Flow resistivity in Pa s/m².", GH_ParamAccess.item, 10000);
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
            double sigma = 0;
            string name = Name;
            double carbon = 0;
            if (!DA.GetData(0, ref depth)) return;
            if (!DA.GetData(1, ref sigma)) return;
            if (!DA.GetData(2, ref name) || !DA.GetData(3, ref carbon)) return;
            if (!(depth > 0) || double.IsInfinity(depth))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Thickness must be finite and positive.");
                return;
            }
            if (!(sigma > 0) || double.IsInfinity(sigma))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Flow resistivity must be finite and positive.");
                return;
            }
            if (double.IsNaN(carbon) || double.IsInfinity(carbon))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Embodied carbon must be finite.");
                return;
            }
            ABS_Layer layer = ABS_Layer.Porous_DelaneyBazley(depth / 1000, sigma, carbon);
            layer.Material_Name = name;
            DA.SetData(0, layer);
        }

        protected override System.Drawing.Bitmap Icon
        {
            get { return Properties.Resources.Porous_Layer; }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("896b6bd3-31b3-49b8-a48a-19299275fa61"); }
        }
    }
}