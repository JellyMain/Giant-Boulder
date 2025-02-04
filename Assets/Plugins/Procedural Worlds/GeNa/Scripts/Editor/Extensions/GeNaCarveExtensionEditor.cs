using UnityEditor;
using UnityEngine;

namespace GeNa.Core
{
    [CustomEditor(typeof(GeNaCarveExtension))]
    public class GeNaCarveExtensionEditor : GeNaSplineExtensionEditor
    {
        private GeNaCarveExtension m_extension;
        private bool m_isAsset;

        protected void OnEnable()
        {
            if (m_editorUtils == null)
                m_editorUtils = PWApp.GetEditorUtils(this, "GeNaSplineExtensionEditor");

            m_extension = target as GeNaCarveExtension;
            if (m_extension != null)
            {
                m_isAsset = AssetDatabase.Contains(m_extension);
            }
        }

        public override void OnInspectorGUI()
        {
            Initialize();
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
                m_extension = target as GeNaCarveExtension;
            EditorGUI.BeginChangeCheck();
            {
                m_extension.Width = m_editorUtils.FloatField("Width", m_extension.Width, HelpEnabled);
                if (m_extension.Width < 0.05f)
                {
                    m_extension.Width = 0.05f;
                }

                m_extension.HeightOffset =
                    m_editorUtils.FloatField("Height Offset", m_extension.HeightOffset, HelpEnabled);
                m_extension.Shoulder = m_editorUtils.FloatField("Shoulder", m_extension.Shoulder, HelpEnabled);
                m_extension.ShoulderFalloff =
                    m_editorUtils.CurveField("Shoulder Falloff", m_extension.ShoulderFalloff, HelpEnabled);
                m_extension.RoadLike = m_editorUtils.Toggle("Road Like", m_extension.RoadLike, HelpEnabled);
                m_editorUtils.Fractal(m_extension.MaskFractal, HelpEnabled);
                m_extension.ShowPreview = m_editorUtils.Toggle("Preview Btn", m_extension.ShowPreview, HelpEnabled);
                if (!m_isAsset)
                {
                    if (m_editorUtils.Button("Carve Btn", HelpEnabled))
                        m_extension.Carve();
                    GeNaSpline spline = m_extension.Spline;
                    if (spline != null)
                    {
                        if (spline.Settings.Advanced.DebuggingEnabled)
                        {
                            if (GUILayout.Button("Output Debugging"))
                            {
                                TerrainEntity terrainEntity = m_extension.GetTerrainEntity();
                                if (terrainEntity != null)
                                {
                                    GeNaDebug.SaveTextureToDisk(terrainEntity.InputDisplacement, "Input Displacement");
                                    GeNaDebug.SaveTextureToDisk(terrainEntity.OutputDisplacement, "Output Displacement");

                                    string texturePath = GeNaDebug.TEXTURE_PATH;

                                    GeNaDebug.Log($"Debug Textures Saved to '{texturePath}'.");
                                    AssetDatabase.Refresh();

                                    var textureAssetsPath = texturePath.ToAssetsPath();
                                    Object textureFolder = AssetDatabase.LoadAssetAtPath<Object>(textureAssetsPath);
                                    if (textureFolder != null)
                                    {
                                        EditorGUIUtility.PingObject(textureFolder);
                                    }
                                }
                            }
                        }
                    }
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