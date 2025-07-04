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
using Grasshopper.Kernel;

namespace PachydermGH
{
    public class Sim_RhinoResult : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public Sim_RhinoResult()
            : base("Pachyderm for Rhino Result", "PachR_Result",
                "Obtains result from Rhinoceros implementation of Pachyderm, provided you have run a simulation there...",
                "Acoustics", "Computation")
        {
        }

        interface_selection I = interface_selection.Pach_Hybrid_Method;

        enum interface_selection
        {
            Pach_Hybrid_Method, Pach_Mapping_Method, Pach_Numeric_TimeDomain
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            //pManager.AddGenericParameter("Which Interface?", "I", "Integer 0 for Hybrid Results, Integer 1 for Mapping...", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            switch (I)
            {
                case interface_selection.Pach_Hybrid_Method:
                    pManager.AddGenericParameter("Direct Sound Data", "DS", "The pachyderm direct sound data", GH_ParamAccess.list);
                    pManager.AddGenericParameter("Image Source Data", "IS", "The pachyderm image source data", GH_ParamAccess.list);
                    pManager.AddGenericParameter("Ray Tracing Data", "RT", "The pachyderm ray tracing data", GH_ParamAccess.list);
                    break;
                case interface_selection.Pach_Mapping_Method:
                    pManager.AddGenericParameter("Ray Tracing Data", "RT", "The pachyderm ray tracing data", GH_ParamAccess.list);
                    break;
                case interface_selection.Pach_Numeric_TimeDomain:
                    pManager.AddGenericParameter("Numeric Time Domain Data", "NTD", "Recorded signal at receivers.", GH_ParamAccess.item);
                    break;
            }
        }
        
        //public override bool AppendMenuItems(ToolStripDropDown menu)
        //{
        //    Menu_AppendObjectNameEx(menu);
        //    Menu_AppendEnableItem(menu);
        //    Menu_AppendItem(menu, "Obtain data from Pach_Hybrid_Method", Hybrid_Click);
        //    Menu_AppendItem(menu, "Obtain data from Pach_Mapping_Method", Mapping_Click);
        //    Menu_AppendItem(menu, "Obtain data from Numeric_TimeDomain_Method", NTD_Click);
        //    return true;
        //}

        private void Hybrid_Click(Object sender, EventArgs e)
        {
            I = interface_selection.Pach_Hybrid_Method;
            //Params.Clear();
            //Grasshopper.Kernel.Parameters.Param_GenericObject D = new Grasshopper.Kernel.Parameters.Param_GenericObject();
            //D.Name = "Direct Sound Data";
            //D.NickName = "DS";
            //D.Description = "The pachyderm direct sound data";
            //D.Access = GH_ParamAccess.list;
            //Grasshopper.Kernel.Parameters.Param_GenericObject IS = new Grasshopper.Kernel.Parameters.Param_GenericObject();
            //IS.Name = "Image Source Data";
            //IS.NickName = "IS";
            //IS.Description = "The pachyderm image source data";
            //IS.Access = GH_ParamAccess.list;
            //Grasshopper.Kernel.Parameters.Param_GenericObject RT = new Grasshopper.Kernel.Parameters.Param_GenericObject();
            //RT.Name = "Ray Tracing Data";
            //RT.NickName = "RT";
            //RT.Description = "The pachyderm ray tracing data";
            //RT.Access = GH_ParamAccess.list;

            this.ClearData();
            //Params.Output.Add(D);
            //Params.Output.Add(IS);
            //Params.Output.Add(RT);
            //this.CollectData();
            ExpireSolution(true);
            //this.ComputeData();
        }

        private void Mapping_Click(Object sender, EventArgs e)
        {
            I = interface_selection.Pach_Mapping_Method;
            //Params.Clear();
            //Grasshopper.Kernel.Parameters.Param_GenericObject RT = new Grasshopper.Kernel.Parameters.Param_GenericObject();
            //RT.Name = "Ray Tracing Data";
            //RT.NickName = "RT";
            //RT.Description = "The pachyderm ray tracing data";
            //RT.Access = GH_ParamAccess.list;


            this.ClearData();
            //Params.Output.Add(RT);
            //this.CreateAttributes();
            //CollectData();
            ExpireSolution(true);
            //this.ComputeData();
        }

