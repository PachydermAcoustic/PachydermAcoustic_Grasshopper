//'Pachyderm-Acoustic: Geometrical Acoustics for Rhinoceros (GPL) by Arthur van der Harten 
//' 
//'This file is part of Pachyderm-Acoustic. 
//' 
//'Copyright (c) 2008-2025, Arthur van der Harten 
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
    public class Sim_PachFile : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public Sim_PachFile()
            : base("Pachyderm File", "Pach_File",
                "Obtains result from saved Pachyderm file.",
                "Acoustics", "Computation")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Path", "P", "Path to Pachyderm file. .Pac1 gives D, IS and RT types. .Pachm gives all data in RT (Mapping Receiver Bank) type.", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Direct Sound Data", "DS", "The pachyderm direct sound data", GH_ParamAccess.list);
            pManager.AddGenericParameter("Image Source Data", "IS", "The pachyderm image source data", GH_ParamAccess.list);
            pManager.AddGenericParameter("Ray Tracing Data", "RT", "The pachyderm ray tracing data", GH_ParamAccess.list);
        }
        
        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string p = "";
            DA.GetData<string>(0, ref p);
            string extension = System.IO.Path.GetExtension(p);
            
            if (extension.ToLower() == ".pac1")
            {
                Pachyderm_Acoustic.Direct_Sound[] D = new Pachyderm_Acoustic.Direct_Sound[0];
                Pachyderm_Acoustic.ImageSourceData[] IS = new Pachyderm_Acoustic.ImageSourceData[0];
                Pachyderm_Acoustic.Environment.Receiver_Bank[] RT = new Pachyderm_Acoustic.Environment.Receiver_Bank[0];
                Pachyderm_Acoustic.Utilities.FileIO.Read_Pac1(p, ref D, ref IS, ref RT);

                for (int i = 0; i < RT.Length; i++) RT[i].HasFilter();
                
                DA.SetDataList(0, D);
                DA.SetDataList(1, IS);
                DA.SetDataList(2, RT);
            }
            else if (extension.ToLower() == ".pachm")
            {
                Pachyderm_Acoustic.PachMapReceiver[] PMR = new Pachyderm_Acoustic.PachMapReceiver[0];
                Pachyderm_Acoustic.Utilities.FileIO.Read_pachm(p, ref PMR);
                DA.SetDataList(0, null);
                DA.SetDataList(1, null);

                for (int i = 0; i < PMR.Length; i++) PMR[i].HasFilter();

                DA.SetDataList(2, PMR);
            }
            else
            {
                throw new Exception("File extension not recognized...");
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
                // You can add image files to your project resources and access them like this:
                //return Resources.IconForThisComponent;
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
            get { return new Guid("{69C6C89A-AC64-48BC-83DF-9B694E2EA7AB}"); }
        }
    }
}