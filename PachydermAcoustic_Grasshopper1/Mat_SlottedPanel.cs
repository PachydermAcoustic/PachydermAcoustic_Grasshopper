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
    public class Mat_SlottedPanel : GH_Component
    {
        public Mat_SlottedPanel()
            : base("Slotted Panel Layer", "Slot", "Slotted layer using Pachyderm's panel impedance model.", "Acoustics", "Materials")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Thickness", "D", "Layer thickness in millimetres.", GH_ParamAccess.item, 25);
            pManager.AddNumberParameter("Pitch", "P", "Hole or slot pitch in millimetres.", GH_ParamAccess.item, 10);
            pManager.AddNumberParameter("Width", "W", "Hole diameter, square side or slot width in millimetres; less than pitch.", GH_ParamAccess.item, 1);
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
            double pitch = 0;
            double width = 0;
            string name = Name;
            double carbon = 0;
            if (!DA.GetData(0, ref depth)) return;
            if (!DA.GetData(1, ref pitch)) return;
            if (!DA.GetData(2, ref width)) return;
            if (!DA.GetData(3, ref name) || !DA.GetData(4, ref carbon)) return;
            if (!(depth > 0) || double.IsInfinity(depth))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Thickness must be finite and positive.");
                return;
            }
            if (!(pitch > 0) || double.IsInfinity(pitch) || !(width > 0) || double.IsInfinity(width) || width >= pitch)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Pitch and width must be finite and positive, with width less than pitch.");
                return;
            }
            if (double.IsNaN(carbon) || double.IsInfinity(carbon))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Embodied carbon must be finite.");
                return;
            }
            ABS_Layer layer = new ABS_Layer(ABS_Layer.LayerType.Slotted_Panel, depth / 1000, pitch / 1000,
                width / 1000, 0, 0, carbon, name);
            DA.SetData(0, layer);
        }

        protected override System.Drawing.Bitmap Icon
        {
            get { return Properties.Resources.Medium_Properties; }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("17bc9a96-95a0-495c-a15b-77cc7aad56a7"); }
        }
    }
}