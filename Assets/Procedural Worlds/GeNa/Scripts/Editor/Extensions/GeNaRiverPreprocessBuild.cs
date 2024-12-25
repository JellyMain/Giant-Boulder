using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine.SceneManagement;

namespace GeNa.Core
{
    public class GeNaRiverPreprocessBuild : IProcessSceneWithReport
    {
        int IOrderedCallback.callbackOrder => int.MaxValue;

        void IProcessSceneWithReport.OnProcessScene(Scene scene, BuildReport report)
        {
            if (report == null)
                return;

            if (scene.isSubScene)
                return;

            GeNaRiverExtension[] riverExtensions = GeNaEvents.FindObjectsOfType<GeNaRiverExtension>();

            List<GeNaRiverExtension> activeRiverExtensions = new List<GeNaRiverExtension>();
            foreach (GeNaRiverExtension extension in riverExtensions)
                if (extension.IsActive && extension.Spline.gameObject.activeInHierarchy)
                    activeRiverExtensions.Add(extension);


            if (activeRiverExtensions.Count > 0)
            {
                bool PostProcessSplitAtTerrains = true;
                if (GeNaEvents.HasTerrainsAsScenes())
                {
                    Debug.LogWarning($"Warning: {activeRiverExtensions.Count} active River Spline(s) have not been baked.");
                    PostProcessSplitAtTerrains = false;
                }
                else
                {
                    Debug.Log($"Preprocessing {activeRiverExtensions.Count} active Rivers.");
                }

                foreach (GeNaRiverExtension riverExtension in activeRiverExtensions)
                {
                    riverExtension.Bake(PostProcessSplitAtTerrains);
                }
            }
            /*
            List<GeNaRiverExtension> riversToProcess = new List<GeNaRiverExtension>();
            foreach (GeNaRiverExtension activeRiver in activeRiverExtensions)
            {
                if (activeRiver.RiverProfile != null && activeRiver.RiverProfile.RiverParamaters != null && activeRiver.RiverProfile.RiverParamaters.m_renderMode == Constants.ProfileRenderMode.RiverFlow)
                    riversToProcess.Add(activeRiver);
            }

            if (riversToProcess.Count > 0)
            {
                Debug.Log($"Processing {riversToProcess.Count} active Rivers with the new River Shader.");

                foreach (GeNaRiverExtension riverExtension in riversToProcess)
                {
                    riverExtension.Bake();
                }
            }
            */
        }
    }
}