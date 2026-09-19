using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Rhino.Geometry;
using Pachyderm_Acoustic.Environment;

namespace PachydermGH
{
    public class SourceCluster_Component : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the SourceCluster_Component class.
        /// </summary>
        public SourceCluster_Component()
          : base("Source Cluster", "SrcCluster",
              "Combines multiple source objects into a single source cluster",
              "Acoustics", "Model")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Sources", "Src", "Pachyderm source objects to cluster", GH_ParamAccess.list);
            // Add additional parameters if needed for the Source_Cluster configuration
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Cluster", "Clstr", "Source cluster combining input sources", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // Get the input sources
            List<Pachyderm_Acoustic.Environment.Source> sources = new List<Pachyderm_Acoustic.Environment.Source>();
            if (!DA.GetDataList(0, sources)) return;

            if (sources.Count == 0)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "No source objects provided");
                return;
            }

            // Create a new Source_Cluster from the input sources
            Pachyderm_Acoustic.Environment.SourceCluster cluster = new Pachyderm_Acoustic.Environment.SourceCluster(sources,0);

            // Output the cluster
            DA.SetData(0, cluster);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Internal_Icon_24x24 => Properties.Resources.Geodesic_Source;

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("b57c2e3f-84a6-4d12-95c3-6a81d7e45ad2"); }
        }
    }
}