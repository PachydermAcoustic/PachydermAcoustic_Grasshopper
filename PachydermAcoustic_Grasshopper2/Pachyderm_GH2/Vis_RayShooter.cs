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
    [IoId("DC140949-0D92-4A31-BDB2-C2641338D778")]
    public class VisualizeRays : Component
    {
        /// <summary>
        /// Each implementation of Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public VisualizeRays()
            : base(new Nomen("Visualize Pachyderm Rays",
                "Casts specular rays on the geometry specified, and returns the ray paths as polylines.",
                "Acoustics", "Visualization"))
        {
            Threading = Grasshopper2.Components.ThreadingState.SingleThreaded;
        }

        public VisualizeRays(IReader reader) : base(reader) { Threading = Grasshopper2.Components.ThreadingState.SingleThreaded; }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void AddInputs(InputAdder inputs)
        {
            inputs.AddGeneric("Room Model", "Room", "The Pachyderm Room Model Reference", Access.Item);
            inputs.AddGeneric("Source", "Src", "Sound Source Objects...", Access.Tree);
            inputs.AddVector("Ray Directions", "Dir", "The initial direction of each ray (normalized or unnormalized).", Access.Tree);
            inputs.AddSurface("Terminating Surface", "_X", "Terminating surface", Access.Tree);
            inputs.AddInteger("Number of Bounces", "Bs", "Number of times the ray will reflect before terminating...", new Grasshopper2.UI.UiInteger(2), Access.Item);
            inputs[3].Requirement = Requirement.MayBeMissing;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void AddOutputs(OutputAdder outputs)
        {
            outputs.AddCurve("Ray Curves", "RC", "The rays, returned as polylines", Access.Tree);
            outputs.AddPoint("End Points", "X.", "The point at which each ray terminated...", Access.Tree);
            outputs.AddNumber("Arrival Time Delay", "t", "The time delay of each ray, relative to it's termination point...", Access.Tree);
            outputs.AddNumber("Itensity", "I", "The remaining power of each ray at its termination point. Includes deduction for spherical propagation, assigned absorption and scattering coefficients, and air absorption...", Access.Tree);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="access">The access object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void Process(IDataAccess access)
        {
            Pachyderm_Acoustic.Environment.Scene S = null;
            access.GetItem<Pachyderm_Acoustic.Environment.Scene>(0, out S);
            Tree<Pachyderm_Acoustic.Environment.Source> Src;
            access.GetTree<Pachyderm_Acoustic.Environment.Source>(1, out Src);
            Tree<Vector3d> Dir;
            access.GetTree<Vector3d>(2, out Dir);
            Tree<Brep> terminus;
            access.GetTree<Brep>(3, out terminus);
            terminus = terminus ?? Garden.TreeFromList(Array.Empty<Brep>());
            int bounces = 0;
            access.GetItem<int>(4, out bounces);

            Random Rnd = new Random((int)System.DateTime.UtcNow.Ticks);

            List<Polyline> rays = new List<Polyline>();
            List<Point3d> Ends = new List<Point3d>();
            List<double> times = new List<double>();
            List<double[]> power = new List<double[]>();

            int sno = -1;

            foreach (Pachyderm_Acoustic.Environment.Source Pt in Src.AllItems)
            {
                sno++;
                foreach (Vector3d vector in Dir.AllItems)
                {
                    Hare.Geometry.Point Startpt = Pt.Origin;
                    Point3d RPT = new Point3d(Startpt.x, Startpt.y, Startpt.z);
                    Hare.Geometry.Vector vct = new Hare.Geometry.Vector(vector.X, vector.Y, vector.Z);
                    vct.Normalize();
                    Polyline poly = new Polyline();
                    poly.Add(RPT);
                    bool terminate = false;
                    
                    BroadRay ray = new BroadRay(Startpt.x, Startpt.y, Startpt.z, vct.dx, vct.dy, vct.dz, Rnd.Next(), 0, Pt.DirPower(0, Rnd.Next(), vct), 0, 0);

                    for (int i = 0; i < bounces; i++)
                    {
                        double u, v;
                        int poly_id;
                        List<int> code;
                        List<Hare.Geometry.Point> X;
                        List<double> t;

                        if (S.shoot(ray, out u, out v, out poly_id, out X, out t, out code)) 
                        {
                            ray.x = X[0].x;
                            ray.y = X[0].y;
                            ray.z = X[0].z;
                            Hare.Geometry.Vector N = S.Normal(poly_id, u, v);
                            double dot2 = Hare.Geometry.Hare_math.Dot(ray.dx, ray.dy, ray.dz, N.dx, N.dy, N.dz) * 2;
                            ray.dx -= N.dx * dot2;
                            ray.dy -= N.dy * dot2;
                            ray.dz -= N.dz * dot2;
                            ray.Surf_ID = poly_id;
                            RPT = new Point3d(ray.x, ray.y, ray.z);
                            poly.Add(RPT);
                            foreach (Brep br in terminus.AllItems)
                            {
                                ComponentIndex c;
                                Point3d p;
                                double s, time;
                                Vector3d Norm;

                                terminate |= br.ClosestPoint(RPT, out p, out c, out s, out time, 0.01, out Norm);
                            }
                            double cos_theta;
                            if (i < bounces - 1)
                            {
                                S.Absorb(ref ray, out cos_theta, u, v);
                                double[] scat = S.ScatteringValue[ray.Surf_ID].Coefficient();
                                for (int oct = 0; oct < 8; oct++) ray.Energy[oct] *= (1 - scat[oct]);
                            }
                        }else{break;}

                        if (terminate) break;
                    }
                    if (poly.Count > 1)
                    {
                        rays.Add(poly);
                        Ends.Add(RPT);
                        double dist = poly.Length;
                        //double dir = (Pt.Origin - Startpt).Length();
                        times.Add(dist/ S.Sound_speed(Startpt));
                        double propmod = 4 * Math.PI * dist * dist;

                        for(int oct = 0; oct < 8; oct++)
                        {
                            ray.Energy[oct] *= Math.Pow(10, -.1 * S.Attenuation(0)[oct] * dist);
                            ray.Energy[oct] /= propmod;
                        }

                        power.Add(ray.Energy);
                    }
                }

            }
                ComponentSupport.SetTree(access, 0, Garden.TreeFromList(rays));
                ComponentSupport.SetTree(access, 1, Garden.TreeFromList(Ends));
                ComponentSupport.SetTree(access, 2, Garden.TreeFromList(times));
                ComponentSupport.SetTree(access, 3, Garden.TreeFromArrays(power.ToArray()));
        }
        protected override IIcon IconInternal
        {
            get
            {
                var assembly = typeof(SPLETC).Assembly;
                var resourceName = "PachydermGH2.Resources.Ray Tracing.png";

                using (var stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream == null) return null;

                    // FromStream reads serialized .ghicon data, not PNG/BMP images.
                    // The PixelIcon retains the bitmap for its cached lifetime.
                    return new Grasshopper2.UI.Icon.PixelIcon(new Eto.Drawing.Bitmap(stream));
                }
            }
        }

        //protected override IIcon IconInternal => new Grasshopper2.UI.Icon.PixelIcon("Ray_Tracing");
    }
}
