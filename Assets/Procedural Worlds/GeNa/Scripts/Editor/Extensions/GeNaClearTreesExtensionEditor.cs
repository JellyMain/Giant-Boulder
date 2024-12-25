using UnityEditor;
using UnityEngine;

namespace GeNa.Core
{
    [CustomEditor(typeof(GeNaClearTreesExtension))]
    public class GeNaClearTreesExtensionEditor : GeNaSplineExtensionEditor
    {
        private GeNaClearTreesExtension m_extension;
        private bool m_isAsset;

        protected void OnEnable()
        {
            if (m_editorUtils == null)
                m_editorUtils = PWApp.GetEditorUtils(this, "GeNaSplineExtensionEditor");
            m_extension = target as GeNaClearTreesExtension;
            if (m_extension == null)
                return;
            m_isAsset = AssetDatabase.Contains(m_extension);
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (!GeNaEditorUtility.ValidateComputeShader())
            {
                Color guiColor = GUI.backgroundColor;
                GUI.backgroundColor = Color.red;
                EditorGUILayout.BeginVertical(Styles.box);
                m_editorUtils.Text("NoComputeShaderHelp");
                EditorGUILayout.EndVertical();
                GUI.backgroundColor = guiColor;
                GUI.enabled = false;
            }
            if (m_extension == null)
                m_extension = target as GeNaClearTreesExtension;
            EditorGUI.BeginChangeCheck();
            {
                m_extension.Width = m_editorUtils.FloatField("Width", m_extension.Width, HelpEnabled);
                m_extension.Shoulder = m_editorUtils.FloatField("Shoulder", m_extension.Shoulder, HelpEnabled);
                m_extension.ShoulderFalloff = m_editorUtils.CurveField("Shoulder Falloff", m_extension.ShoulderFalloff, HelpEnabled);
                m_editorUtils.Fractal(m_extension.MaskFractal, HelpEnabled);
                if (!m_isAsset)
                {
                    if (m_editorUtils.Button("Clear Trees Btn", HelpEnabled))
                        m_extension.Clear();
                }
            }
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(m_extension);
                AssetDatabase.SaveAssets();
            }
        }
    }
}