using PWCommon5;
using UnityEditor;
using UnityEngine;

namespace GeNa.Core
{
    [CustomEditor(typeof(GaiaTimeOfDayLightSync))]
    public class GaiaTimeOfDayLightSyncEditor : PWEditor
    {
        private GaiaTimeOfDayLightSync m_editor;
        private GUIStyle m_boxStyle;
        private EditorUtils m_editorUtils;

        private void OnEnable()
        {
            m_editorUtils = PWApp.GetEditorUtils(this, null, null);
        }
        private void OnDestroy()
        {
            if (m_editorUtils != null)
            {
                m_editorUtils.Dispose();
                m_editorUtils = null;
            }
        }
        public override void OnInspectorGUI()
        {
            m_editorUtils.Initialize();
            m_editor = (GaiaTimeOfDayLightSync)target;
            //Set up the box style
            if (m_boxStyle == null)
            {
                m_boxStyle = new GUIStyle(GUI.skin.box)
                {
                    normal = {textColor = GUI.skin.label.normal.textColor},
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.UpperLeft
                };
            }
          
            m_editorUtils.Panel("GlobalPanel", GlobalPanel, true);
        }

        private void GlobalPanel(bool helpEnabled)
        {
#if HDPipeline && HDRPTIMEOFDAY
            if (GUILayout.Button(new GUIContent("Convert To Standalone HDRP TOD",
                    "Converts all the gena light syncs scripts to HDRP TOD scripts.")))
            {
                if (EditorUtility.DisplayDialog("Converting",
                        "This will convert the gena light sync scripts to HDRP Standalone in the scene, note this can not be undone.",
                        "Yes", "No"))
                {
                    GaiaTimeOfDayLightSyncManagerEditor.ConvertToHDRPStandalone(m_editor);
                }
            }
#else
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.BeginVertical(m_boxStyle);
            EditorGUILayout.LabelField("Component Settings");
            m_editor.m_lightSource = (Light)m_editorUtils.ObjectField("Light Source", m_editor.m_lightSource, typeof(Light), true, helpEnabled);
            m_editor.m_emissionObject = (GameObject)m_editorUtils.ObjectField("Emission GameObject", m_editor.m_emissionObject, typeof(GameObject), true, helpEnabled);

            EditorGUILayout.EndVertical();
            if (EditorGUI.EndChangeCheck())
            {
                m_editor.ValidateComponents(GaiaTimeOfDayLightSyncManager.Instance);
                if (m_editor != null)
                {
                    EditorUtility.SetDirty(m_editor);
                }
            }

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.BeginVertical(m_boxStyle);
            EditorGUILayout.LabelField("Light Sync Settings");
            m_editor.m_renderMode = (LightSyncRenderMode)m_editorUtils.EnumPopup("RenderMode", m_editor.m_renderMode, helpEnabled);
            if (m_editor.m_renderMode != LightSyncRenderMode.AlwaysOn)
            {
                EditorGUI.indentLevel++;
                m_editor.m_useRenderDistance = m_editorUtils.Toggle("UseRenderDistance", m_editor.m_useRenderDistance, helpEnabled);
                if (m_editor.m_useRenderDistance)
                {
                    EditorGUI.indentLevel++;
                    m_editor.m_renderDistance = m_editorUtils.FloatField("RenderDistance", m_editor.m_renderDistance, helpEnabled);
                    m_editor.m_useShadowDistanceCulling = m_editorUtils.Toggle("UseShadowCulling", m_editor.m_useShadowDistanceCulling);
                    if (m_editor.m_useShadowDistanceCulling)
                    {
                        EditorGUI.indentLevel++;
                        m_editor.m_shadowRenderDistance = m_editorUtils.FloatField("ShadowRenderDistance", m_editor.m_shadowRenderDistance, helpEnabled);
                        EditorGUI.indentLevel--;
                    }
                    EditorGUI.indentLevel--;
                }

                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndVertical();
            if (EditorGUI.EndChangeCheck())
            {
                m_editor.Refresh();
                if (m_editor != null)
                {
                    EditorUtility.SetDirty(m_editor);
                }
            }
#endif
        }
    }
}