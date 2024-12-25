using UnityEngine;
using UnityEditor;
using PWCommon5;
namespace GeNa.Core
{
    [CustomEditor(typeof(RoadProfile))]
    public class RoadProfileEditor : GeNaRoadProfileEditor
    {
        private string[] statList =
        {
            "PW Shader",
            "Material",
            // Shouldn't exist for roads
            // "RiverFlow"
        };
        private EditorUtils m_editorUtils;
        protected override void OnEnable()
        {
            if (m_editorUtils == null)
            {
                // Get editor utils for this
                m_editorUtils = PWApp.GetEditorUtils(this, "GeNaRoadProfileEditor", null);
            }
        }
        public override void OnInspectorGUI()
        {
            m_editorUtils.Initialize();
            if (m_profile == null)
            {
                m_profile = (RoadProfile)target;
            }
            m_editorUtils.Panel("ProfileSettings", ProfilePanel, false);
        }
        private void ProfilePanel(bool helpEnabled)
        {
            if (m_profile == null)
                return;
            EditorGUI.BeginChangeCheck();
            var roadParameters = m_profile.RoadParameters;
            Constants.RenderPipeline pipeline = GeNaUtility.GetActivePipeline();
            roadParameters.m_renderMode = (Constants.ProfileRenderMode)EditorGUILayout.Popup("Render Mode", (int)roadParameters.m_renderMode, statList);
            m_editorUtils.InlineHelp("RenderMode", helpEnabled);
            if (roadParameters.m_renderMode != Constants.ProfileRenderMode.RiverFlow)
            {
                m_editorUtils.Heading("RoadShadersSettings");
                m_editorUtils.InlineHelp("RoadShadersSettings", helpEnabled);
                switch (roadParameters.m_renderMode)
                {
                    case Constants.ProfileRenderMode.PWShader:
                        roadParameters.BuiltInRoadShader = (Shader)m_editorUtils.ObjectField("RoadBuilt-InShader", roadParameters.BuiltInRoadShader, typeof(Shader), false, helpEnabled);
                        roadParameters.UniversalRoadShader = (Shader)m_editorUtils.ObjectField("RoadUniversalShader", roadParameters.UniversalRoadShader, typeof(Shader), false, helpEnabled);
                        roadParameters.HighDefinitionRoadShader = (Shader)m_editorUtils.ObjectField("RoadHighDefinitionShader", roadParameters.HighDefinitionRoadShader, typeof(Shader), false, helpEnabled);
                        break;
                    case Constants.ProfileRenderMode.Material:
                        roadParameters.m_builtInRoadMaterial = (Material)m_editorUtils.ObjectField("RoadBuilt-InMaterial", roadParameters.m_builtInRoadMaterial, typeof(Material), false, helpEnabled);
                        roadParameters.m_universalRoadMaterial = (Material)m_editorUtils.ObjectField("RoadUniversalMaterial", roadParameters.m_universalRoadMaterial, typeof(Material), false, helpEnabled);
                        roadParameters.m_highDefinitionRoadMaterial = (Material)m_editorUtils.ObjectField("RoadHighDefinitionMaterial", roadParameters.m_highDefinitionRoadMaterial, typeof(Material), false, helpEnabled);
                        break;
                }
                m_editorUtils.Heading("IntersectionShadersSettings");
                m_editorUtils.InlineHelp("IntersectionShadersSettings", helpEnabled);
                switch (roadParameters.m_renderMode)
                {
                    case Constants.ProfileRenderMode.PWShader:
                        roadParameters.BuiltInIntersectionRoadShader = (Shader)m_editorUtils.ObjectField("IntersectionBuilt-InShader", roadParameters.BuiltInIntersectionRoadShader, typeof(Shader), false, helpEnabled);
                        roadParameters.UniversalIntersectionRoadShader = (Shader)m_editorUtils.ObjectField("IntersectionUniversalShader", roadParameters.UniversalIntersectionRoadShader, typeof(Shader), false, helpEnabled);
                        roadParameters.HighDefinitionIntersectionRoadShader = (Shader)m_editorUtils.ObjectField("IntersectionHighDefinitionShader", roadParameters.HighDefinitionIntersectionRoadShader, typeof(Shader), false, helpEnabled);
                        break;
                    case Constants.ProfileRenderMode.Material:
                        roadParameters.m_builtInIntersectionMaterial = (Material)m_editorUtils.ObjectField("IntersectionBuilt-InMaterial", roadParameters.m_builtInIntersectionMaterial, typeof(Material), false, helpEnabled);
                        roadParameters.m_universalIntersectionMaterial = (Material)m_editorUtils.ObjectField("IntersectionUniversalMaterial", roadParameters.m_universalIntersectionMaterial, typeof(Material), false, helpEnabled);
                        roadParameters.m_highDefinitionIntersectionMaterial = (Material)m_editorUtils.ObjectField("IntersectionHighDefinitionMaterial", roadParameters.m_highDefinitionIntersectionMaterial, typeof(Material), false, helpEnabled);
                        break;
                }
                if (roadParameters.m_renderMode == Constants.ProfileRenderMode.PWShader)
                {
                    EditorGUILayout.Space();
                    m_editorUtils.Heading("AlbedoSettings");
                    m_editorUtils.InlineHelp("AlbedoSettings", helpEnabled);
                    m_editorUtils.LabelField("Road");
                    EditorGUI.indentLevel++;
                    roadParameters.m_roadAlbedoMap = (Texture2D)m_editorUtils.ObjectField("AlbedoMap", roadParameters.m_roadAlbedoMap, typeof(Texture2D), false, helpEnabled, GUILayout.MaxHeight(16f));
                    roadParameters.m_roadTintColor = m_editorUtils.ColorField("TintColor", roadParameters.m_roadTintColor, helpEnabled);
                    EditorGUI.indentLevel--;
                    m_editorUtils.LabelField("Intersection");
                    EditorGUI.indentLevel++;
                    roadParameters.m_intersectionAlbedoMap = (Texture2D)m_editorUtils.ObjectField("AlbedoMap", roadParameters.m_intersectionAlbedoMap, typeof(Texture2D), false, helpEnabled, GUILayout.MaxHeight(16f));
                    roadParameters.m_intersectionTintColor = m_editorUtils.ColorField("TintColor", roadParameters.m_intersectionTintColor, helpEnabled);
                    EditorGUI.indentLevel--;
                    EditorGUILayout.Space();
                    m_editorUtils.Heading("NormalSettings");
                    m_editorUtils.InlineHelp("NormalSettings", helpEnabled);
                    m_editorUtils.LabelField("Road");
                    EditorGUI.indentLevel++;
                    roadParameters.m_roadNormalMap = (Texture2D)m_editorUtils.ObjectField("NormalMap", roadParameters.m_roadNormalMap, typeof(Texture2D), false, helpEnabled, GUILayout.MaxHeight(16f));
                    roadParameters.m_roadNormalStrength = m_editorUtils.Slider("NormalStrength", roadParameters.m_roadNormalStrength, 0f, 5f, helpEnabled);
                    EditorGUI.indentLevel--;
                    m_editorUtils.LabelField("Intersection");
                    EditorGUI.indentLevel++;
                    roadParameters.m_intersectionNormalMap = (Texture2D)m_editorUtils.ObjectField("NormalMap", roadParameters.m_intersectionNormalMap, typeof(Texture2D), false, helpEnabled, GUILayout.MaxHeight(16f));
                    roadParameters.m_intersectionNormalStrength = m_editorUtils.Slider("NormalStrength", roadParameters.m_intersectionNormalStrength, 0f, 5f, helpEnabled);
                    EditorGUI.indentLevel--;
                    EditorGUILayout.Space();
                    if (pipeline != Constants.RenderPipeline.HighDefinition)
                    {
                        bool displayOldSettings = true;

                        //URP
                        if (pipeline == Constants.RenderPipeline.Universal)
                        {
                            bool customShaderMatch = GeNaRoadShaderID.URPRoadShaderCustom == roadParameters.m_universalRoadShaderName;
                            if (customShaderMatch)
                            {
                                displayOldSettings = false;
                            }
                        }
                        //Built in
                        else
                        {
                            bool customShaderMatch = GeNaRoadShaderID.BuiltInRoadShaderCustom == roadParameters.m_builtInRoadShaderName;
                            if (customShaderMatch)
                            {
                                displayOldSettings = false;
                            }
                        }
                        if (displayOldSettings)
                        {
                            m_editorUtils.Heading("PBRSettings");
                            m_editorUtils.InlineHelp("PBRSettings", helpEnabled);
                            m_editorUtils.LabelField("Road");
                            EditorGUI.indentLevel++;
                            if (roadParameters.m_roadMetallicMap == null)
                            {
                                roadParameters.m_roadMetallicMap = (Texture2D)m_editorUtils.ObjectField("MetallicMap", roadParameters.m_roadMetallicMap, typeof(Texture2D), false, helpEnabled, GUILayout.MaxHeight(16f));
                                roadParameters.m_roadMetallic = m_editorUtils.Slider("Metallic", roadParameters.m_roadMetallic, 0f, 1f, helpEnabled);
                            }
                            else
                            {
                                roadParameters.m_roadMetallicMap = (Texture2D)m_editorUtils.ObjectField("MetallicMap", roadParameters.m_roadMetallicMap, typeof(Texture2D), false, helpEnabled, GUILayout.MaxHeight(16f));
                            }
                            roadParameters.m_roadOcclusionMap = (Texture2D)m_editorUtils.ObjectField("OcclusionMap", roadParameters.m_roadOcclusionMap, typeof(Texture2D), false, helpEnabled, GUILayout.MaxHeight(16f));
                            roadParameters.m_roadOcclusionStrength = m_editorUtils.Slider("OcclusionStrength", roadParameters.m_roadOcclusionStrength, 0f, 1f, helpEnabled);
                            roadParameters.m_roadSmoothness = m_editorUtils.Slider("Smoothness", roadParameters.m_roadSmoothness, 0f, 1f, helpEnabled);
                            EditorGUI.indentLevel--;
                            m_editorUtils.LabelField("Intersection");
                            EditorGUI.indentLevel++;
                            if (roadParameters.m_intersectionMetallicMap == null)
                            {
                                roadParameters.m_intersectionMetallicMap = (Texture2D)m_editorUtils.ObjectField("MetallicMap", roadParameters.m_intersectionMetallicMap, typeof(Texture2D), false, helpEnabled, GUILayout.MaxHeight(16f));
                                roadParameters.m_intersectionMetallic = m_editorUtils.Slider("Metallic", roadParameters.m_intersectionMetallic, 0f, 1f, helpEnabled);
                            }
                            else
                            {
                                roadParameters.m_intersectionMetallicMap = (Texture2D)m_editorUtils.ObjectField("MetallicMap", roadParameters.m_intersectionMetallicMap, typeof(Texture2D), false, helpEnabled, GUILayout.MaxHeight(16f));
                            }
                            roadParameters.m_intersectionOcclusionMap = (Texture2D)m_editorUtils.ObjectField("OcclusionMap", roadParameters.m_intersectionOcclusionMap, typeof(Texture2D), false, helpEnabled, GUILayout.MaxHeight(16f));
                            roadParameters.m_intersectionOcclusionStrength = m_editorUtils.Slider("OcclusionStrength", roadParameters.m_intersectionOcclusionStrength, 0f, 1f, helpEnabled);
                            roadParameters.m_intersectionSmoothness = m_editorUtils.Slider("Smoothness", roadParameters.m_intersectionSmoothness, 0f, 1f, helpEnabled);
                            EditorGUI.indentLevel--;
                            EditorGUILayout.Space();
                            m_editorUtils.Heading("HeightSettings");
                            m_editorUtils.InlineHelp("HeightSettings", helpEnabled);
                            m_editorUtils.LabelField("Road");
                            EditorGUI.indentLevel++;
                            roadParameters.m_roadHeightMap = (Texture2D)m_editorUtils.ObjectField("HeightMap", roadParameters.m_roadHeightMap, typeof(Texture2D), false, helpEnabled, GUILayout.MaxHeight(16f));
                            roadParameters.m_roadHeightStrength = m_editorUtils.Slider("HeightStrength", roadParameters.m_roadHeightStrength, 0f, 1f, helpEnabled);
                            EditorGUI.indentLevel--;
                            m_editorUtils.LabelField("Intersection");
                            EditorGUI.indentLevel++;
                            roadParameters.m_intersectionHeightMap = (Texture2D)m_editorUtils.ObjectField("HeightMap", roadParameters.m_intersectionHeightMap, typeof(Texture2D), false, helpEnabled, GUILayout.MaxHeight(16f));
                            roadParameters.m_intersectionHeightStrength = m_editorUtils.Slider("HeightStrength", roadParameters.m_intersectionHeightStrength, 0f, 1f, helpEnabled);
                            EditorGUI.indentLevel--;
                            EditorGUILayout.Space();
                        }
                    }
                    else
                    {
                        m_editorUtils.Heading("MaskMapSettings");
                        m_editorUtils.InlineHelp("MaskMapSettings", helpEnabled);
                        m_editorUtils.LabelField("Road");
                        EditorGUI.indentLevel++;
                        roadParameters.m_roadMaskMap = (Texture2D)m_editorUtils.ObjectField("MaskMap", roadParameters.m_roadMaskMap, typeof(Texture2D), false, helpEnabled, GUILayout.MaxHeight(16f));
                        EditorGUI.indentLevel--;
                        EditorGUI.indentLevel--;
                        EditorGUILayout.Space();
                        EditorGUI.indentLevel++;
                        m_editorUtils.LabelField("Intersection");
                        EditorGUI.indentLevel++;
                        roadParameters.m_intersectionMaskMap = (Texture2D)m_editorUtils.ObjectField("MaskMap", roadParameters.m_intersectionMaskMap, typeof(Texture2D), false, helpEnabled, GUILayout.MaxHeight(16f));
                        EditorGUI.indentLevel--;
                        EditorGUILayout.Space();
                    }
                    if (pipeline != Constants.RenderPipeline.HighDefinition)
                    {
                        m_editorUtils.Heading("MaskMapSettings");
                        m_editorUtils.InlineHelp("MaskMapSettings", helpEnabled);
                        m_editorUtils.LabelField("Road");
                        EditorGUI.indentLevel++;
                        roadParameters.m_roadMaskMap = (Texture2D)m_editorUtils.ObjectField("MaskMap", roadParameters.m_roadMaskMap, typeof(Texture2D), false, helpEnabled, GUILayout.MaxHeight(16f));
                        EditorGUI.indentLevel--;
                        EditorGUILayout.Space();
                        m_editorUtils.LabelField("Intersection");
                        EditorGUI.indentLevel++;
                        roadParameters.m_intersectionMaskMap = (Texture2D)m_editorUtils.ObjectField("MaskMap", roadParameters.m_intersectionMaskMap, typeof(Texture2D), false, helpEnabled, GUILayout.MaxHeight(16f));
                        EditorGUI.indentLevel--;
                        EditorGUILayout.Space();
                    }
                    bool displayNewSettings = false;
                    switch (pipeline)
                    {
                        case Constants.RenderPipeline.BuiltIn:
                            if (roadParameters.m_builtInRoadShaderName != null)
                            {
                                displayNewSettings = (GeNaRoadShaderID.BuiltInRoadShaderCustom == roadParameters.m_builtInRoadShaderName);
                            }
                            break;
                        case Constants.RenderPipeline.Universal:
                            if (roadParameters.m_universalRoadShaderName != null)
                            {
                                displayNewSettings = (GeNaRoadShaderID.URPRoadShaderCustom == roadParameters.m_universalRoadShaderName);
                            }
                            break;
                        case Constants.RenderPipeline.HighDefinition:
                            if (roadParameters.m_highDefinitionRoadShaderName != null)
                            {
                                displayNewSettings = (GeNaRoadShaderID.HDRPRoadShaderCustom == roadParameters.m_highDefinitionRoadShaderName);
                            }
                            break;
                    }
                    if (displayNewSettings)
                    {
                        //Mask Map Parameters
                        m_editorUtils.LabelField("Mask Parameters");
                        roadParameters.m_maskMapMetallicEnabled = EditorUtilsExtensions.BeginOverrideToggle(roadParameters.m_maskMapMetallicEnabled,
                            () => GUI.changed = true,
                            () =>
                            {
                                float result = m_editorUtils.Slider("(R) - Metallic", roadParameters.m_maskMapMetallicEnabled ? roadParameters.m_maskMapMetallic : 0.01f, 0.01f, 2f);
                                if (roadParameters.m_maskMapMetallicEnabled)
                                {
                                    roadParameters.m_maskMapMetallic = result;
                                }
                            });
                        roadParameters.m_maskMapAOEnabled = EditorUtilsExtensions.BeginOverrideToggle(roadParameters.m_maskMapAOEnabled,
                            () => GUI.changed = true,
                            () =>
                            {
                                var result = m_editorUtils.Slider("(G) - AO", roadParameters.m_maskMapAOEnabled ? roadParameters.m_maskMapAO : 1f, 0.01f, 2f, helpEnabled);
                                if (roadParameters.m_maskMapAOEnabled)
                                {
                                    roadParameters.m_maskMapAO = result;
                                }
                            });
                        if (pipeline != Constants.RenderPipeline.BuiltIn)
                        {
                            roadParameters.m_maskMapHeightEnabled = EditorUtilsExtensions.BeginOverrideToggle(roadParameters.m_maskMapHeightEnabled,
                                () => GUI.changed = true,
                                () =>
                                {
                                    float result = m_editorUtils.Slider("(B) - Height", roadParameters.m_maskMapHeightEnabled ? roadParameters.m_maskMapHeight : 1f, 0.01f, 2f, helpEnabled);
                                    if (roadParameters.m_maskMapHeightEnabled)
                                    {
                                        roadParameters.m_maskMapHeight = result;
                                    }
                                });
                        }
                        roadParameters.m_maskMapSmoothnessEnabled = EditorUtilsExtensions.BeginOverrideToggle(roadParameters.m_maskMapSmoothnessEnabled,
                            () => GUI.changed = true,
                            () =>
                            {
                                float result = m_editorUtils.Slider("(A) - Smoothness", roadParameters.m_maskMapSmoothnessEnabled ? roadParameters.m_maskMapSmoothness : 1f, 0.01f, 2f, helpEnabled);
                                if (roadParameters.m_maskMapSmoothnessEnabled)
                                {
                                    roadParameters.m_maskMapSmoothness = result;
                                }
                            });
                        EditorGUILayout.Space();
                        if (pipeline != Constants.RenderPipeline.BuiltIn)
                        {
                            m_editorUtils.Heading("Tiling");
                            m_editorUtils.InlineHelp("Tiling", helpEnabled);
                            roadParameters.m_scaleFromCenter = m_editorUtils.Toggle("Scale From Center", roadParameters.m_scaleFromCenter, helpEnabled);
                            roadParameters.m_uvTiling = m_editorUtils.Vector2Field("UV Tiling", roadParameters.m_uvTiling, helpEnabled);
                            EditorGUILayout.Space();
                            m_editorUtils.Heading("Edge Blending");
                            m_editorUtils.InlineHelp("Edge Blending", helpEnabled);
                            roadParameters.m_edgeBlend = m_editorUtils.Slider("Edge Distance", 1 - roadParameters.m_edgeBlend, 0f, 1f, helpEnabled);
                            roadParameters.m_edgeBlend = 1 - roadParameters.m_edgeBlend;
                            roadParameters.m_edgeBlendPower = m_editorUtils.Slider("Edge Contrast", roadParameters.m_edgeBlendPower, 1f, 60f, helpEnabled);
                            EditorGUILayout.Space();
                            m_editorUtils.Heading("Road Pattern");
                            m_editorUtils.InlineHelp("Road Pattern", helpEnabled);
                            roadParameters.m_doubleTrackRoads = m_editorUtils.Toggle("Double Track Roads", roadParameters.m_doubleTrackRoads, helpEnabled);
                            if (roadParameters.m_doubleTrackRoads)
                            {
                                roadParameters.m_roadShape = 1f;
                                roadParameters.m_doubleTrackPosition = m_editorUtils.Slider("Double Track Position", roadParameters.m_doubleTrackPosition, 1f, 30f, helpEnabled);
                                roadParameters.m_doubleTrackRange = m_editorUtils.Slider("Double Track Range", roadParameters.m_doubleTrackRange, 1f, 5f, helpEnabled);
                            }
                            else
                            {
                                roadParameters.m_roadShape = 0f;
                                roadParameters.m_middleTrackPosition = m_editorUtils.Slider("Middle Track Position", roadParameters.m_middleTrackPosition, 0f, 1f, helpEnabled);
                                roadParameters.m_middleTrackRange = m_editorUtils.Slider("Middle Track Range", roadParameters.m_middleTrackRange, 1f, 20f, helpEnabled);
                            }
                            EditorGUILayout.Space();
                            m_editorUtils.Heading("Height Adjustments");
                            m_editorUtils.InlineHelp("Height Adjustments", helpEnabled);
                            roadParameters.m_heightContrast = m_editorUtils.Slider("Height Contrast", roadParameters.m_heightContrast, 0f, 1f, helpEnabled);
                            roadParameters.m_heightTransition = m_editorUtils.Slider("Height Transition", roadParameters.m_heightTransition, 0f, 5f, helpEnabled);
                            EditorGUILayout.Space();
                            m_editorUtils.Heading("Ground Blending");
                            m_editorUtils.InlineHelp("Ground Blending", helpEnabled);
                            roadParameters.m_blendWithGround = m_editorUtils.Slider("Blend With Ground", roadParameters.m_blendWithGround, 0f, 1f, helpEnabled);
                            roadParameters.m_inGroundPush = m_editorUtils.Slider("Ground Height", roadParameters.m_inGroundPush, 0f, 1f, helpEnabled);
                        }
                        EditorGUILayout.Space();
                        m_editorUtils.Heading("Distance Offset");
                        m_editorUtils.InlineHelp("Distance Offset", helpEnabled);
                        roadParameters.m_terrainLODOffset = m_editorUtils.Slider("Terrain LOD Offset", roadParameters.m_terrainLODOffset, 0f, 5f, helpEnabled);
                        roadParameters.m_terrainLODDistance = m_editorUtils.FloatField("Terrain LOD Distance", roadParameters.m_terrainLODDistance, helpEnabled);
                        if (pipeline != Constants.RenderPipeline.BuiltIn)
                        {
                            m_editorUtils.Heading("Noise");
                            m_editorUtils.InlineHelp("Noise", helpEnabled);
                            m_editorUtils.Fractal(roadParameters.m_noise, helpEnabled, false);
                        }
                    }
                }
            }
            else
            {
                EditorGUILayout.LabelField("River Profile Mode disabled for Road Profiles!", EditorStyles.boldLabel);
            }
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(m_profile);
            }
        }
    }
}