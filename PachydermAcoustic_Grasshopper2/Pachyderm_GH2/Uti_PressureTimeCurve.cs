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

namespace PachydermGH
{
    [IoId("6DB47BF7-B4C5-410F-8155-88434902BE4A")]
    public class PressureTimeCurve : Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent2 class.
        /// </summary>
        public PressureTimeCurve()
            : base(new Nomen("Pressure-Time Curve",
                "Creates the Impulse Response from simulation results. Note that this version of the impulse response has the power spectrum of the source, and can be used for SPL predictions, but should not be used for auralization.",
                "Acoustics", "Utility"))
        {
            Threading = Grasshopper2.Components.ThreadingState.SingleThreaded;
        }

        public PressureTimeCurve(IReader reader) : base(reader) { Threading = Grasshopper2.Components.ThreadingState.SingleThreaded; Combine=reader.TryRead<bool>("Combine",true);}

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        public override void Store(IWriter writer) { base.Store(writer); writer.Boolean("Combine",Combine); }
        protected override void AddInputs(InputAdder inputs)
        {
            inputs.AddGeneric("Direct Sound", "D", "Plug the Direct Sound in here.", Access.Tree);
            inputs.AddGeneric("Image Source", "IS", "Plug the Image Source in here.", Access.Tree);
            inputs.AddGeneric("Ray Tracing", "Tr", "Plug the Receiver from Ray Tracing in here.", Access.Tree);
            inputs.AddInterval("Frequency Scope", "Oct", "An interval of the first and last octave to calculate (0 = 62.5 Hz, 1 = 125 HZ., ..., 7 = 8000 Hz.", Access.Item).Set(new Rhino.Geometry.Interval(0,7));

            inputs[0].Requirement = Requirement.MayBeMissing;
            inputs[1].Requirement = Requirement.MayBeMissing;
            inputs[2].Requirement = Requirement.MayBeMissing;
            inputs[3].Requirement = Requirement.MayBeMissing;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void AddOutputs(OutputAdder outputs)
        {
            outputs.AddGeneric("Energy-Time Curve", "ETC", "The energy-time-curve result of the simulation...", Access.Tree);
        }

        public override void AppendToInputPanel(Grasshopper2.UI.InputPanel.InputPanel panel)
        {
            base.AppendToInputPanel(panel);
            panel.AddCheck("Combine sources at each receiver",Combine,value => {
                Combine=value; Expire(); Document?.Solution.Start();
            });
        }
        bool Combine = true;

        //private void Combine_Click(Object sender, EventArgs e)
        //{
        //    Combine = !Combine;
        //    ExpireSolution(true);
        //}

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="access">The access object is used to retrieve from inputs and store in outputs.</param>
        protected override void Process(IDataAccess access)
        {
            var simulations = new ComponentSupport.Simulations(access);
            var octaves = new Interval(0,7);
            if (!access.GetItem<Interval>(3, out octaves)) octaves = new Interval(0,7);
            ComponentSupport.Octaves(octaves, out int first, out int last);
            double[] altitude = new double[] { 0 }, azimuth = new double[] { 0 };
            int degree = 0, standard = 0;
            var result = new List<Audio_Signal>();
            var paths = new List<Grasshopper2.Data.Path>();
            for(int source=0; source<simulations.Count; source++) {
                int receivers=simulations.ReceiverCount(source);
                if(Combine && source>0 && result.Count!=receivers) throw new ArgumentException("Combined sources must share receiver counts and ordering.");
                for(int receiver=0; receiver<receivers; receiver++) {
                    double alt=ComponentSupport.Angle(altitude,receiver,receivers), azi=ComponentSupport.Angle(azimuth,receiver,receivers);
                    var signal=ComponentSupport.Response(simulations,source,receiver,"Pressure",first,last,alt,azi,degree,standard);
                    if(Combine && source>0) result[receiver]=ComponentSupport.Sum(result[receiver],signal);
                    else { result.Add(signal); paths.Add(Combine ? new Grasshopper2.Data.Path(receiver) : new Grasshopper2.Data.Path(source,receiver)); }
                }
            }
            ComponentSupport.SetTree(access, 0,Garden.TreeFromArrays(new Grasshopper2.Data.Paths(paths),result.Select(signal=>new[]{signal}).ToArray()));
        }
        protected override IIcon IconInternal
        {
            get
            {
                var assembly = typeof(SPLETC).Assembly;
                var resourceName = "PachydermGH2.Resources.Energy Time Curve.png";

                using (var stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream == null) return null;

                    // FromStream reads serialized .ghicon data, not PNG/BMP images.
                    // The PixelIcon retains the bitmap for its cached lifetime.
                    return new Grasshopper2.UI.Icon.PixelIcon(new Eto.Drawing.Bitmap(stream));
                }
            }
        }

        //protected override IIcon IconInternal => new Grasshopper2.UI.Icon.PixelIcon("Energy_Time_Curve");
    }
}
