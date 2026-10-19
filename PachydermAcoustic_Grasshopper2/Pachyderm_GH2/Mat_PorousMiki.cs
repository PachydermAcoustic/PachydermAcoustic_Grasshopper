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
    [IoId("7192df1f-c8da-4828-a76b-238ca8ed7473")]
    public class Mat_PorousMiki : Component
    {
        public Mat_PorousMiki()
            : base(new Nomen("Miki Layer", "Porous layer using Pachyderm's Miki model.", "Acoustics", "Materials"))
        {
            Threading = ThreadingState.SingleThreaded;
        }

        public Mat_PorousMiki(IReader reader) : base(reader) { Threading = ThreadingState.SingleThreaded; }

        protected override void AddInputs(InputAdder pManager)
        {
            pManager.AddNumber("Thickness", "D", "Layer thickness in millimetres.", Access.Item).Set(25);
            pManager.AddNumber("Flow Resistivity", "Sigma", "Flow resistivity in Pa s/m².", Access.Item).Set(10000);
            pManager.AddText("Name", "N", "Material name retained in the layer definition.", Access.Item).Set("PorousM");
            pManager.AddNumber("Embodied Carbon", "EC", "Embodied carbon coefficient in kgCO2e/m³; metadata only.", Access.Item).Set(0);
        }

        protected override void AddOutputs(OutputAdder pManager)
        {
            pManager.AddGeneric("Layer", "L", "Pachyderm ABS_Layer. Merge layers in exposed-face-to-backing order and connect to TMM.", Access.Item);
        }

        protected override void Process(IDataAccess DA)
        {
            double depth = 0;
            double sigma = 0;
            string name = "PorousM";
            double carbon = 0;
            if (!DA.GetItem(0, out depth)) return;
            if (!DA.GetItem(1, out sigma)) return;
            if (!DA.GetItem(2, out name) || !DA.GetItem(3, out carbon)) return;
            if (!(depth > 0) || double.IsInfinity(depth))
            {
                throw new ArgumentException("Thickness must be finite and positive.");
            }
            if (!(sigma > 0) || double.IsInfinity(sigma))
            {
                throw new ArgumentException("Flow resistivity must be finite and positive.");
            }
            if (double.IsNaN(carbon) || double.IsInfinity(carbon))
            {
                throw new ArgumentException("Embodied carbon must be finite.");
            }
            ABS_Layer layer = ABS_Layer.Porous_Miki(depth / 1000, sigma, carbon);
            layer.Material_Name = name;
            DA.SetItem(0, layer);
        }

        protected override IIcon IconInternal
        {
            get
            {
                using (var stream = typeof(Mat_PorousMiki).Assembly.GetManifestResourceStream("PachydermGH2.Resources.Porous Layer.png"))
                    return stream == null ? null : new PixelIcon(new Eto.Drawing.Bitmap(stream));
            }
        }
    }
}