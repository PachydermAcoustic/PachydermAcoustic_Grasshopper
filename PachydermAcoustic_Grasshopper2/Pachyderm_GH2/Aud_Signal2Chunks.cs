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
    [IoId("8A545C8C-846C-4BDE-80DE-42C15AB652CA")]
    public class Signal2Chunks : Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public Signal2Chunks()
            : base(new Nomen("SignalDivide",
                "Divides a signal into equal sized chunks",
                "Acoustics", "Audio"))
        {
        }

        public Signal2Chunks(IReader reader) : base(reader) { }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void AddInputs(InputAdder inputs)
        {
            inputs.AddGeneric("Input Data", "Signal", "The data to divide...", Access.Item);
            inputs.AddInteger("Chunksize", "Size", "The number of samples in chunks...", Access.Item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void AddOutputs(OutputAdder outputs)
        {
            outputs.AddGeneric("Chunks", "Chs", "The divided signal", Access.Tree);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void Process(IDataAccess access)
        {
            int chunksize = 0;
            object test = new object();
            Audio_Signal Buffer = new Audio_Signal();
            access.GetItem<Audio_Signal>(0, out Buffer);            
            //access.GetItem<object>(0, out test);
            access.GetItem<int>(1, out chunksize);

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
                    //Grasshopper2.Data.Tree<double> chunk = new Grasshopper2.Data.Tree<double>();
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
            access.SetTree(0, Garden.ITreeFromList(SL));
        }
        protected override IIcon IconInternal
        {
            get
            {
                var assembly = typeof(SPLETC).Assembly;
                var resourceName = "Pachyderm_GH.Icons.Divide_Signal.png";

                using (var stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream == null) return null;

                    var ms = new System.IO.MemoryStream();
                    stream.CopyTo(ms);
                    ms.Position = 0;
                    return Grasshopper2.UI.Icon.PixelIcon.FromStream(ms);
                }
            }
        }
    }
}