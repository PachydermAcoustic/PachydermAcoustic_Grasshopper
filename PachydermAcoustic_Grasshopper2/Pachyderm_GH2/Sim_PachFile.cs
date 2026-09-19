using System.Collections.Generic;
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
using GrasshopperIO;
using Grasshopper2.Data;

namespace PachydermGH
{
    [IoId("69C6C89A-AC64-48BC-83DF-9B694E2EA7AB")]
    public class Sim_PachFile : Component
    {
        /// <summary>
        /// Each implementation of Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public Sim_PachFile()
            : base(new Nomen("Pachyderm File",
                "Obtains result from saved Pachyderm file.",
                "Acoustics", "Computation"))
        {
            Threading = Grasshopper2.Components.ThreadingState.SingleThreaded;
        }

        public Sim_PachFile(IReader reader) : base(reader) { Threading = Grasshopper2.Components.ThreadingState.SingleThreaded; }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void AddInputs(InputAdder inputs)
        {
            inputs.AddText("Path", "P", "Path to Pachyderm file. .Pac1 gives D, IS and RT types. .Pachm gives all data in RT (Mapping Receiver Bank) type.", Access.Item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void AddOutputs(OutputAdder outputs)
        {
            outputs.AddGeneric("Direct Sound data", "DS", "The pachyderm direct sound data", Access.Tree);
            outputs.AddGeneric("Image Source data", "IS", "The pachyderm image source data", Access.Tree);
            outputs.AddGeneric("Ray Tracing data", "RT", "The pachyderm ray tracing data", Access.Tree);
        }
        
        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="access">The access object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void Process(IDataAccess access)
        {
            string p = "";
            access.GetItem<string>(0, out p);
            string extension = System.IO.Path.GetExtension(p);
            
            if (extension.ToLower() == ".pac1")
            {
                Pachyderm_Acoustic.Direct_Sound[] D = new Pachyderm_Acoustic.Direct_Sound[0];
                Pachyderm_Acoustic.ImageSourceData[] IS = new Pachyderm_Acoustic.ImageSourceData[0];
                Pachyderm_Acoustic.Environment.Receiver_Bank[] RT = new Pachyderm_Acoustic.Environment.Receiver_Bank[0];
                Pachyderm_Acoustic.Utilities.FileIO.Read_Pac1(p, ref D, ref IS, ref RT);

                for (int i = 0; i < RT.Length; i++) RT[i].HasFilter();
                
                ComponentSupport.SetTree(access, 0, Garden.TreeFromList(D));
                ComponentSupport.SetTree(access, 1, Garden.TreeFromList(IS));
                ComponentSupport.SetTree(access, 2, Garden.TreeFromList(RT));
            }
            else if (extension.ToLower() == ".pachm")
            {
                Pachyderm_Acoustic.PachMapReceiver[] PMR = new Pachyderm_Acoustic.PachMapReceiver[0];
                Pachyderm_Acoustic.Utilities.FileIO.Read_pachm(p, ref PMR);
                ComponentSupport.SetTree(access, 0, Garden.TreeFromList(Array.Empty<Pachyderm_Acoustic.Direct_Sound>()));
                ComponentSupport.SetTree(access, 1, Garden.TreeFromList(Array.Empty<Pachyderm_Acoustic.ImageSourceData>()));

                for (int i = 0; i < PMR.Length; i++) PMR[i].HasFilter();

                ComponentSupport.SetTree(access, 2, Garden.TreeFromList(PMR));
            }
            else
            {
                throw new Exception("File extension not recognized...");
            }
        }
    }
}
