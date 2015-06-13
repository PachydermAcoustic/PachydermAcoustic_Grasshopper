using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace PachydermGH
{
    public class ComplexPaToSPL : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent2 class.
        /// </summary>
        public ComplexPaToSPL()
            : base("Complex Pressure to SPL", "SPL-CPa",
                "Converts complex pressure values to Sound Pressure Level",
                "Acoustics", "General")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddComplexNumberParameter("Complex Pressure", "P", "Complex-valued pressure", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Sound Pressure Level", "SPL", "Sound Pressure Level", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Grasshopper.Kernel.Types.Complex> Pa = new List<Grasshopper.Kernel.Types.Complex>();
            double[] P = new double[Pa.Count];
            DA.GetDataList<Grasshopper.Kernel.Types.Complex>(0, Pa);
            for(int i = 0; i < Pa.Count; i++)
            {
                P[i] = Pachyderm_Acoustic.Utilities.AcousticalMath.SPL_Pressure(Pachyderm_Acoustic.Utilities.Numerics.Abs((float)Pa[i].Real, (float)Pa[i].Imaginary));
            }
            DA.SetDataList(0, P);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return null;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{C4DAFA2D-3B55-4D96-B011-05F1BC13989E}"); }
        }
    }
}