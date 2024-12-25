using UnityEngine;
using UnityEditor;

namespace GeNa.Core
{
    [CustomEditor(typeof(GeNaMaterialDecoratorDatabase))]
    public class GeNaMaterialDecoratorDatabaseEditor : Editor
    {
        private GeNaMaterialDecoratorDatabase m_editor;

        private void OnEnable()
        {
            m_editor = (GeNaMaterialDecoratorDatabase) target;
            if (m_editor != null)
            {
                m_editor.GetPresets();
            }
        }
        public override void OnInspectorGUI()
        {
            if (m_editor == null)
            {
                m_editor = (GeNaMaterialDecoratorDatabase) target;
            }

            DrawDefaultInspector();

            EditorGUI.BeginChangeCheck();

            if (GUILayout.Button("Refresh Presets"))
            {
                m_editor.GetPresets(true);
            }

            if (m_editor.m_presets.Count > 0)
            {
                EditorGUILayout.LabelField("Global Overrides", EditorStyles.boldLabel);
                m_editor.m_overridePreset = EditorGUILayout.Toggle("Override Preset", m_editor.m_overridePreset);
                if (m_editor.m_overridePreset)
                {
                    EditorGUI.indentLevel++;
                    m_editor.m_selectedPreset = EditorGUILayout.Popup("Preset", m_editor.m_selectedPreset, m_editor.m_presets.ToArray());
                    EditorGUI.indentLevel--;
                }
            }

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(m_editor);
            }
        }
    }
}