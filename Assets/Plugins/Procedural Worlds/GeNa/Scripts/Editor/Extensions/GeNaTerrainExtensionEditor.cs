using UnityEditor;
using UnityEngine;
namespace GeNa.Core
{
    [CustomEditor(typeof(GeNaTerrainExtension))]
    public class GeNaTerrainExtensionEditor : GeNaSplineExtensionEditor
    {
        private GeNaTerrainExtension m_extension;
        private bool m_isAsset;
        
        protected void OnEnable()
        {
            if (m_editorUtils == null)
                m_editorUtils = PWApp.GetEditorUtils(this, "GeNaSplineExtensionEditor");
            m_extension = target as GeNaTerrainExtension;

            if (m_extension != null)
            {
                m_isAsset = AssetDatabase.Contains(m_extension);
            }
        }
        public void RenderPanel()
        {
            m_extension = target as GeNaTerrainExtension;
            if (m_extension == null)
                return;
            EditorGUI.BeginChangeCheck();
            {
                m_extension.Width = m_editorUtils.FloatField("Width", m_extension.Width, HelpEnabled);
                switch (m_extension.EffectType)
                {
                    case EffectType.Raise:
                    case EffectType.Lower:
                    case EffectType.Flatten:
                        m_extension.HeightOffset = m_editorUtils.FloatField("Height Offset", m_extension.HeightOffset, HelpEnabled);
                        break;
                }
                m_extension.Strength = m_editorUtils.Slider("Strength", m_extension.Strength, 0f, 1f, HelpEnabled);
                m_extension.Shoulder = m_editorUtils.FloatField("Shoulder", m_extension.Shoulder, HelpEnabled);
                m_extension.ShoulderFalloff = m_editorUtils.CurveField("Shoulder Falloff", m_extension.ShoulderFalloff, HelpEnabled);
                m_editorUtils.Fractal(m_extension.MaskFractal, HelpEnabled);
                if (!m_isAsset)
                {
                    if (GUILayout.Button(m_extension.EffectType.ToString()))
                        m_extension.Clear();
                }
            }
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(m_extension);
                AssetDatabase.SaveAssets();
            }
        }
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            m_extension = target as GeNaTerrainExtension;
            if (m_extension == null)
                return;
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
            Terrain terrain = Terrain.activeTerrain;
            if (terrain != null)
            {
                TerrainData terrainData = terrain.terrainData;
                if (terrainData != null)
                {
                    m_extension.EffectType = (EffectType)m_editorUtils.EnumPopup("Effect Type", m_extension.EffectType, HelpEnabled);
                    switch (m_extension.EffectType)
                    {
                        case EffectType.Raise:
                        case EffectType.Lower:
                        case EffectType.Flatten:
                        case EffectType.ClearTrees:
                        case EffectType.ClearDetails:
                            RenderPanel();
                            break;
                        case EffectType.Texture:
                            TerrainLayer[] terrainLayers = terrainData.terrainLayers;
                            if (terrainLayers.Length > 0)
                            {
                                int alphamapLayers = terrainData.alphamapLayers;
                                GUIContent[] choices = new GUIContent[alphamapLayers];
                                for (int assetIdx = 0; assetIdx < choices.Length; assetIdx++)
                                {
                                    TerrainLayer terrainLayer = terrainLayers[assetIdx];
                                    var diffuseTexture = terrainLayer.diffuseTexture;
                                    var normalMapTexture = terrainLayer.normalMapTexture;
                                    var maskMapTexture = terrainLayer.maskMapTexture;
                                    string name = "Unknown Asset";
                                    if (diffuseTexture != null)
                                        name = diffuseTexture.name;
                                    else if (normalMapTexture != null)
                                        name = normalMapTexture.name;
                                    else if (maskMapTexture != null)
                                        name = maskMapTexture.name;
                                    if (terrainLayer.diffuseTexture != null)
                                        name = terrainLayer.diffuseTexture.name;
                                    choices[assetIdx] = new GUIContent(name);
                                }
                                m_extension.TextureProtoIndex = m_editorUtils.Popup("Texture", m_extension.TextureProtoIndex, choices, HelpEnabled);
                                RenderPanel();
                            }
                            else
                            {
                                m_editorUtils.Label("Missing Terrain Layers", HelpEnabled);
                            }
                            break;
                        case EffectType.Detail:
                            DetailPrototype[] detailPrototypes = terrainData.detailPrototypes;
                            if (detailPrototypes.Length > 0)
                            {
                                GUIContent[] choices = new GUIContent[detailPrototypes.Length];
                                for (int assetIdx = 0; assetIdx < choices.Length; assetIdx++)
                                {
                                    DetailPrototype detailProto = detailPrototypes[assetIdx];
                                    var prefab = detailProto.prototype;
                                    var texture = detailProto.prototypeTexture;
                                    string name = "Unknown Asset";
                                    if (prefab != null)
                                        name = prefab.name;
                                    else if (texture != null)
                                        name = texture.name;
                                    choices[assetIdx] = new GUIContent(name);
                                }
                                m_extension.DetailProtoIndex = m_editorUtils.Popup("Details", m_extension.DetailProtoIndex, choices, HelpEnabled);
                                RenderPanel();
                            }
                            else
                            {
                                m_editorUtils.Label("Missing Terrain Details", HelpEnabled);
                            }
                            break;
                    }
                }
                else
                {
                    m_editorUtils.Label("Missing TerrainData", HelpEnabled);
                }
            }
            else
            {
                m_editorUtils.Label("Missing Terrain", HelpEnabled);
            }
        }
    }
}