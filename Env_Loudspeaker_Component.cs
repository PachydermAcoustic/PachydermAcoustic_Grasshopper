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
    public class Loudspeaker_Component : GH_Component
    {
        Mesh M;
        string[] CLF_Contents;
        Speaker_Balloon Balloon;
        string Sensitivity;
        string Max;
        Pachyderm_Acoustic.Environment.DirectionalSource S;
        Vector3d CurrentD;
        Point3d CurrentO;
        double CurrentR;

        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public Loudspeaker_Component()
            : base("Loudspeaker", "Speaker",
                "Common Loudspeaker Format",
                "Acoustics", "Model")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Origin", "Or", "Acoustic Center of the Speaker", GH_ParamAccess.item);
            pManager.AddVectorParameter("Direction", "D", "Aiming direction for the loudspeaker", GH_ParamAccess.item);
            pManager.AddNumberParameter("Rotation", "R", "Rotation of Speaker in degrees", GH_ParamAccess.item);
            pManager.AddNumberParameter("Power", "P", "0 for Sensitivity, 1 for Max, anything else for Flat spectrum.", GH_ParamAccess.list);
            pManager.AddNumberParameter("Delay", "D", "Signal delay", GH_ParamAccess.item);
            
            Grasshopper.Kernel.Parameters.Param_Number param = (pManager[1] as Grasshopper.Kernel.Parameters.Param_Number);
            if (param != null) param.SetPersistentData(new List<GH_Number> { new GH_Number(120), new GH_Number(120), new GH_Number(120), new GH_Number(120), new GH_Number(120), new GH_Number(120), new GH_Number(120), new GH_Number(120) });
            Grasshopper.Kernel.Parameters.Param_Vector param1 = (pManager[1] as Grasshopper.Kernel.Parameters.Param_Vector);
            if (param1 != null) param1.SetPersistentData(new GH_Vector(new Vector3d(1,0,0)));
            Grasshopper.Kernel.Parameters.Param_Number param2 = (pManager[2] as Grasshopper.Kernel.Parameters.Param_Number);
            if (param2 != null) param2.SetPersistentData(0);
            Grasshopper.Kernel.Parameters.Param_Number param3 = (pManager[2] as Grasshopper.Kernel.Parameters.Param_Number);
            if (param3 != null) param3.SetPersistentData(0);
            Grasshopper.Kernel.Parameters.Param_Number param4 = (pManager[3] as Grasshopper.Kernel.Parameters.Param_Number);
            if (param4 != null) param4.SetPersistentData(new List<GH_Number> { new GH_Number(0), new GH_Number(0), new GH_Number(0), new GH_Number(0), new GH_Number(0), new GH_Number(0), new GH_Number(0), new GH_Number(0) });
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Source", "Src", "Loudspeaker source object, with directivity data.", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Point3d Origin = new Point3d();
            double delay = 0, rot = 0;
            List<double> Level = new List<double>();
            Vector3d V = default(Vector3d);
            DA.GetData<Point3d>(0, ref Origin);
            DA.GetData<Vector3d>(1, ref V);
            DA.GetData<double>(2, ref rot);
            DA.GetDataList<double>(3, Level);
            DA.GetData<double>(4, ref delay);

            if (V == null || V.Length == 0) throw new Exception("Provide a vector indicating the direction of the speaker.");

            if (!CurrentD.Equals(V) || !CurrentO.Equals(Origin) || CurrentR != rot)
            {
                CurrentD = V;
                CurrentO = Origin;
                CurrentR = rot;

                if (S == null)
                {
                    CLF_Contents = CLF_Read.SecureAccess.Read();
                    this.Description = CLF_Contents[0];
                    Sensitivity = CLF_Contents[2];
                    Max = CLF_Contents[3];
                    string[] Code = new string[] { CLF_Contents[4], CLF_Contents[5], CLF_Contents[6], CLF_Contents[7], CLF_Contents[8], CLF_Contents[9], CLF_Contents[10], CLF_Contents[11] };
                    Balloon = new Speaker_Balloon(Code, Sensitivity, int.Parse(CLF_Contents[1]), new Hare.Geometry.Point(0, 0, 0));
                }

                 string[] B = CLF_Contents[12].Split(';');

                double[] SWL;
                if (Level.Count == 1)
                {
                    if (Level[0] == 0)
                    {
                        SWL = Pachyderm_Acoustic.Utilities.PachTools.DecodeSourcePower(Sensitivity);
                    }
                    if (Level[0] == 1)
                    {
                        SWL = Pachyderm_Acoustic.Utilities.PachTools.DecodeSourcePower(Max);
                    }
                    else
                    { 
                        SWL = new double[] { Level[0], Level[0], Level[0], Level[0], Level[0], Level[0], Level[0], Level[0] };
                    }
                }
                else if (Level.Count == 8)
                {
                    SWL = Level.ToArray();
                }
                else
                {
                    throw new Exception("Power Levels are coded incorrectly. Use 0 for Sensitivity, 1 for Max level, any number for a flat level, or specify by octave band.");
                }

                Balloon.Update_Position(new Hare.Geometry.Point(Origin.X, Origin.Y, Origin.Z));
                Balloon.CurrentAlt = (float)(Math.Asin(V.Z / Math.Sqrt(V.X * V.X + V.Y * V.Y + V.Z * V.Z)) * 180 / Math.PI);
                Balloon.CurrentAzi = (float)(-Math.Atan2(V.X, V.Y) * 180 / Math.PI);
                Balloon.CurrentAxi = (float)rot;
                Balloon.Update_Aim();

                S = new Pachyderm_Acoustic.Environment.DirectionalSource(Balloon, SWL, new Hare.Geometry.Point(Origin.X, Origin.Y, Origin.Z), new int[] { int.Parse(B[0]), int.Parse(B[1]) }, 0);
                M = Pachyderm_Acoustic.Utilities.RC_PachTools.Hare_to_RhinoMesh(Balloon.m_DisplayMesh, false);
                M.Flip(true, true, true);
            }

            DA.SetData("Source", S);
        }

        public override void DrawViewportWires(IGH_PreviewArgs args)
        {
            if (Balloon != null) args.Display.DrawMeshShaded(M, new Rhino.Display.DisplayMaterial(System.Drawing.Color.Blue));
            base.DrawViewportWires(args);
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
            get { return new Guid("{392c2c48-3c11-486e-999f-5e24045a3dfe}"); }
        }
    }
}