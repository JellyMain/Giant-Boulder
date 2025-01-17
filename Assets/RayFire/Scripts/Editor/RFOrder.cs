using UnityEditor;
using RayFire;

namespace RayFireEditor
{
    [InitializeOnLoad]
    public class RFScriptOrder : Editor
    {
        static RFScriptOrder()
        {
            int manExe = -50;

            string man  = typeof(RayfireMan).Name;
            string uny  = typeof(RayfireUnyielding).Name;
            string conn = typeof(RayfireConnectivity).Name;
            
            foreach (MonoScript mono in MonoImporter.GetAllRuntimeMonoScripts())
            {
                if (mono.GetClass() != null)
                {
                    if (mono.name == man)
                    {
                        if (MonoImporter.GetExecutionOrder (mono) != manExe)
                            MonoImporter.SetExecutionOrder (mono, manExe);
                    }
                    else if (mono.name == uny)
                    {
                        if (MonoImporter.GetExecutionOrder (mono) != 10)
                            MonoImporter.SetExecutionOrder (mono, 10);
                    }
                    else if (mono.name == conn)
                    {
                        if (MonoImporter.GetExecutionOrder (mono) != 20)
                            MonoImporter.SetExecutionOrder (mono, 20);
                    }
                }
            }
        }
    }
}