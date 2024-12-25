using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine.SceneManagement;

namespace GeNa.Core
{
    public class GeNaRoadPreprocessBuild : IProcessSceneWithReport
    {
        int IOrderedCallback.callbackOrder => int.MaxValue;

        void IProcessSceneWithReport.OnProcessScene(Scene scene, BuildReport report)
        {
            if (report == null)
                return;

            if (scene.isSubScene)
                return;

            GeNaRoadExtension[] roadExtensions = GeNaEvents.FindObjectsOfType<GeNaRoadExtension>();

            List<GeNaRoadExtension> activeRoadExtensions = new List<GeNaRoadExtension>();
            foreach (GeNaRoadExtension extension in roadExtensions)
            {
                if (extension.IsActive && extension.Spline.gameObject.activeInHierarchy)
                        activeRoadExtensions.Add(extension);
            }

            if (activeRoadExtensions.Count > 0)
            {
                bool PostProcessSplitAtTerrains = true;

                if (GeNaEvents.HasTerrainsAsScenes())
                {
                    Debug.LogWarning($"Warning: {activeRoadExtensions.Count} active Road Spline(s) have not been baked.");
                    PostProcessSplitAtTerrains = false;
                }
                else
                {
                    Debug.Log($"Preprocessing {activeRoadExtensions.Count} active Road networks.");
                }

                foreach (GeNaRoadExtension roadExtension in activeRoadExtensions)
                {
                    roadExtension.Bake(PostProcessSplitAtTerrains);
                }
            }
        }
    }
}