//Copyright(c)2020 Procedural Worlds Pty Limited 
using UnityEngine;
using UnityEditor;

namespace GeNa.Core
{
    [CustomEditor(typeof(GeNaRiverExtension))]
    public class GeNaRiverExtensionEditor : GeNaSplineExtensionEditor
    {
        private GeNaRiverExtension m_extension;
        private bool m_isAsset;

        protected Editor m_riverProfileEditor;

        protected void OnEnable()
        {
            if (m_editorUtils == null)
                m_editorUtils = PWApp.GetEditorUtils(this, "GeNaSplineExtensionEditor");
            m_extension = target as GeNaRiverExtension;
            m_isAsset = AssetDatabase.Contains(m_extension);
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (m_extension == null)
                m_extension = target as GeNaRiverExtension;

            EditorGUI.BeginChangeCheck();
            {
                bool defaultGUIEnabled = GUI.enabled;
                EditorGUILayout.BeginHorizontal();
                m_editorUtils.LabelField("Tag", GUILayout.MaxWidth(40));
                m_extension.Tag = EditorGUILayout.TagField(m_extension.Tag);
                m_editorUtils.LabelField("Layer", GUILayout.MaxWidth(40));
                m_extension.Layer = EditorGUILayout.LayerField(m_extension.Layer);
                EditorGUILayout.EndHorizontal();
                m_editorUtils.InlineHelp("TagAndLayerHelp", HelpEnabled);
                Constants.RenderPipeline pipeline = GeNaUtility.GetActivePipeline();
                if (pipeline != Constants.RenderPipeline.BuiltIn)
                {
                    EditorGUILayout.BeginHorizontal();
                    m_extension.CastShadows = m_editorUtils.Toggle("CastShadows", m_extension.CastShadows);
                    m_extension.ReceiveShadows = m_editorUtils.Toggle("ReceiveShadows", m_extension.ReceiveShadows);
                    m_editorUtils.InlineHelp("ShadowsHelp", HelpEnabled);
                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.Space();
                m_editorUtils.Heading("RiverMeshSettings");
                m_editorUtils.InlineHelp("RiverMeshSettings", HelpEnabled);
                EditorGUI.indentLevel++;
                m_extension.StartFlow = m_editorUtils.FloatField("StartDepth", m_extension.StartFlow, HelpEnabled);
                m_extension.CapDistance = m_editorUtils.FloatField("StartCapDistance", m_extension.CapDistance, HelpEnabled);
                m_extension.EndCapDistance = m_editorUtils.FloatField("EndCapDistance", m_extension.EndCapDistance, HelpEnabled);
                m_extension.RiverWidth = m_editorUtils.FloatField("RiverWidth", m_extension.RiverWidth, HelpEnabled);
                m_extension.VertexDistance = m_editorUtils.Slider("VertexDistance", m_extension.VertexDistance, 1.5f, 8.0f, HelpEnabled);
                m_extension.BankOverstep = m_editorUtils.FloatField("BankOverstep", m_extension.BankOverstep, HelpEnabled);
                GeNaRiverProfile riverProfile = m_extension.RiverProfile;
                if (riverProfile != null)
                {
                    GeNaRiverParameters riverParameters = riverProfile.RiverParameters;
                    if (riverParameters != null)
                    {
                        if (riverParameters.m_renderMode == Constants.ProfileRenderMode.PWShader)
                        {
                            m_extension.UseWorldspaceTextureWidth = m_editorUtils.Toggle("Use Worldspace Width Texturing", m_extension.UseWorldspaceTextureWidth, HelpEnabled);
                            GUI.enabled = m_extension.UseWorldspaceTextureWidth;
                            EditorGUI.indentLevel++;
                            m_extension.WorldspaceWidthRepeat = m_editorUtils.Slider("Worldspace Width Repeat", m_extension.WorldspaceWidthRepeat, 0.5f, 50.0f, HelpEnabled);
                            EditorGUI.indentLevel--;
                            GUI.enabled = true;
                        }
                    }
                }

                EditorGUI.indentLevel--;
                EditorGUILayout.Space();
                m_editorUtils.Heading("RiverBehaviourSettings");
                m_editorUtils.InlineHelp("RiverBehaviourSettings", HelpEnabled);
                EditorGUI.indentLevel++;
                if (GeNaUtility.Gaia2Present)
                {
                    m_extension.UseGaiaSeaLevel = m_editorUtils.Toggle("UseSeaLevel", m_extension.UseGaiaSeaLevel, HelpEnabled);
                    GUI.enabled = !m_extension.UseGaiaSeaLevel;
                    EditorGUI.indentLevel++;
                    m_extension.SeaLevel = m_editorUtils.FloatField("SeaLevel", m_extension.SeaLevel, HelpEnabled);
                    EditorGUI.indentLevel--;
                    GUI.enabled = true;
                }
                else
                {
                    m_extension.SeaLevel = m_editorUtils.FloatField("SeaLevel", m_extension.SeaLevel, HelpEnabled);
                }

                m_extension.UpdateOnTerrainChange = m_editorUtils.Toggle("Auto-Update On Terrain Change", m_extension.UpdateOnTerrainChange, HelpEnabled);
                m_extension.RaycastTerrainOnly = m_editorUtils.Toggle("RaycastTerrainOnly", m_extension.RaycastTerrainOnly, HelpEnabled);
                m_extension.AddCollider = m_editorUtils.Toggle("AddCollider", m_extension.AddCollider, HelpEnabled);
                m_extension.SplitAtTerrains = m_editorUtils.Toggle("SplitMeshesAtTerrains", m_extension.SplitAtTerrains, HelpEnabled);
                EditorGUI.indentLevel--;
                EditorGUILayout.Space();
                bool showRiverSettings = true;
                var spline = m_extension.Spline;
                if (spline != null)
                {
                    showRiverSettings |= spline.gameObject.activeInHierarchy;
                }

                GUI.enabled = showRiverSettings;
                {
                    m_editorUtils.Heading("RiverRenderingSettings");
                    m_editorUtils.InlineHelp("RiverRenderingSettings", HelpEnabled);
                    EditorGUI.indentLevel++;
                    EditorGUI.BeginChangeCheck();
                    riverProfile = (GeNaRiverProfile)m_editorUtils.ObjectField("RiverProfile", riverProfile, typeof(GeNaRiverProfile), false, HelpEnabled);
                    if (EditorGUI.EndChangeCheck())
                        m_extension.UpdateMaterial();
                    if (riverProfile != null)
                    {
                        if (m_riverProfileEditor == null)
                            m_riverProfileEditor = CreateEditor(riverProfile);
                        GeNaRiverProfileEditor.SetProfile(riverProfile, (GeNaRiverProfileEditor)m_riverProfileEditor);
                        EditorGUI.BeginChangeCheck();
                        m_riverProfileEditor.OnInspectorGUI();
                        if (EditorGUI.EndChangeCheck())
                            m_extension.UpdateMaterial();
                    }

                    if (!m_isAsset)
                    {
                        if (riverProfile != null)
                        {
                            var riverParameters = riverProfile.RiverParameters;
                            if (riverParameters.m_renderMode == Constants.ProfileRenderMode.RiverFlow)
                            {
                                if (m_editorUtils.Button("Save Flow Texture"))
                                    m_extension.CaptureRiverFlowTexture(true);
                            }
                        }
                    }

                    m_extension.RiverProfile = riverProfile;

                    EditorGUI.indentLevel--;
                }
                GUI.enabled = defaultGUIEnabled;
                if (!m_isAsset)
                {
                    if (m_editorUtils.Button("MakeSplineDownhill", HelpEnabled))
                        m_extension.SetSplineToDownhill();
                    if (m_editorUtils.Button("BakeRiver", HelpEnabled))
                    {
                        if (EditorUtility.DisplayDialog(m_editorUtils.GetTextValue("BakeTitleRiver"), m_editorUtils.GetTextValue("BakeMessageRiver"), "Ok"))
                        {
                            m_extension.Bake(true);
                        }
                        GUIUtility.ExitGUI();
                    }
                    if (m_extension.HasBakedRivers())
                    {
                        EditorGUILayout.Space(3);
                        if (m_editorUtils.Button("DeleteBakedRiver", HelpEnabled))
                        {
                            if (EditorUtility.DisplayDialog(m_editorUtils.GetTextValue("DeleteBakeTitleRiver"), m_editorUtils.GetTextValue("DeleteBakeMessageRiver"), "Ok", "Cancel"))
                            {
                                m_extension.DeleteBakedRiver(true);
                            }
                            GUIUtility.ExitGUI();
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