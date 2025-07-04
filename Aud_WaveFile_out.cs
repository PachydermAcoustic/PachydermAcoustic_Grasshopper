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
using Rhino.Geometry;
using System.Linq;

namespace PachydermGH
{
    public class ExportWaveFile : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent2 class.
        /// </summary>
        public ExportWaveFile()
            : base("Write Wave File", ".wav-out",
                "Writes a signal to a wave file.",
                "Acoustics", "Audio")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Signal Buffer", "Signal", "All input signals to be written to wave file. Signals will be written in channels according to their order at input.", GH_ParamAccess.item);
            pManager.AddTextParameter("Wave File Path", "Path", "The location of the wave file.", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
        }

        //public override bool AppendMenuItems(ToolStripDropDown menu)
        //{
        //    if (base.AppendMenuItems(menu))
        //    {
        //        Menu_AppendItem(menu, "16-bit", bitrate_click, true, bitrate == 16);
        //        //Menu_AppendItem(menu, "24-bit", bitrate_click, true, bitrate == 24);
        //        Menu_AppendItem(menu, "32-bit", bitrate_click, true, bitrate == 32);
        //        Menu_AppendItem(menu, "Normalize", normalize_click, true, Normalize);
        //        return true;
        //    }
        //    return false;
        //}

        int bitrate = 16;
        bool Normalize = false;

        public void bitrate_click(Object sender, EventArgs e)
        {
            bitrate = int.Parse(sender.ToString().Split('-')[0]);
            this.ExpireSolution(true);
        }

        public void normalize_click(Object sender, EventArgs e)
        {
            Normalize = !Normalize;
            this.ExpireSolution(true);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {   
            Audio_Signal S = new Audio_Signal();
            DA.GetData<Audio_Signal>(0, ref S);
            if (S.Count < 1) throw new Exception("Signals should be type of Audio Signal.");

            string path = "";
            DA.GetData(1, ref path);
            if (!path.EndsWith(".wav")) throw new Exception("Path invalid. Make sure that directory exists, and that the ");

            if (Normalize)
            {
                for (int a = 0; a < S.Count; a++)
                {
                    double m = S[a].Max();
                    for (int i = 0; i < S[a].Length; i++) S[a][i] /= m;
                }
            }

            float[][] data = new float[S.ChannelCount][];
            for (int i = 0; i < S.ChannelCount; i++) data[i] = S.toFloat(i);
            Pachyderm_Acoustic.Audio.Pach_SP.Wave.Write(data, S.SampleFrequency, path, bitrate);
        }

        /// <summary>
        /// Provides an Icon for the component....
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                System.Drawing.Bitmap b = Properties.Resources.Wave_File;
                b.MakeTransparent(System.Drawing.Color.White);
                return b;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{F3098E20-508B-4ED3-BE41-5BDDDE87E71D}"); }
        }
    }
}