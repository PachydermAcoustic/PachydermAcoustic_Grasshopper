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

using Grasshopper2.Components;
using Grasshopper2.Data;
using Grasshopper2.Parameters;
using Grasshopper2.UI;
using Grasshopper2.UI.Icon;
using GrasshopperIO;
using Pachyderm_Acoustic;
using Pachyderm_Acoustic.Environment;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.IO;

namespace PachydermGH
{
    [IoId("251477CD-DE9E-48E0-9F97-1B68240E1F09")]
    public class LE_ETC : Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent2 class.
        /// </summary>
        public LE_ETC()
            : base(new Nomen("Lateral Efficiency",
                "Computes Lateral Fraction from Energy Time Curve",
                "Acoustics", "Analysis"))
        {
            Threading = Grasshopper2.Components.ThreadingState.SingleThreaded;
        }
        public LE_ETC(IReader reader) : base(reader) { Threading = Grasshopper2.Components.ThreadingState.SingleThreaded; }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void AddInputs(InputAdder inputs)
        {
            inputs.AddGeneric("Direct Sound", "D", "Plug the Direct Sound in here.", Access.Tree);
            inputs.AddGeneric("Image Source", "IS", "Plug the Image Source in here.", Access.Tree, Requirement.MayBeMissing);
            inputs.AddGeneric("Ray Tracing", "Tr", "Plug the Receiver from Ray Tracing in here.", Access.Tree, Requirement.MayBeMissing);
            inputs.AddNumber("Altitude", "Alt", "Euler altitude angle.", Access.Tree, Requirement.MayBeMissing);
            inputs.AddNumber("Azimuth", "Azi", "Euler azimuth angle.", Access.Tree, Requirement.MayBeMissing);
            inputs.AddInterval("Frequency Scope", "Oct", "An interval of the first and last octave to calculate (0 = 62.5 Hz, 1 = 125 HZ., ..., 7 = 8000 Hz.", Access.Item).Set(new Rhino.Geometry.Interval(0,7));

            //inputs[1].Optional = true;
            //inputs[2].Optional = true;
            //inputs[5].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void AddOutputs(OutputAdder outputs)
        {
            outputs.AddNumber("Lateral Efficiency", "LE", "Lateral Efficiency", Access.Tree);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void Process(IDataAccess access)
        {
            var sims=new ComponentSupport.Simulations(access);
            var altitude=ComponentSupport.Items<double>(access,3); var azimuth=ComponentSupport.Items<double>(access,4);
            if((altitude.Length==0)!=(azimuth.Length==0)) throw new ArgumentException("Provide both altitude and azimuth.");
            var octave=new Interval(0,7); if(!access.GetItem<Interval>(5,out octave)) octave=new Interval(0,7);
            ComponentSupport.Octaves(octave,out int first,out int last);
            var rows=new List<double[]>(); var paths=new List<Grasshopper2.Data.Path>();
            for(int source=0;source<sims.Count;source++) {
                for(int receiver=0;receiver<sims.ReceiverCount(source);receiver++) {
                    double alt,azi;
                    if(altitude.Length==0) {
                        if(sims.Direct[source]==null || sims.Receivers[source]==null) throw new ArgumentException("Automatic orientation requires direct sound and receiver data.");
                        Pachyderm_Acoustic.Utilities.PachTools.World_Angles(sims.Direct[source].Src.Origin,sims.Receivers[source].Origin(receiver),true,out alt,out azi);
                    } else { alt=ComponentSupport.Angle(altitude,receiver,sims.ReceiverCount(source)); azi=ComponentSupport.Angle(azimuth,receiver,sims.ReceiverCount(source)); }
                    var values=new double[last-first+1]; int fs=sims.SampleRate(source);
                    for(int oct=first;oct<=last;oct++) {
                        var etc=Pachyderm_Acoustic.Utilities.IR_Construction.ETCurve(sims.Direct[source],sims.Images[source],sims.Receivers[source],sims.Cutoff(source),fs,oct,receiver,false);
                        var lateral=Pachyderm_Acoustic.Utilities.IR_Construction.ETCurve_1d(sims.Direct[source],sims.Images[source],sims.Receivers[source],sims.Cutoff(source),fs,oct,receiver,false,alt,azi,true)[1];
                        values[oct-first]=Pachyderm_Acoustic.Utilities.AcousticalMath.Lateral_Efficiency(etc,lateral,fs,(double)sims.Arrival(source,receiver,fs)/fs,false);
                    }
                    rows.Add(values); paths.Add(new Grasshopper2.Data.Path(source,receiver));
                }
            }
            ComponentSupport.SetTree(access, 0,Garden.TreeFromArrays(new Grasshopper2.Data.Paths(paths),rows.ToArray()));
        }

        protected override IIcon IconInternal
        {
            get
            {
                var assembly = typeof(SPLETC).Assembly;
                var resourceName = "PachydermGH2.Resources.Geodesic Source.png";

                using (var stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream == null) return null;

                    // FromStream reads serialized .ghicon data, not PNG/BMP images.
                    // The PixelIcon retains the bitmap for its cached lifetime.
                    return new Grasshopper2.UI.Icon.PixelIcon(new Eto.Drawing.Bitmap(stream));
                }
            }
        }

        //protected override IIcon IconInternal => new Grasshopper2.UI.Icon.PixelIcon("Geodesic_Source");
    }
}
