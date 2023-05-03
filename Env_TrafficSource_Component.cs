//'Pachyderm-Acoustic: Geometrical Acoustics for Rhinoceros (GPL) by Arthur van der Harten 
//' 
//'This file is part of Pachyderm-Acoustic. 
//' 
//'Copyright (c) 2008-2019, Arthur van der Harten 
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
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using Pachyderm_Acoustic;

namespace PachydermGH
{
    public class Trafficsource_Component : GH_Component
    {
        Pachyderm_Acoustic.Environment.LineSource S;

        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public Trafficsource_Component()
            : base("FWHA Traffic Line Source", "Traffic Source",
                "Line/Curve source defined similarly to the FHWA Traffic Noise Model",
                "Acoustics", "Model")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("Curve", "C", "A curve defining a street or the movement of traffic.", GH_ParamAccess.item);
            pManager.AddIntegerParameter("pavement", "P", "Index indicating road type: Average_DGAC_PCC = 0, DGAC_Asphalt = 1, PCC_Concrete = 2, OGAC_OpenGradedAsphalt = 3", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Speed", "S", "Speed of traffic in kilometers per hour", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Automobiles", "A", "Nubmer of automobiles per hour", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Medium Trucks", "MT", "Number of medium trucks per hour", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Heavy Trucks", "HT", "Number of heavy trucks per hour", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Buses", "B", "Number of buses per hour", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Motorcyles", "M", "Number of motorcycles per hour", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Full Throttle?", "TH", "Full throttle generally occurs on highway on-ramps. Is traffic pedal to the metal?", GH_ParamAccess.item);
            pManager.AddNumberParameter("Samples per Meter", "S_M", "The number of point sources per meter that will be used to approximate the line source. 4 is the default for Pachyderm, but other numbers may achieve similar results with less time, depending on the model.", GH_ParamAccess.item);

            Grasshopper.Kernel.Parameters.Param_Integer paramp = (pManager[1] as Grasshopper.Kernel.Parameters.Param_Integer);
            if (paramp != null) paramp.SetPersistentData(0);
            Grasshopper.Kernel.Parameters.Param_Integer param = (pManager[2] as Grasshopper.Kernel.Parameters.Param_Integer);
            if (param != null) param.SetPersistentData(100);
            Grasshopper.Kernel.Parameters.Param_Integer param1 = (pManager[3] as Grasshopper.Kernel.Parameters.Param_Integer);
            if (param1 != null) param1.SetPersistentData(10000);
            Grasshopper.Kernel.Parameters.Param_Integer param2 = (pManager[4] as Grasshopper.Kernel.Parameters.Param_Integer);
            if (param2 != null) param2.SetPersistentData(100);
            Grasshopper.Kernel.Parameters.Param_Integer param3 = (pManager[5] as Grasshopper.Kernel.Parameters.Param_Integer);
            if (param3 != null) param3.SetPersistentData(700);
            Grasshopper.Kernel.Parameters.Param_Integer param4 = (pManager[6] as Grasshopper.Kernel.Parameters.Param_Integer);
            if (param4 != null) param4.SetPersistentData(200);
            Grasshopper.Kernel.Parameters.Param_Integer param5 = (pManager[7] as Grasshopper.Kernel.Parameters.Param_Integer);
            if (param5 != null) param5.SetPersistentData(100);
            Grasshopper.Kernel.Parameters.Param_Boolean param6 = (pManager[8] as Grasshopper.Kernel.Parameters.Param_Boolean);
            if (param5 != null) param6.SetPersistentData(false);
            Grasshopper.Kernel.Parameters.Param_Integer param7 = (pManager[9] as Grasshopper.Kernel.Parameters.Param_Integer);
            if (param5 != null) param6.SetPersistentData(4);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Source", "Src", "Line Source object.", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_Curve Origin = new GH_Curve();
            DA.GetData<GH_Curve>(0, ref Origin);
            int pavement = 0;
            int speed = 0, auto = 0, mt = 0, ht = 0, b = 0, m = 0;
            Boolean throttle = false;
            double el_m = 0;
            DA.GetData<int>(1, ref pavement);
            DA.GetData<int>(2, ref speed);
            DA.GetData<int>(3, ref auto);
            DA.GetData<int>(4, ref mt);
            DA.GetData<int>(5, ref ht);
            DA.GetData<int>(6, ref b);
            DA.GetData<int>(7, ref m);
            DA.GetData<Boolean>(8, ref throttle);
            DA.GetData<double>(9, ref el_m);

            double[] SWL = Pachyderm_Acoustic.Utilities.StandardConstructions.FHWA_TNM10_SoundPower(speed, pavement, auto, mt, ht, b, m, throttle);

            Rhino.Geometry.Point3d[] pts = Origin.Value.DivideEquidistant(1d / el_m);
            if (pts == null || pts.Length == 0) pts = new Point3d[1] { (Origin.Value as Curve).PointAtNormalizedLength(0.5) };
            Hare.Geometry.Point[] Samples = new Hare.Geometry.Point[pts.Length];

            for (int i = 0; i < pts.Length; i++)
            {
                Samples[i] = Pachyderm_Acoustic.Utilities.RC_PachTools.RPttoHPt(pts[i]);
            }
            S = new Pachyderm_Acoustic.Environment.LineSource(Samples, (Origin.Value as Curve).GetLength(), Pachyderm_Acoustic.Utilities.PachTools.EncodeSourcePower(SWL), el_m, 0, false);

            DA.SetData("Source", S);
        }

        public override void DrawViewportWires(IGH_PreviewArgs args)
        {
            //if (Balloon != null) args.Display.DrawMeshShaded(M, new Rhino.Display.DisplayMaterial(System.Drawing.Color.Blue));
            //base.DrawViewportWires(args);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                System.Drawing.Bitmap b = Properties.Resources.LoudSpeaker;
                b.MakeTransparent(System.Drawing.Color.White);
                return b;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{01759E33-9D87-42D2-AA87-48A9A086E4C9}"); }
        }
    }
}