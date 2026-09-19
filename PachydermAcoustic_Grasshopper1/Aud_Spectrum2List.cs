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
using System.Collections.Generic;
using Grasshopper.Kernel;

namespace PachydermGH
{
    public class Spectrum2List : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public Spectrum2List()
            : base("Spectrum2List", "SpectrumOut",
                "Casts a signal to a list readable in Grasshopper",
                "Acoustics", "Audio")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Input Data", "Signal", "The data to divide...", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Magnitude", "M", "The exported Frequency Magnitude in pressure...", GH_ParamAccess.list);
            pManager.AddComplexNumberParameter("Spectrum", "SC", "The complex power at each frequency in the spectrum...", GH_ParamAccess.list);
            pManager.AddNumberParameter("Frequency", "F", "The exported Frequency Domain...", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //int chan = 0;
            Frequency_Spectrum Buffer = new Frequency_Spectrum();
            DA.GetData<Frequency_Spectrum>(0, ref Buffer);
            //DA.GetData<int>(1, ref chan);

            List<Grasshopper.Kernel.Types.GH_Number> signal = new List<Grasshopper.Kernel.Types.GH_Number>();
            List<Grasshopper.Kernel.Types.GH_ComplexNumber> spectrum = new List<Grasshopper.Kernel.Types.GH_ComplexNumber>(); 
            List<Grasshopper.Kernel.Types.GH_Number> Freq = new List<Grasshopper.Kernel.Types.GH_Number>();
            for (int s = 0; s < Buffer.Magnitude.Length; s++) signal.Add(new Grasshopper.Kernel.Types.GH_Number(Buffer.Magnitude[s]));
            for (int s = 0; s < Buffer.Value.Length; s++ ) spectrum.Add(new Grasshopper.Kernel.Types.GH_ComplexNumber(new Grasshopper.Kernel.Types.Complex(Buffer.Value[s].Real, Buffer.Value[s].Imaginary)));
            for (int s = 0; s < Buffer.Frequency.Length; s++ ) Freq.Add(new Grasshopper.Kernel.Types.GH_Number(Buffer.Frequency[s]));

            DA.SetDataList(0, signal);
            DA.SetDataList(1, spectrum);
            DA.SetDataList(2, Freq);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                System.Drawing.Bitmap b = Properties.Resources.Signal_to_List;
                b.MakeTransparent(System.Drawing.Color.White);
                return b;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{0F1433F5-AED9-470D-8FDA-BD40A9DB5B59}"); }
        }
    }
}