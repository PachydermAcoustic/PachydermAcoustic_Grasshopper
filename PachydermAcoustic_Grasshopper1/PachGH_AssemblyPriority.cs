//using System;
//using Grasshopper.Kernel;

//namespace PachydermGH
//{
//    public class PachGH_AssemblyPriority : Grasshopper.Kernel.GH_AssemblyPriority
//    {
//        public override GH_LoadingInstruction PriorityLoad()
//        {
//            //AppDomain.CurrentDomain.AssemblyResolve += GetAssemblyFromRhino;

//            //AppDomain ad = AppDomain.CreateDomain("Opening Domain");
//            //ad.AssemblyResolve += GetAssemblyFromRhino;
//            string PachPath;

//            string[] plugins = Rhino.PlugIns.PlugIn.GetInstalledPlugInNames();
//            bool isinstalled = false;

//            foreach (string p in plugins)
//            {
//                Rhino.RhinoApp.WriteLine(p);
//                if (p == "Pachyderm_Acoustic")
//                {
//                    isinstalled = true;
//                    break;
//                }
//            }

//            if (isinstalled)
//            {
//                PachPath = Pachyderm_Acoustic.UI.PachydermAc_PlugIn.Instance.GetPluginPath();
//                if (PachPath == null || PachPath == "")
//                {
//                    if (Rhino.PlugIns.PlugIn.LoadPlugIn(new Guid("25895777-97d3-4058-8753-503183d4bc01")))
//                    {
//                        PachPath = Pachyderm_Acoustic.UI.PachydermAc_PlugIn.Instance.GetPluginPath();
//                    }
//                }
//            }
//            else
//            {
//                System.Windows.Forms.MessageBox.Show("Pachyderm Acoustic for Grasshopper relies on the Rhinoceros version. However, Pachyderm Acoustic for Rhinoceros has not been installed. Install Pachyderm for Rhinoceros prior to loading grasshopper in order to use Pachyderm for Grasshopper.");
//                return GH_LoadingInstruction.Abort;
//            }
//            PachPath = PachPath.Remove(PachPath.Length - 22);

//            System.Reflection.Assembly.LoadFile(PachPath + "Hare.dll");
//            System.Reflection.Assembly.LoadFile(PachPath + "CLF_Read.dll");
//            System.Reflection.Assembly.LoadFile(PachPath + "MathNet.Numerics.dll");
//            System.Reflection.Assembly.LoadFile(PachPath + "NAudio.dll");
//            System.Reflection.Assembly.LoadFile(PachPath + "Pachyderm_Acoustic.dll");
//            System.Reflection.Assembly.LoadFile(PachPath + "Pachyderm_Acoustic.rhp");
//            return GH_LoadingInstruction.Proceed;
//        }

//        System.Reflection.Assembly GetAssemblyFromRhino(object source, ResolveEventArgs e)
//        {
//            string PachPath;

//            string[] plugins = Rhino.PlugIns.PlugIn.GetInstalledPlugInNames();
//            bool isinstalled = false;

//            foreach (string p in plugins)
//            {
//                Rhino.RhinoApp.WriteLine(p);
//                if (p == "Pachyderm_Acoustic")
//                {
//                    isinstalled = true;
//                    break;
//                }
//            }

//            if (isinstalled)
//            {
//                PachPath = Pachyderm_Acoustic.UI.PachydermAc_PlugIn.Instance.GetPluginPath();
//                if (PachPath == null || PachPath == "")
//                {
//                    if (Rhino.PlugIns.PlugIn.LoadPlugIn(new Guid("25895777-97d3-4058-8753-503183d4bc01")))
//                    {
//                        PachPath = Pachyderm_Acoustic.UI.PachydermAc_PlugIn.Instance.GetPluginPath();
//                    }
//                }
//            }
//            else
//            {
//                System.Windows.Forms.MessageBox.Show("Pachyderm Acoustic for Grasshopper relies on the Rhinoceros version. However, Pachyderm Acoustic for Rhinoceros has not been installed. Install Pachyderm for Rhinoceros prior to loading grasshopper in order to use Pachyderm for Grasshopper.");
//                return null;
//            }
//            PachPath = PachPath.Remove(PachPath.Length - 22);

//            switch (e.Name)
//            {
//                case "Hare":
//                    return System.Reflection.Assembly.LoadFile(PachPath + "Hare.dll");
//                case "CLF_Read":
//                    return System.Reflection.Assembly.LoadFile(PachPath + "CLF_Read.dll");
//                case "MathNet.Numerics":
//                    return System.Reflection.Assembly.LoadFile(PachPath + "MathNet.Numerics.dll");
//                case "NAudio":
//                    return System.Reflection.Assembly.LoadFile(PachPath + "NAudio.dll");
//                case "Pachyderm_Acoustic":
//                    return System.Reflection.Assembly.LoadFile(PachPath + "Pachyderm_Acoustic.rhp");
//            }
//            return null;
//        }
//    }
//}