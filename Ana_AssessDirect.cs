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
    public class Assess_Direct : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent2 class.
        /// </summary>
        public Assess_Direct()
            : base("Assess Direct Time", "Find Direct",
                "Takes an impulse response, finds the direct sound, and adds the direct time to the signal object for use in analysis.",
                "Acoustics", "Analysis")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Impulse Response", "IR", "The impulse response, or other signal for which the direct sound must be found.", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Mod-ms", "_t", "Did we get the time wrong? This method is pretty good, but it isn't perfect. Use this to specify the number of milliseconds to modify it by.", GH_ParamAccess.item, 0);
            pManager[1].Optional = true;
            Grasshopper.Kernel.Parameters.Param_Integer param = (pManager[1] as Grasshopper.Kernel.Parameters.Param_Integer);
        }

        //bool Noise_Compensation = false;

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Impulse Response_out", "IRout", "The modified impulse response with the direct time built in.", GH_ParamAccess.item);

        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Audio_Signal ETC = null;
            DA.GetData<Audio_Signal>(0, ref ETC);
            int tx = 0;
            DA.GetData<int>(1, ref tx);

            ETC.Direct_Sample = new int[ETC.ChannelCount];

            double deltaEMax = 0;
            int D_Sound = 0;
            for (int c = 0; c < ETC.ChannelCount; c++)
            { 
                for (int i = 1; i < ETC.Count; i++)
                {
                    double deltaE = ETC[c][i] * ETC[c][i] - ETC[c][i - 1] * ETC[c][i - 1];
                    if (deltaE > deltaEMax)
                    {
                        deltaEMax = deltaE;
                        D_Sound = i;
                    }
                }
                ETC.Direct_Sample[c] = D_Sound + (int)(tx * ETC.SampleFrequency);
                
            }

            DA.SetData(0, ETC);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                System.Drawing.Bitmap b = Properties.Resources.RT;
                b.MakeTransparent(System.Drawing.Color.White);
                return b;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{E60566C0-95C5-4FC0-BEA2-EDEECD31A5FD}"); }
        }
    }
}