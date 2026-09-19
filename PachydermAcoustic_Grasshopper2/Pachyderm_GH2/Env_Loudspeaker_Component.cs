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
using Rhino.UI;
using Eto.Drawing;
using Eto.Forms;
using Rhino.Display;
using Grasshopper2.Display;

namespace PachydermGH
{
    [IoId("392c2c48-3c11-486e-999f-5e24045a3dfe")]
    public class Loudspeaker_Component : Component
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
            : base(new Nomen("Loudspeaker",
                "Common Loudspeaker Format",
                "Acoustics", "Model"))
        {
            Threading = Grasshopper2.Components.ThreadingState.UiSingleThreaded;
        }

        public Loudspeaker_Component(IReader reader) : base(reader) { Threading = Grasshopper2.Components.ThreadingState.UiSingleThreaded; if(reader.HasItem("CLF")) CLF_Contents=reader.StringArray("CLF"); }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        public override void AppendToInputPanel(Grasshopper2.UI.InputPanel.InputPanel panel)
        {
            panel.AddCheck("Select or replace CLF loudspeaker", false, choose => {
                if (!choose) return;
                var contents=CLF_Read.SecureAccess.ReadAny(Rhino.UI.RhinoEtoApp.MainWindow);
                if(contents==null || contents.Length<13) return;
                CLF_Contents=contents; Expire(); Document?.Solution.Start();
            });
            base.AppendToInputPanel(panel);
        }
        public override void Store(IWriter writer) { base.Store(writer); if(CLF_Contents!=null) writer.StringArray("CLF",CLF_Contents); }
        protected override void AddInputs(InputAdder inputs)
        {
            List<double> SWL_Default = new List<double> { 120, 120, 120, 120, 120, 120, 120, 120 };

            inputs.AddPoint("Origin", "Or", "Acoustic Center of the Speaker", Access.Item);
            inputs.AddVector("Direction", "D", "Aiming direction for the loudspeaker", Access.Item, Requirement.MustExist);
            inputs.AddNumber("Rotation", "R", "Rotation of Speaker in degrees", Access.Item, 0).Set(0.0);
            inputs.AddNumber("Power", "P", "0 for Sensitivity, 1 for Max, anything else for Flat spectrum.", Access.Tree, 0).Set(new double[] { 0.0 });
            inputs.AddNumber("Delay", "D", "Signal delay", Access.Item).Set(0.0);
            
            //Grasshopper.Kernel.Parameters.Param_Number param = (inputs[1] as Grasshopper.Kernel.Parameters.Param_Number);
            //if (param != null) param.SetPersistentData(new List<GH_Number> { new GH_Number(120), new GH_Number(120), new GH_Number(120), new GH_Number(120), new GH_Number(120), new GH_Number(120), new GH_Number(120), new GH_Number(120) });
            //Grasshopper.Kernel.Parameters.Param_Vector param1 = (inputs[1] as Grasshopper.Kernel.Parameters.Param_Vector);
            //if (param1 != null) param1.SetPersistentData(new GH_Vector(new Vector3d(1,0,0)));
            //Grasshopper.Kernel.Parameters.Param_Number param2 = (inputs[2] as Grasshopper.Kernel.Parameters.Param_Number);
            //if (param2 != null) param2.SetPersistentData(0);
            //Grasshopper.Kernel.Parameters.Param_Number param3 = (inputs[2] as Grasshopper.Kernel.Parameters.Param_Number);
            //if (param3 != null) param3.SetPersistentData(0);
            //Grasshopper.Kernel.Parameters.Param_Number param4 = (inputs[3] as Grasshopper.Kernel.Parameters.Param_Number);
            //if (param4 != null) param4.SetPersistentData(new List<GH_Number> { new GH_Number(0), new GH_Number(0), new GH_Number(0), new GH_Number(0), new GH_Number(0), new GH_Number(0), new GH_Number(0), new GH_Number(0) });
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void AddOutputs(OutputAdder outputs)
        {
            outputs.AddGeneric("Source", "Src", "Loudspeaker source object, with directivity data.", Access.Item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void Process(IDataAccess access)
        {
            Point3d Origin = new Point3d();
            double delay = 0, rot = 0;
            Tree<double> Level_T;
            Vector3d V = default(Vector3d);
            access.GetItem<Point3d>(0, out Origin);
            access.GetItem<Vector3d>(1, out V);
            access.GetItem<double>(2, out rot);
            access.GetTree<double>(3, out Level_T);
            access.GetItem<double>(4, out delay);

            if (V.Length == 0) throw new Exception("Provide a vector indicating the direction of the speaker.");
            List<double> Level = new List<double>(Level_T.AllItems);

            //if (!CurrentD.Equals(V) || !CurrentO.Equals(Origin) || CurrentR != rot)
            //{
            CurrentD = V;
                CurrentO = Origin;
                CurrentR = rot;

                if (CLF_Contents == null)
                {
                    throw new ArgumentException("Select a CLF loudspeaker using the component input panel.");
                }
                {
                    //this.Description = CLF_Contents[0];
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
                    else if (Level[0] == 1)
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

                S = new Pachyderm_Acoustic.Environment.DirectionalSource(Balloon, SWL, new Hare.Geometry.Point(Origin.X, Origin.Y, Origin.Z), new int[] { int.Parse(B[0]), int.Parse(B[1]) }, 0, false);
                M = Pachyderm_Acoustic.Utilities.RCPachTools.HaretoRhinoMesh(Balloon.m_DisplayMesh, false);
                M.Flip(true, true, true);
            //}

            ComponentSupport.SetDelay(S, delay);
            access.SetItem(0, S);
        }

        public override void DisplayWires(DisplayPipeline pipeline, Guises guises, ref BoundingBox extents)
        {
            if (Balloon != null) pipeline.DrawMeshShaded(M, new Rhino.Display.DisplayMaterial(System.Drawing.Color.Blue));
            base.DisplayWires(pipeline, guises, ref extents);
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
