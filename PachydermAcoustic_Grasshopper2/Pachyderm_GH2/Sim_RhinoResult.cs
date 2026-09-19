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
using GrasshopperIO;
using Grasshopper2.Data;

namespace PachydermGH
{
    [IoId("900E0EF8-E7B5-4D36-B574-3A9BC98A7550")]
    public class Sim_RhinoResult : Component
    {
        /// <summary>
        /// Each implementation of Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public Sim_RhinoResult()
            : base(new Nomen("Pach_Rhino Result",
                "Obtains result from Rhinoceros implementation of Pachyderm, provided you have run a simulation there...",
                "Acoustics", "Computation"))
        {
        }

        public Sim_RhinoResult(IReader reader) : base(reader) { }

        interface_selection I = interface_selection.Pach_Hybrid_Method;

        enum interface_selection
        {
            Pach_Hybrid_Method, Pach_Mapping_Method, Pach_Numeric_TimeDomain
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void AddInputs(InputAdder inputs)
        {
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void AddOutputs(OutputAdder outputs)
        {
            switch (I)
            {
                case interface_selection.Pach_Hybrid_Method:
                    outputs.AddGeneric("Direct Sound data", "DS", "The pachyderm direct sound data", Access.Tree);
                    outputs.AddGeneric("Image Source data", "IS", "The pachyderm image source data", Access.Tree);
                    outputs.AddGeneric("Ray Tracing data", "RT", "The pachyderm ray tracing data", Access.Tree);
                    break;
                case interface_selection.Pach_Mapping_Method:
                    outputs.AddGeneric("Ray Tracing data", "RT", "The pachyderm ray tracing data", Access.Tree);
                    break;
                case interface_selection.Pach_Numeric_TimeDomain:
                    outputs.AddGeneric("Numeric Time Domain data", "NTD", "Recorded signal at receivers.", Access.Item);
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

        //private void Hybrid_Click(Object sender, EventArgs e)
        //{
        //    I = interface_selection.Pach_Hybrid_Method;
        //    //Params.Clear();
        //    //Grasshopper.Kernel.Parameters.Param_GenericObject D = new Grasshopper.Kernel.Parameters.Param_GenericObject();
        //    //D.Name = "Direct Sound data";
        //    //D.NickName = "DS";
        //    //D.Description = "The pachyderm direct sound data";
        //    //D.Access = Access.Tree;
        //    //Grasshopper.Kernel.Parameters.Param_GenericObject IS = new Grasshopper.Kernel.Parameters.Param_GenericObject();
        //    //IS.Name = "Image Source data";
        //    //IS.NickName = "IS";
        //    //IS.Description = "The pachyderm image source data";
        //    //IS.Access = Access.Tree;
        //    //Grasshopper.Kernel.Parameters.Param_GenericObject RT = new Grasshopper.Kernel.Parameters.Param_GenericObject();
        //    //RT.Name = "Ray Tracing data";
        //    //RT.NickName = "RT";
        //    //RT.Description = "The pachyderm ray tracing data";
        //    //RT.Access = Access.Tree;

        //    this.ClearData();
        //    //Params.Output.Add(D);
        //    //Params.Output.Add(IS);
        //    //Params.Output.Add(RT);
        //    //this.Collectdata();
        //    ExpireSolution(true);
        //    //this.Computedata();
        //}

        //private void Mapping_Click(Object sender, EventArgs e)
        //{
        //    I = interface_selection.Pach_Mapping_Method;
        //    //Params.Clear();
        //    //Grasshopper.Kernel.Parameters.Param_GenericObject RT = new Grasshopper.Kernel.Parameters.Param_GenericObject();
        //    //RT.Name = "Ray Tracing data";
        //    //RT.NickName = "RT";
        //    //RT.Description = "The pachyderm ray tracing data";
        //    //RT.Access = Access.Tree;


        //    this.ClearData();
        //    //Params.Output.Add(RT);
        //    //this.CreateAttributes();
        //    //Collectdata();
        //    ExpireSolution(true);
        //    //this.Computedata();
        //}

        //private void NTD_Click(Object sender, EventArgs e)
        //{
        //    I = interface_selection.Pach_Numeric_TimeDomain;
        //    //ExpireSolution(true);
        //    Params.Clear();
        //    for (int i = 0; i < Params.Output.Count; i++) Params.UnregisterParameter(Params.Output[i]);
        //    Grasshopper.Kernel.Parameters.Param_GenericObject PN = new Grasshopper.Kernel.Parameters.Param_GenericObject();
        //    PN.Name = "Recording";
        //    PN.NickName = "R";
        //    PN.Description = "A signal object representing the recording at each receiver in the Rhinoceros FVM Visualization simulation.";
        //    PN.Access = Access.Tree;
        //    Params.RegisterOutputParam(PN);
        //    Params.OnParametersChanged();
        //    this.ExpireSolution(true);
        //    //ExpireSolution(true);
        //}

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="access">The access object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void Process(IDataAccess access)
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
                access.SetTree(0, Garden.TreeFromList(D));
                access.SetTree(1, Garden.TreeFromList(IS));
                access.SetTree(2, Garden.TreeFromList(RT));
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
                access.SetTree(0, Garden.TreeFromList(D));
                access.SetTree(1, Garden.TreeFromList(IS));
                access.SetTree(2, Garden.TreeFromList(RT));
            }
            else if (I == interface_selection.Pach_Numeric_TimeDomain && Pachyderm_Acoustic.UI.PachTDNumericControl.Instance.FDTD != null && Pachyderm_Acoustic.UI.PachTDNumericControl.Instance.FDTD.Mic != null)
            {
                double[][] Rec = Pachyderm_Acoustic.UI.PachTDNumericControl.Instance.FDTD.Mic.Recordings_Current();
                if (Rec == null || Rec.Length < 1) throw new Exception("Recordings not found. Are you sure you have receivers, and that you have run your FDTD simulation?");
                double FS = Pachyderm_Acoustic.UI.PachTDNumericControl.Instance.FDTD.SampleFrequency;
                System.Collections.Generic.List<Audio_Signal> AS = new System.Collections.Generic.List<Audio_Signal>();
                for (int i = 0; i < Rec.Length; i++) AS.Add(new Audio_Signal(Rec[i], (int)FS));
                access.SetTree(0, Garden.TreeFromList(AS));
            }
        }
    }
}