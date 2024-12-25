using UnityEditor;
using UnityEngine;

namespace GeNa.Core
{
    [CustomEditor(typeof(GeNaExtrusionExtension))]
    public class GeNaExtrusionExtensionEditor : GeNaSplineExtensionEditor
    {
        private GeNaExtrusionExtension m_extension;
        private bool m_isAsset;

        protected void OnEnable()
        {
            if (m_editorUtils == null)
                m_editorUtils = PWApp.GetEditorUtils(this, "GeNaSplineExtensionEditor");
            m_extension = target as GeNaExtrusionExtension;
            m_isAsset = AssetDatabase.Contains(m_extension);
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (m_extension == null)
                m_extension = target as GeNaExtrusionExtension;
            EditorGUI.BeginChangeCheck();
            {
                m_extension.SharedMaterial = (Material)m_editorUtils.ObjectField("Extrusion Material", m_extension.SharedMaterial, typeof(Material), true, HelpEnabled);
                m_extension.Smoothness = m_editorUtils.Slider("Mesh Smoothness", m_extension.Smoothness, 1f, 5f, HelpEnabled);
                m_extension.Width = m_editorUtils.FloatField("Mesh Width", m_extension.Width, HelpEnabled);
                m_extension.HeightOffset = m_editorUtils.FloatField("Mesh Height Offset", m_extension.HeightOffset, HelpEnabled);
                m_extension.SnapToGround = m_editorUtils.Toggle("Mesh Snap to Terrain", m_extension.SnapToGround, HelpEnabled);
                m_extension.Curve = m_editorUtils.CurveField("Extrusion", m_extension.Curve, HelpEnabled);
                m_extension.SplitAtTerrains = m_editorUtils.Toggle("SplitMeshesAtTerrains", m_extension.SplitAtTerrains, HelpEnabled);
                if (!m_isAsset)
                {
                    if (m_editorUtils.Button("BakeExtrusion"))
                        m_extension.Bake();
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