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
using System.Linq;

namespace PachydermGH
{
    [IoId("C196641B-3929-4904-8925-AACB6CEAA39E")]
    public class QuickDirect : Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public QuickDirect()
            : base(new Nomen("Pach_Direct",
                "Maps direct sound to a mesh, including phase and air attenuation.",
                "Acoustics", "Computation"))
        {
            Threading = Grasshopper2.Components.ThreadingState.SingleThreaded;
        }

        public QuickDirect(IReader reader) : base(reader) { Threading = Grasshopper2.Components.ThreadingState.SingleThreaded; }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void AddInputs(InputAdder inputs)
        {
            inputs.AddGeneric("Source Object", "S", "The Pachyderm sound source object.", Access.Tree);
            inputs.AddGeneric("Medium Properties", "P", "The Pachyderm medium properties object", Access.Item);
            inputs.AddMesh("Receivers", "R", "The points or mesh to use for mapping", Access.Item);
            inputs.AddNumber("Delays in ms", "D", "The number of milliseconds each source is delayed. Input one integer (ms) per source object.", Access.Tree);
            inputs.AddInterval("Frequency Scope", "Oct", "An interval of the first and last octave to calculate (0 = 62.5 Hz, 1 = 125 HZ., ..., 7 = 8000 Hz.", Access.Item).Set(new Rhino.Geometry.Interval(0,7));

            //Grasshopper.Kernel.Parameters.Param_GenericObject param = (inputs[1] as Grasshopper.Kernel.Parameters.Param_GenericObject);
            //if (param != null) param.SetPersistentdata(new Pachyderm_Acoustic.Environment.Uniform_Medium(0, 100000, 20+273.15, 50, false));

            //Grasshopper.Kernel.Parameters.Param_Interval param2 = (inputs[4] as Grasshopper.Kernel.Parameters.Param_Interval);
            //if (param2 != null) param2.SetPersistentdata(new Grasshopper.Kernel.Types.GH_Interval(new Interval(0,7)));
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void AddOutputs(OutputAdder outputs)
        {
            outputs.AddNumber("Pressure Field", "P", "Pressure field calculation for each point on the mesh.", Access.Tree);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="access">The access object is used to retrieve from inputs and store in outputs.</param>
        protected override void Process(IDataAccess access)
        {
            Tree<Pachyderm_Acoustic.Environment.Source> Src;
            Mesh M = new Mesh();
            Tree<double> delays;
            Interval F = new Interval();
            Pachyderm_Acoustic.Environment.Uniform_Medium MP = new Pachyderm_Acoustic.Environment.Uniform_Medium(0, 101325, 293.15, 50, false);
            access.GetTree<Pachyderm_Acoustic.Environment.Source>(0, out Src);
            access.GetItem<Pachyderm_Acoustic.Environment.Uniform_Medium>(1, out MP);
            access.GetItem<Mesh>(2, out M);
            access.GetTree<double>(3, out delays);
            access.GetItem<Interval>(4, out F);

            ComponentSupport.Octaves(F, out _, out _);
            if (M.Vertices.Count == 0 || delays.ItemCount != Src.ItemCount) throw new ArgumentException("Provide a nonempty mesh and one delay per source.");
            int No_of_octaves = (int)F.Max - (int)F.Min + 1;
            if (No_of_octaves < 0) return;

            Point3d[] Pts = M.Vertices.ToPoint3dArray();
            double[] P_Sum = new double[Pts.Length];

            Random Rnd = new Pachyderm_Acoustic.Utilities.PachTools.RandomNumberGenerator();

            double c = MP.Sound_Speed(new Hare.Geometry.Point(Pts[0].X, Pts[0].Y, Pts[0].Z));

            double[] lambaccess2pi = new double[8]{ 2 * Math.PI * 62.5 / c, 2 * Math.PI * 125 / c, 2 * Math.PI * 250 / c, 2 * Math.PI * 500 / c, 2 * Math.PI * 1000 / c, 2 * Math.PI * 2000 / c, 2 * Math.PI * 4000 / c, 2 * Math.PI * 8000 / c };

            System.Threading.Tasks.Parallel.For(0, Pts.Length, i =>
            {
                double[] P_Real = new double[No_of_octaves], P_Imag = new double[No_of_octaves];
                for (int S_id = 0; S_id < Src.ItemCount; S_id++)
                {
                    Vector3d V = Pts[i] - new Point3d(Src.Items[S_id].Origin.x, Src.Items[S_id].Origin.y, Src.Items[S_id].Origin.z);
                    double Length = V.Length;
                    if (Length <= 0) throw new ArgumentException("A receiver coincides with a source.");
                    int id;
                    id = i;
                    double delay = delays.Items[S_id] * 0.001 * c;
                    V.Unitize();
                    double[] Power = Src.Items[S_id].DirPower(0, id, new Hare.Geometry.Vector(V.X, V.Y, V.Z));
                    for (int oct = 0; oct < No_of_octaves; oct++)
                    {
                        double I = Power[oct+(int)F.Min] * Math.Pow(10, -MP.Attenuation_Coef(0)[oct+(int)F.Min] * Length) / (4 * Math.PI * Length * Length);
                        double real, imag;
                        Pachyderm_Acoustic.Utilities.Numerics.ExpComplex(0, lambaccess2pi[oct+(int)F.Min] * (Length + delay), out real, out imag);
                        Hare.Geometry.Point pt = new Hare.Geometry.Point(Pts[i].X, Pts[i].Y, Pts[i].Z);
                        P_Real[oct] += Math.Sqrt(I * MP.Rho_C(pt)) * real;
                        P_Imag[oct] += Math.Sqrt(I * MP.Rho_C(pt)) * imag;
                    }
                }

                for (int oct = 0; oct < No_of_octaves; oct++) 
                {
                    double P_Mag = Pachyderm_Acoustic.Utilities.Numerics.Abs(P_Real[oct], P_Imag[oct]);
                    P_Sum[i] += P_Mag * P_Mag;
                }

                P_Sum[i] = Math.Sqrt(P_Sum[i]);
            });

            ComponentSupport.SetTree(access, 0, Garden.TreeFromList<double>(P_Sum));
        }
        protected override IIcon IconInternal
        {
            get
            {
                var assembly = typeof(SPLETC).Assembly;
                var resourceName = "PachydermGH2.Resources.Patch-Direct.png";

                using (var stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream == null) return null;

                    // FromStream reads serialized .ghicon data, not PNG/BMP images.
                    // The PixelIcon retains the bitmap for its cached lifetime.
                    return new Grasshopper2.UI.Icon.PixelIcon(new Eto.Drawing.Bitmap(stream));
                }
            }
        }

        //protected override IIcon IconInternal => new Grasshopper2.UI.Icon.PixelIcon("Patch_Direct");
    }
}
