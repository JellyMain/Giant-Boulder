using UnityEditor;
using UnityEngine;

namespace GeNa.Core
{
    [CustomEditor(typeof(GaiaTimeOfDayLightSyncManager))]
    public class GaiaTimeOfDayLightSyncManagerEditor : Editor
    {
        private GaiaTimeOfDayLightSyncManager m_manager;
        private const string GlobalHelpBox = "This manager handles all the light syncs in the scene.";
        private Color m_backgroundColor;

        public override void OnInspectorGUI()
        {
            m_manager = (GaiaTimeOfDayLightSyncManager)target;
            m_backgroundColor = GUI.backgroundColor;
            EditorGUILayout.HelpBox(GlobalHelpBox, MessageType.Info);

#if HDPipeline && HDRPTIMEOFDAY
            if (GUILayout.Button(new GUIContent("Convert All To Standalone HDRP TOD",
                    "Converts all the gena light syncs scripts to HDRP TOD scripts.")))
            {
                if (EditorUtility.DisplayDialog("Converting",
                        "This will convert the gena light sync scripts to HDRP Standalone in the scene, note this can not be undone.",
                        "Yes", "No"))
                {
                    ConvertToHDRPStandalone();
                }
            }
#else
            EditorGUI.BeginChangeCheck();
            Camera cam = m_manager.MainCamera;
            LightShadows lightShadows = m_manager.DefaultShadowCastingMode;
            bool enabled = m_manager.EnableSystem;

            enabled = EditorGUILayout.Toggle(new GUIContent("System Enabled", "If enabled the code to process the light sync system will be executed"), enabled);
            if (enabled)
            {
                EditorGUI.indentLevel++;
                cam = (Camera)EditorGUILayout.ObjectField(new GUIContent("Main Camera", "Camera used to calculate the render culling distance"), cam, typeof(Camera), true);
                lightShadows = (LightShadows)EditorGUILayout.EnumPopup(new GUIContent("Default Shadows Mode", "Sets the default shadow casting state mode when you are within the shadow culling range"), lightShadows);
                int currentCount = m_manager.GetCurrentSyncCount(out int activeCount);
                EditorGUILayout.LabelField("Light Syncs Found In The Scene: " + currentCount, EditorStyles.boldLabel);
                GUI.backgroundColor = Color.green;
                EditorGUILayout.LabelField("Light Syncs Active In The Scene: " + activeCount + " out of " + currentCount, EditorStyles.boldLabel);
                GUI.backgroundColor = m_backgroundColor;
                EditorGUI.indentLevel--;
            }

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(m_manager, "Manager changed");
                m_manager.MainCamera = cam;
                m_manager.DefaultShadowCastingMode = lightShadows;
                m_manager.EnableSystem = enabled;
                EditorUtility.SetDirty(m_manager);
            }
#endif
        }

        public static void ConvertToHDRPStandalone()
        {
            GaiaTimeOfDayLightSync[] lightSyncs = GeNaEvents.FindObjectsOfType<GaiaTimeOfDayLightSync>();
            if (lightSyncs.Length > 0)
            {
                for (int i = lightSyncs.Length - 1; i >= 0; i--)  
                {
                    ConvertToHDRPStandalone(lightSyncs[i]);
                }   
            }
        }
        public static void ConvertToHDRPStandalone(GaiaTimeOfDayLightSync lightSync)
        { 
            // Note: This method has been disabled until the new version of HDRP TOD has been released.
// #if HDPipeline && HDRPTIMEOFDAY
//             if (lightSync != null)
//             {
//                 ProceduralWorlds.HDRPTOD.HDRPTimeOfDayLightComponent hdrpLightSync = lightSync.gameObject.AddComponent<ProceduralWorlds.HDRPTOD.HDRPTimeOfDayLightComponent>();
//                 hdrpLightSync.m_lightSource = lightSync.m_lightSource;
//                 hdrpLightSync.m_useRenderDistance = lightSync.m_useRenderDistance;
//                 hdrpLightSync.m_renderDistance = lightSync.m_renderDistance;
//                 switch (lightSync.m_renderMode)
//                 {
//                     case LightSyncRenderMode.AlwaysOn:
//                     {
//                         hdrpLightSync.m_renderMode = ProceduralWorlds.HDRPTOD.LightSyncRenderMode.AlwaysOn;
//                         break;
//                     }
//                     case LightSyncRenderMode.NightOnly:
//                     {
//                         hdrpLightSync.m_renderMode = ProceduralWorlds.HDRPTOD.LightSyncRenderMode.NightOnly;
//                         break;
//                     }
//                     case LightSyncRenderMode.AlwaysOnOptimized:
//                     {
//                         hdrpLightSync.m_renderMode = ProceduralWorlds.HDRPTOD.LightSyncRenderMode.AlwaysOnOptimized;
//                         break;
//                     }
//                 }
//                 hdrpLightSync.RefreshLightSource();
//                 if (hdrpLightSync.m_lightData == null)
//                 {
//                     hdrpLightSync.m_lightData = lightSync.m_lightSource.GetComponent<UnityEngine.Rendering.HighDefinition.HDAdditionalLightData>();
//                     if (hdrpLightSync.m_lightData == null)
//                     {
//                         hdrpLightSync.m_lightData = lightSync.m_lightSource.gameObject.AddComponent<UnityEngine.Rendering.HighDefinition.HDAdditionalLightData>();
//                     }
//                 }
//                 hdrpLightSync.m_lightData.SetIntensity(20, UnityEngine.Rendering.HighDefinition.LightUnit.Ev100);
//
//                 DestroyImmediate(lightSync);
//             }
// #endif
        }
    }
}