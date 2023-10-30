using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace PachydermGH
{
    public class MLS_Tail : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public MLS_Tail()
          : base("Diffuse MLS Tail", "Tail",
              "Dr. Ning Xiang sent AvH a paper concerning constructing a reverberant tail using MLS based signals. This node is the result.",
              "Acoustics", "Audio")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Reverberation Time", "RT", "Reverberation time in seconds. Specify 8 values (for octave bands 63 - 8k).", GH_ParamAccess.list);
            pManager.AddNumberParameter("Direct to Reverberant Ratio", "D2R", "Dictates the reverberant level of the output response, as a function of the direct sound power. Specify 8 values (for octave bands 63 - 8k).", GH_ParamAccess.list);
            pManager.AddNumberParameter("Sampling Frequency", "FS", "The number of samples per second (sampling frequency). 44100 hz. default.", GH_ParamAccess.item, 44100);
            pManager.AddNumberParameter("Duration (milliseconds)", "D_ms", "Impulse Response length in milliseconds. 1000 ms. default", GH_ParamAccess.item, 1000);
            pManager.AddGenericParameter("Direct Sound", "D", "Input the direct sound simulation. (optional) If no direct sound is added, a value of 1 is assumed for the direct intensity.", GH_ParamAccess.item);
            pManager.AddGenericParameter("Image Source", "IS", "Input the image-source simulation. (optional)", GH_ParamAccess.item);
            pManager[4].Optional = true;
            pManager[5].Optional = true;

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Impulse Response", "IR", "The artificial reverberant tail based on MLS noise...", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<double> RT = new List<double>();
            DA.GetDataList<double>(0, RT);
            List<double> D2R = new List<double>();
            DA.GetDataList<double>(1, D2R);
            double FS = 0;
            DA.GetData<double>(2, ref FS);
            double Dur = 0;
            DA.GetData<double>(3, ref Dur);
            Pachyderm_Acoustic.Direct_Sound Dir = default;
            DA.GetData<Pachyderm_Acoustic.Direct_Sound>(4, ref Dir);
            Pachyderm_Acoustic.ImageSourceData IS = default;
            DA.GetData<Pachyderm_Acoustic.ImageSourceData>(5, ref IS);

            Dir.Create_Filter();
            
            if(IS != null) IS.Create_Filter(Dir.SWL, 4096);

            Pachyderm_Acoustic.Environment.Receiver_Bank[] rec = new Pachyderm_Acoustic.Environment.Receiver_Bank[1];

            double[] magnitude = new double[8] {1,1,1,1,1,1,1,1};
            if (Dir != null) { for (int i = 0; i < magnitude.Length; i++) { magnitude[i] = Math.Sqrt(Dir.EnergyValue(i, 0).Sum() * Math.Pow(10, D2R[0] / 10)); } }

            double[] AF = Pachyderm_Acoustic.Utilities.IR_Construction.Auralization_Filter(new Pachyderm_Acoustic.Direct_Sound[1] { Dir }, new Pachyderm_Acoustic.ImageSourceData[1] { IS }, null, Dur, (int)FS, 0, new List<int> { 0 }, false, true);

            double[] Tail = Pachyderm_Acoustic.Audio.Pach_SP.MLS_Reverb((double)Dur/1000d, RT.ToArray(), (int)FS, magnitude);
            double dt = Dir.Time(0);

            double t = 0.1 + dt;

            if (IS != null && IS.Paths.Length > 0)
            {
                //Find the first 1st order sidewall reflection...
                for (int i = 0; i < IS.Paths.Length; i++)
                {
                    if (IS.Paths[0][i].Path[0].Length > 3) { continue; }
                    if (IS.Paths[0][i].TravelTime > 0.010 && IS.Paths[0][i].TravelTime < t) t = IS.Paths[0][i].TravelTime;
                }
                if (t == 0.1 + dt) t = dt + 0.007;
            }
            else
            {
                t = dt + 0.007;
            }

            int start = (int)(t * FS);
            int end = Math.Min(Tail.Length, AF.Length - start);
            for (int i = start; i < end; i++)
            {
                AF[start + i] += Tail[i];
            }

            DA.SetData(0, new Audio_Signal(AF, (int)FS, (int)(dt * FS)));
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        /// 
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
            get { return new Guid("89ebd299-d562-41b4-91bd-0b17a3253c15"); }
        }
    }
}