﻿//'Pachyderm-Acoustic: Geometrical Acoustics for Rhinoceros (GPL)   
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
using System.Collections.Generic;
using Grasshopper.Kernel;
using Pachyderm_Acoustic.Environment;
using Pachyderm_Acoustic.UI;
using Rhino.Geometry;

namespace PachydermGH
{
    public class VisualizeRays : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public VisualizeRays()
            : base("Visualize Pachyderm Rays", "PachVisRays",
                "Casts specular rays on the geometry specified, and returns the ray paths as polylines.",
                "Acoustics", "Visualization")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Room Model", "Room", "The Pachyderm Room Model Reference", GH_ParamAccess.item);
            pManager.AddGenericParameter("Source", "Src", "Sound Source Objects...", GH_ParamAccess.list);
            pManager.AddVectorParameter("Ray Directions", "Dir", "The initial direction of each ray (normalized or unnormalized).", GH_ParamAccess.list);
            pManager.AddBrepParameter("Terminating Surface", "_X", "Terminating surface", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Number of Bounces", "Bs", "Number of times the ray will reflect before terminating...", GH_ParamAccess.item);
            pManager[3].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("Ray Curves", "RC", "The rays, returned as polylines", GH_ParamAccess.list);
            pManager.AddPointParameter("End Points", "X.", "The point at which each ray terminated...", GH_ParamAccess.list);
            pManager.AddNumberParameter("Arrival Time Delay", "t", "The time delay of each ray, relative to it's termination point...", GH_ParamAccess.list);
            pManager.AddNumberParameter("Itensity", "I", "The remaining power of each ray at its termination point. Includes deduction for spherical propagation, assigned absorption and scattering coefficients, and air absorption...", GH_ParamAccess.tree);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Pachyderm_Acoustic.Environment.Scene S = null;
            DA.GetData<Pachyderm_Acoustic.Environment.Scene>(0, ref S);
            List<Pachyderm_Acoustic.Environment.Source> Src = new List<Pachyderm_Acoustic.Environment.Source>();
            DA.GetDataList<Pachyderm_Acoustic.Environment.Source>(1, Src);
            List<Vector3d> Dir = new List<Vector3d>();
            DA.GetDataList<Vector3d>(2, Dir);
            List<Brep> terminus = new List<Brep>();
            DA.GetDataList<Brep>(3, terminus);
            int bounces = 0;
            DA.GetData<int>(4, ref bounces);

            Random Rnd = new Random((int)System.DateTime.UtcNow.Ticks);

            List<Polyline> rays = new List<Polyline>();
            List<Point3d> Ends = new List<Point3d>();
            List<double> times = new List<double>();
            Grasshopper.DataTree<double> power = new Grasshopper.DataTree<double>();

            int sno = -1;

            foreach (Pachyderm_Acoustic.Environment.Source Pt in Src)
            {
                sno++;
                foreach (Vector3d vector in Dir)
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
                            poly.Add(ray.x, ray.y, ray.z);
                            foreach (Brep br in terminus)
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
                    if (poly.Count > 0)
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

                        for (int oct = 0; oct < 8; oct++) power.Add(ray.Energy[oct], new Grasshopper.Kernel.Data.GH_Path(new int[] { sno, rays.Count - 1, oct }));
                    }
                }

                DA.SetDataList(0, rays);
                DA.SetDataList(1, Ends);
                DA.SetDataList(2, times);
                DA.SetDataTree(3, power);
            }
        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                System.Drawing.Bitmap b = Properties.Resources.Ray_Tracing;
                b.MakeTransparent(System.Drawing.Color.White);
                return b;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{DC140949-0D92-4A31-BDB2-C2641338D778}"); }
        }
    }
}