        private void NTD_Click(Object sender, EventArgs e)
        {
            I = interface_selection.Pach_Numeric_TimeDomain;
            //ExpireSolution(true);
            Params.Clear();
            for (int i = 0; i < Params.Output.Count; i++) Params.UnregisterParameter(Params.Output[i]);
            Grasshopper.Kernel.Parameters.Param_GenericObject PN = new Grasshopper.Kernel.Parameters.Param_GenericObject();
            PN.Name = "Recording";
            PN.NickName = "R";
            PN.Description = "A signal object representing the recording at each receiver in the Rhinoceros FVM Visualization simulation.";
            PN.Access = GH_ParamAccess.list;
            Params.RegisterOutputParam(PN);
            Params.OnParametersChanged();
            this.ExpireSolution(true);
            //ExpireSolution(true);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Pachyderm_Acoustic.Direct_Sound[] D = new Pachyderm_Acoustic.Direct_Sound[0];
            Pachyderm_Acoustic.ImageSourceData[] IS = new Pachyderm_Acoustic.ImageSourceData[0];
            Pachyderm_Acoustic.Environment.Receiver_Bank[] RT = new Pachyderm_Acoustic.Environment.Receiver_Bank[0];

            if (I == interface_selection.Pach_Hybrid_Method && Pachyderm_Acoustic.UI.PachHybridControl.Instance.AuralisationReady())
            {
                Hare.Geometry.Point[] SRC = new Hare.Geometry.Point[0];
                Hare.Geometry.Point[] REC = new Hare.Geometry.Point[0];
                Pachyderm_Acoustic.UI.PachHybridControl.Instance.GetSims(ref SRC, ref REC, ref D, ref IS, ref RT);
                if (RT.Length == 0) RT = new Pachyderm_Acoustic.Environment.Receiver_Bank[D.Length];
                if (IS.Length == 0) IS = new Pachyderm_Acoustic.ImageSourceData[D.Length];
                DA.SetDataList(0, D);
                DA.SetDataList(1, IS);
                DA.SetDataList(2, RT);
            }
            else if (I == interface_selection.Pach_Mapping_Method && Pachyderm_Acoustic.UI.Pach_Mapping_Control.Instance.Simulations_Ready())
            {
                Pachyderm_Acoustic.PachMapReceiver[] PMR = new Pachyderm_Acoustic.PachMapReceiver[0];
                Pachyderm_Acoustic.UI.Pach_Mapping_Control.Instance.GetSims(ref PMR);
                D = new Pachyderm_Acoustic.Direct_Sound[PMR.Length];
                IS = new Pachyderm_Acoustic.ImageSourceData[PMR.Length];
                RT = new Pachyderm_Acoustic.Environment.Receiver_Bank[PMR.Length];
                for (int i = 0; i < PMR.Length; i++)
                {
                    D[i] = null;
                    IS[i] = null;
                    RT[i] = PMR[i];
                }
                DA.SetDataList(0, D);
                DA.SetDataList(1, IS);
                DA.SetDataList(2, RT);
            }
            else if (I == interface_selection.Pach_Numeric_TimeDomain && Pachyderm_Acoustic.UI.PachTDNumericControl.Instance.FDTD != null && Pachyderm_Acoustic.UI.PachTDNumericControl.Instance.FDTD.Mic != null)
            {
                double[][] Rec = Pachyderm_Acoustic.UI.PachTDNumericControl.Instance.FDTD.Mic.Recordings_Current();
                if (Rec == null || Rec.Length < 1) throw new Exception("Recordings not found. Are you sure you have receivers, and that you have run your FDTD simulation?");
                double FS = Pachyderm_Acoustic.UI.PachTDNumericControl.Instance.FDTD.SampleFrequency;
                System.Collections.Generic.List<Audio_Signal> AS = new System.Collections.Generic.List<Audio_Signal>();
                for (int i = 0; i < Rec.Length; i++) AS.Add(new Audio_Signal(Rec[i], (int)FS));
                DA.SetDataList(0, AS);
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
                //return Properties.Resources.;
                return null;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{900E0EF8-E7B5-4D36-B574-3A9BC98A7550}"); }
        }
    }
}