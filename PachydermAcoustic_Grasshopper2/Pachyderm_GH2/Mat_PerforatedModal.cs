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
    [IoId("f89f48f3-a3d4-4f9d-93bb-e716baaf454f")]
    public class Mat_PerforatedModal : Component
    {
        public Mat_PerforatedModal()
            : base(new Nomen("Perforated Modal Layer", "Perforated layer using Pachyderm's modal model.", "Acoustics", "Materials"))
        {
            Threading = ThreadingState.SingleThreaded;
        }

        public Mat_PerforatedModal(IReader reader) : base(reader) { Threading = ThreadingState.SingleThreaded; }

        protected override void AddInputs(InputAdder pManager)
        {
            pManager.AddNumber("Thickness", "D", "Layer thickness in millimetres.", Access.Item).Set(25);
            pManager.AddNumber("Pitch", "P", "Hole or slot pitch in millimetres.", Access.Item).Set(10);
            pManager.AddNumber("Width", "W", "Hole diameter, square side or slot width in millimetres; less than pitch.", Access.Item).Set(1);
            pManager.AddInteger("Shape", "S", "0 = circular holes; 1 = square holes.", Access.Item).Set(0);
            pManager.AddInteger("Grid", "G", "0 = square grid; 1 = hexagonal grid.", Access.Item).Set(0);
            pManager.AddText("Name", "N", "Material name retained in the layer definition.", Access.Item).Set("Perforated_Modal");
            pManager.AddNumber("Embodied Carbon", "EC", "Embodied carbon coefficient in kgCO2e/m³; metadata only.", Access.Item).Set(0);
        }

        protected override void AddOutputs(OutputAdder pManager)
        {
            pManager.AddGeneric("Layer", "L", "Pachyderm ABS_Layer. Merge layers in exposed-face-to-backing order and connect to TMM.", Access.Item);
        }

        protected override void Process(IDataAccess DA)
        {
            double depth = 0;
            double pitch = 0;
            double width = 0;
            int shape = 0;
            int grid = 0;
            string name = "Perforated_Modal";
            double carbon = 0;
            if (!DA.GetItem(0, out depth)) return;
            if (!DA.GetItem(1, out pitch)) return;
            if (!DA.GetItem(2, out width)) return;
            if (!DA.GetItem(3, out shape)) return;
            if (!DA.GetItem(4, out grid)) return;
            if (!DA.GetItem(5, out name) || !DA.GetItem(6, out carbon)) return;
            if (!(depth > 0) || double.IsInfinity(depth))
            {
                throw new ArgumentException("Thickness must be finite and positive.");
            }
            if (!(pitch > 0) || double.IsInfinity(pitch) || !(width > 0) || double.IsInfinity(width) || width >= pitch)
            {
                throw new ArgumentException("Pitch and width must be finite and positive, with width less than pitch.");
            }
            if (shape < 0 || shape > 1 || grid < 0 || grid > 1)
            {
                throw new ArgumentException("Shape and grid must each be 0 or 1.");
            }
            if (double.IsNaN(carbon) || double.IsInfinity(carbon))
            {
                throw new ArgumentException("Embodied carbon must be finite.");
            }
            ABS_Layer layer = new ABS_Layer(ABS_Layer.LayerType.Perforated_Modal, depth / 1000, pitch / 1000,
                width / 1000, 0, 0, carbon, name, (ABS_Layer.PerfShape)shape, (ABS_Layer.PerfGrid)grid);
            DA.SetItem(0, layer);
        }

        protected override IIcon IconInternal
        {
            get
            {
                using (var stream = typeof(Mat_PerforatedModal).Assembly.GetManifestResourceStream("PachydermGH2.Resources.Medium Properties.png"))
                    return stream == null ? null : new PixelIcon(new Eto.Drawing.Bitmap(stream));
            }
        }
    }
}