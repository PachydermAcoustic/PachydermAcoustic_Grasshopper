//'Pachyderm-Acoustic: Geometrical Acoustics for Rhinoceros (GPL) by Arthur van der Harten 
//' 
//'This file is part of Pachyderm-Acoustic. 
//' 
//'Copyright (c) 2008-2015, Arthur van der Harten 
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

namespace PachydermGH
{
    public class Signal2Chunks : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public Signal2Chunks()
            : base("SignalDivide", "Signal /",
                "Divides a signal into equal sized chunks",
                "Acoustics", "Audio")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Input Data", "Signal", "The data to divide...", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Chunksize", "Size", "The number of samples in chunks...", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Chunks", "Chs", "The divided signal", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            int chunksize = 0;
            object test = new object();
            Audio_Signal Buffer = new Audio_Signal();
            DA.GetData<Audio_Signal>(0, ref Buffer);            
            //DA.GetData<object>(0, ref test);
            DA.GetData<int>(1, ref chunksize);

            //if (test is Audio_Signal)
            //{
            //    Buffer = (test as Audio_Signal);
            //}
            //else throw new Exception("Invalid Signal input");

            int no_of_chunks = (int)Math.Ceiling((double)Buffer.Count / (double)chunksize);

            List<Audio_Signal> SL = new List<Audio_Signal>();

            for (int i = 0; i < no_of_chunks; i++)
            {
                float[][] chunks = new float[Buffer.ChannelCount][];
                for (int c = 0; c < Buffer.ChannelCount; c++)
                {
                    chunks[c] = new float[chunksize];
                    //Grasshopper.DataTree<double> chunk = new Grasshopper.DataTree<double>();
                    if (i < no_of_chunks - 1)
                    {
                        for (int j = 0; j < chunksize; j++)
                        {
                            chunks[c][j] = ((float)Buffer[c][i * chunksize + j]);
                        }
                    }
                    else 
                    {
                        int j = 0, end = (Buffer.Count - i * chunksize);
                        for (j = 0; j < end; j++)
                        {
                            chunks[c][j] = ((float)Buffer[c][i * chunksize + j]);
                        }
                        for (int k = j; k < chunksize; k++) chunks[c][j] = 0;
                    }
                }
                for (int c = 0; c < Buffer.ChannelCount; c++) SL.Add(new Audio_Signal(chunks, Buffer.SampleFrequency));
            }
            DA.SetDataList(0, SL);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                System.Drawing.Bitmap b = Properties.Resources.Divide_Signal;
                b.MakeTransparent(System.Drawing.Color.White);
                return b;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{8A545C8C-846C-4BDE-80DE-42C15AB652CA}"); }
        }
    }
}