using System;
using Grasshopper.Kernel;

namespace PachydermGH
{
    public class PachGH_AssemblyPriority : GH_AssemblyPriority
    {
        public override GH_LoadingInstruction PriorityLoad()
        {
            // Load through Rhino before any component touches Pachyderm types.
            // The ID comes from the Rhino plug-in's Properties/AssemblyInfo.cs.
            try
            {
                if (Rhino.PlugIns.PlugIn.LoadPlugIn(new Guid("fefc90ab-b92d-47fb-88fc-90bf6569ee11")))
                    return GH_LoadingInstruction.Proceed;
            }
            catch (Exception error)
            {
                Rhino.RhinoApp.WriteLine("Could not load Pachyderm for Rhinoceros: " + error.ToString());
            }
            Rhino.RhinoApp.WriteLine("Pachyderm for Grasshopper requires the matching Pachyderm for Rhinoceros plug-in. Install or enable it, then reload Grasshopper.");
            return GH_LoadingInstruction.Abort;
        }
    }
}
