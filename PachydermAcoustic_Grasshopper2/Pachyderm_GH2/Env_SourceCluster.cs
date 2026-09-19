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
using System.Collections.Generic;
using Grasshopper2.Data;

namespace PachydermGH
{
    [IoId("b57c2e3f-84a6-4d12-95c3-6a81d7e45ad2")]
    public class SourceCluster_Component : Component
    {
        /// <summary>
        /// Initializes a new instance of the SourceCluster_Component class.
        /// </summary>
        public SourceCluster_Component()
          : base(new Nomen("Source Cluster",
              "Combines multiple source objects into a single source cluster",
              "Acoustics", "Model"))
        {
        }

        public SourceCluster_Component(IReader reader) : base(reader) { }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void AddInputs(InputAdder inputs)
        {
            inputs.AddGeneric("Sources", "Src", "Pachyderm source objects to cluster", Access.Tree);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void AddOutputs(OutputAdder outputs)
        {
            outputs.AddGeneric("Cluster", "Clstr", "Source cluster combining input sources", Access.Item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void Process(IDataAccess access)
        {
            // Get the input sources
            Tree<Pachyderm_Acoustic.Environment.Source> sources;
            if (!access.GetTree(0, out sources)) return;

            if (sources.ItemCount == 0)
            {
                access.AddMessage(Grasshopper2.Doc.Message.Warning("No source objects provided",""));
                return;
            }

            // Create a new Source_Cluster from the input sources
            Pachyderm_Acoustic.Environment.SourceCluster cluster = new Pachyderm_Acoustic.Environment.SourceCluster(new List<Source>(sources.AllItems),0);

            // Output the cluster
            access.SetItem(0, cluster);
        }

        protected override IIcon IconInternal
        {
            get
            {
                var assembly = typeof(SPLETC).Assembly;
                var resourceName = "Pachyderm_GH.Icons.Geodesic_Source.png";

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
        //protected override IIcon IconInternal => new Grasshopper2.UI.Icon.PixelIcon("Geodesic_Source");
    }
}