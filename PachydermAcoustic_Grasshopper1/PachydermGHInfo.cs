using Grasshopper.Kernel;

namespace MyProject1
{
    public class PachydermGHInfo : GH_AssemblyInfo
    {
        public override string AssemblyName
        {
            get
            {
                return "PachGH";
            }
        }

        //Override here any more methods you see fit.
        //Start typing public override..., select a property and push Enter.
        public override string AssemblyDescription
        {
            get
            {
                return "Provides Pachyderm interface elements for Grasshopper";
            }
        }

        public override System.Drawing.Bitmap AssemblyIcon
        {
            get
            {
                return base.AssemblyIcon;
            }
        }

    }
}