using System.Linq;
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
using Pachyderm_Acoustic;
using Pachyderm_Acoustic.Environment;
using Rhino.Geometry;
using Grasshopper2.Data;
using System.Collections.Generic;

namespace PachydermGH
{
    [IoId("01759E33-9D87-42D2-AA87-48A9A086E4C9")]
    public class Trafficsource_Component : Component
    {

        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public Trafficsource_Component()
            : base(new Nomen("FWHA Traffic Line Source",
                "Line/Curve source defined similarly to the FHWA Traffic Noise Model",
                "Acoustics", "Model"))
        {
            Threading = Grasshopper2.Components.ThreadingState.SingleThreaded;
        }

        public Trafficsource_Component(IReader reader) : base(reader) { Threading = Grasshopper2.Components.ThreadingState.SingleThreaded; }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void AddInputs(InputAdder inputs)
        {
            inputs.AddCurve("Curve", "C", "A curve defining a street or the movement of traffic.", Access.Item);
            inputs.AddInteger("pavement", "P", "Index indicating road type: Average_DGAC_PCC = 0, DGAC_Asphalt = 1, PCC_Concrete = 2, OGAC_OpenGradedAsphalt = 3", Access.Item);
            inputs.AddInteger("Speed", "S", "Speed of traffic in kilometers per hour", Access.Item);
            inputs.AddInteger("Automobiles", "A", "Nubmer of automobiles per hour", Access.Item);
            inputs.AddInteger("Medium Trucks", "MT", "Number of medium trucks per hour", Access.Item);
            inputs.AddInteger("Heavy Trucks", "HT", "Number of heavy trucks per hour", Access.Item);
            inputs.AddInteger("Buses", "B", "Number of buses per hour", Access.Item);
            inputs.AddInteger("Motorcyles", "M", "Number of motorcycles per hour", Access.Item);
            inputs.AddBoolean("Full Throttle?", "TH", "Full throttle generally occurs on highway on-ramps. Is traffic pedal to the metal?", Access.Item, Requirement.MustExist);
            inputs.AddNumber("Samples per Meter", "S_M", "The number of point sources per meter that will be used to approximate the line source. 4 is the default for Pachyderm, but other numbers may achieve similar results with less time, depending on the model.", new Grasshopper2.UI.UiNumber(0,4), Access.Item, Requirement.MustExist);

            //Grasshopper.Kernel.Parameters.Param_Integer paramp = (inputs[1] as Grasshopper.Kernel.Parameters.Param_Integer);
            //if (paramp != null) paramp.SetPersistentData(0);
            //Grasshopper.Kernel.Parameters.Param_Integer param = (inputs[2] as Grasshopper.Kernel.Parameters.Param_Integer);
            //if (param != null) param.SetPersistentData(100);
            //Grasshopper.Kernel.Parameters.Param_Integer param1 = (inputs[3] as Grasshopper.Kernel.Parameters.Param_Integer);
            //if (param1 != null) param1.SetPersistentData(10000);
            //Grasshopper.Kernel.Parameters.Param_Integer param2 = (inputs[4] as Grasshopper.Kernel.Parameters.Param_Integer);
            //if (param2 != null) param2.SetPersistentData(100);
            //Grasshopper.Kernel.Parameters.Param_Integer param3 = (inputs[5] as Grasshopper.Kernel.Parameters.Param_Integer);
            //if (param3 != null) param3.SetPersistentData(700);
            //Grasshopper.Kernel.Parameters.Param_Integer param4 = (inputs[6] as Grasshopper.Kernel.Parameters.Param_Integer);
            //if (param4 != null) param4.SetPersistentData(200);
            //Grasshopper.Kernel.Parameters.Param_Integer param5 = (inputs[7] as Grasshopper.Kernel.Parameters.Param_Integer);
            //if (param5 != null) param5.SetPersistentData(100);
            //Grasshopper.Kernel.Parameters.Param_Boolean param6 = (inputs[8] as Grasshopper.Kernel.Parameters.Param_Boolean);
            //if (param5 != null) param6.SetPersistentData(false);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void AddOutputs(OutputAdder outputs)
        {
            outputs.AddGeneric("Source", "Src", "Line Source object.", Access.Item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void Process(IDataAccess access)
        {
            Curve Origin;
            access.GetItem<Curve>(0, out Origin);
            int pavement = 0;
            int speed = 0, auto = 0, mt = 0, ht = 0, b = 0, m = 0;
            Boolean throttle = false;
            double el_m = 0;
            access.GetItem<int>(1, out pavement);
            access.GetItem<int>(2, out speed);
            access.GetItem<int>(3, out auto);
            access.GetItem<int>(4, out mt);
            access.GetItem<int>(5, out ht);
            access.GetItem<int>(6, out b);
            access.GetItem<int>(7, out m);
            access.GetItem<Boolean>(8, out throttle);
            access.GetItem<double>(9, out el_m);

            if (el_m <= 0 || speed <= 0 || pavement < 0 || pavement > 3 || auto < 0 || mt < 0 || ht < 0 || b < 0 || m < 0) throw new ArgumentException("Provide positive sampling density and speed, pavement 0–3, and nonnegative traffic counts.");
            double[] SWL = Pachyderm_Acoustic.Utilities.StandardConstructions.FHWA_TNM10_SoundPower(speed, pavement, auto, mt, ht, b, m, throttle);

            Rhino.Geometry.Point3d[] pts = Origin.DivideEquidistant(1d / el_m);
            if (pts == null || pts.Length == 0) pts = new Point3d[1] { (Origin as Curve).PointAtNormalizedLength(0.5) };
            Hare.Geometry.Point[] Samples = new Hare.Geometry.Point[pts.Length];

            for (int i = 0; i < pts.Length; i++)
            {
                Samples[i] = Pachyderm_Acoustic.Utilities.RCPachTools.RPttoHPt(pts[i]);
            }
            var S = new Pachyderm_Acoustic.Environment.LineSource(Samples, (Origin as Curve).GetLength(), Pachyderm_Acoustic.Utilities.PachTools.EncodeSourcePower(SWL), el_m, 0, false);

            access.SetItem(0, S);
        }
        protected override IIcon IconInternal
        {
            get
            {
                var assembly = typeof(SPLETC).Assembly;
                var resourceName = "PachydermGH2.Resources.LoudSpeaker.png";

                using (var stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream == null) return null;

                    // FromStream reads serialized .ghicon data, not PNG/BMP images.
                    // The PixelIcon retains the bitmap for its cached lifetime.
                    return new Grasshopper2.UI.Icon.PixelIcon(new Eto.Drawing.Bitmap(stream));
                }
            }
        }
        //protected override IIcon IconInternal => new Grasshopper2.UI.Icon.PixelIcon("Loudspeaker");
    }
}
