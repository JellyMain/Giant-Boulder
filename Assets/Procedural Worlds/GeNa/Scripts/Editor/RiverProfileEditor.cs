using UnityEngine;
using UnityEditor;
using PWCommon5;
namespace GeNa.Core
{
    [CustomEditor(typeof(RiverProfile))]
    public class RiverProfileEditor : GeNaRiverProfileEditor
    { 
        private string[] statList =
        {
            "PW Shader (Legacy)",
            "Material",
            "RiverFlow"
        };
        private EditorUtils m_editorUtils;
        protected override void OnEnable()
        {
            if (m_editorUtils == null)
            {
                // Get editor utils for this
                m_editorUtils = PWApp.GetEditorUtils(this, "GeNaRiverProfileEditor", null);
            }
        }
        public override void OnInspectorGUI()
        {
            m_editorUtils.Initialize();
            if (m_profile == null)
            {
                m_profile = (RiverProfile)target;
            }
            m_editorUtils.Panel("ProfileSettings", ProfilePanel, false);
        }
        private void ProfilePanel(bool helpEnabled)
        {
            if (m_profile == null)
                return;
            EditorGUI.BeginChangeCheck();
            m_profile.RiverParameters.m_renderMode = (Constants.ProfileRenderMode)EditorGUILayout.Popup("Render Mode", (int)m_profile.RiverParameters.m_renderMode, statList);
            m_editorUtils.InlineHelp("RenderMode", helpEnabled);
            if (m_profile.RiverParameters.m_renderMode == Constants.ProfileRenderMode.PWShader)
            {
                Constants.RenderPipeline pipeline = GeNaUtility.GetActivePipeline();
                if (pipeline != Constants.RenderPipeline.BuiltIn)
                {
                    EditorGUILayout.HelpBox(m_editorUtils.GetTextValue("SRPShaderModeHelp"), MessageType.Warning);
                }
                else
                {
                    m_editorUtils.Heading("WeatherSettings");
                    m_editorUtils.InlineHelp("WeatherSettings", helpEnabled);
                    m_profile.RiverParameters.m_syncToWeather = m_editorUtils.Toggle("SyncToWeather", m_profile.RiverParameters.m_syncToWeather, helpEnabled);
                    m_profile.RiverParameters.m_isUsedForWeather = m_profile.RiverParameters.m_syncToWeather;
                    if (m_profile.RiverParameters.m_isUsedForWeather)
                    {
                        EditorGUI.indentLevel++;
                        m_profile.WeatherParameters.m_rainRiverProfile = (GeNaRiverProfile)m_editorUtils.ObjectField("RainRiverProfile", m_profile.WeatherParameters.m_rainRiverProfile, typeof(GeNaRiverProfile), false, helpEnabled);
                        m_profile.WeatherParameters.m_snowRiverProfile = (GeNaRiverProfile)m_editorUtils.ObjectField("SnowRiverProfile", m_profile.WeatherParameters.m_snowRiverProfile, typeof(GeNaRiverProfile), false, helpEnabled);
                        EditorGUI.indentLevel--;
                    }
                    EditorGUILayout.Space();
                }
                m_editorUtils.Heading("ShadersSettings");
                m_editorUtils.InlineHelp("ShadersSettings", helpEnabled);
                m_profile.RiverParameters.BuiltInRiverShader = (Shader)m_editorUtils.ObjectField("Built-InShader", m_profile.RiverParameters.BuiltInRiverShader, typeof(Shader), false, helpEnabled);
                m_profile.RiverParameters.UniversalRiverShader = (Shader)m_editorUtils.ObjectField("UniversalShader", m_profile.RiverParameters.UniversalRiverShader, typeof(Shader), false, helpEnabled);
                m_profile.RiverParameters.HighDefinitionRiverShader = (Shader)m_editorUtils.ObjectField("HighDefinitionShader", m_profile.RiverParameters.HighDefinitionRiverShader, typeof(Shader), false, helpEnabled);
                EditorGUILayout.Space();
                switch (pipeline)
                {
                    case Constants.RenderPipeline.BuiltIn:
                    {
                        m_editorUtils.Heading("ColorSettings");
                        m_editorUtils.InlineHelp("ColorSettings", helpEnabled);
                        m_profile.RiverParameters.m_mainColor = m_editorUtils.ColorField("AlbedoColor", m_profile.RiverParameters.m_mainColor, helpEnabled);
                        m_profile.RiverParameters.m_mainColorDepthStrength = m_editorUtils.Slider("AlbedoDepthStrength", m_profile.RiverParameters.m_mainColorDepthStrength, 0f, 1f, helpEnabled);
                        m_profile.RiverParameters.m_tintColor = m_editorUtils.ColorField("TintColor", m_profile.RiverParameters.m_tintColor, helpEnabled);
                        m_profile.RiverParameters.m_tintStrength = m_editorUtils.Slider("TintStrength", m_profile.RiverParameters.m_tintStrength, 0f, 1f, helpEnabled);
                        EditorGUILayout.Space();
                        m_editorUtils.Heading("PBRSettings");
                        m_editorUtils.InlineHelp("PBRSettings", helpEnabled);
                        m_profile.RiverParameters.m_smoothness = m_editorUtils.Slider("Smoothness", m_profile.RiverParameters.m_smoothness, 0f, 1f, helpEnabled);
                        m_profile.RiverParameters.m_specularColor = m_editorUtils.ColorField("SpecularColor", m_profile.RiverParameters.m_specularColor, helpEnabled);
                        EditorGUILayout.Space();
                        m_editorUtils.Heading("FlowSettings");
                        m_editorUtils.InlineHelp("FlowSettings", helpEnabled);
                        m_profile.RiverParameters.m_speed = m_editorUtils.Slider("Speed", m_profile.RiverParameters.m_speed, 0f, 2f, helpEnabled);
                        m_profile.RiverParameters.m_normalShift = m_editorUtils.Slider("NormalShift", m_profile.RiverParameters.m_normalShift, 0f, 0.5f, helpEnabled);
                        EditorGUILayout.Space();
                        m_editorUtils.Heading("BlendSettings");
                        m_editorUtils.InlineHelp("BlendSettings", helpEnabled);
                        m_profile.RiverParameters.m_shoreBlend = m_editorUtils.Slider("ShoreBlend", m_profile.RiverParameters.m_shoreBlend, 0f, 1f, helpEnabled);
                        m_profile.RiverParameters.m_shoreNormalBlend = m_editorUtils.Slider("ShoreNormalBlend", m_profile.RiverParameters.m_shoreNormalBlend, 0f, 1f, helpEnabled);
                        EditorGUILayout.Space();
                        m_editorUtils.Heading("NormalAndHeightSettings");
                        m_editorUtils.InlineHelp("NormalAndHeightSettings", helpEnabled);
                        m_editorUtils.LabelField("NormalAndHeightMap");
                        EditorGUILayout.BeginHorizontal();
                        m_profile.RiverParameters.m_normalAndHeightMapTiling = EditorGUILayout.Vector2Field("", m_profile.RiverParameters.m_normalAndHeightMapTiling, GUILayout.MaxWidth(EditorGUIUtility.labelWidth - 17f));
                        m_profile.RiverParameters.m_normalAndHeightMap = (Texture2D)EditorGUILayout.ObjectField(m_profile.RiverParameters.m_normalAndHeightMap, typeof(Texture2D), false, GUILayout.MaxHeight(16f), GUILayout.MaxWidth(EditorGUIUtility.currentViewWidth));
                        EditorGUILayout.EndHorizontal();
                        m_editorUtils.InlineHelp("NormalAndHeightMap", helpEnabled);
                        m_profile.RiverParameters.m_normalStrength = m_editorUtils.Slider("NormalStrength", m_profile.RiverParameters.m_normalStrength, 0f, 1f, helpEnabled);
                        m_profile.RiverParameters.m_shoreRippleHeight = m_editorUtils.Slider("ShoreRippleHeight", m_profile.RiverParameters.m_shoreRippleHeight, 0f, 1f, helpEnabled);
                        m_profile.RiverParameters.m_refractionStrength = m_editorUtils.Slider("RefractionStrength", m_profile.RiverParameters.m_refractionStrength, 0f, 1f, helpEnabled);
                        EditorGUILayout.Space();
                        m_editorUtils.Heading("FoamSettings");
                        m_editorUtils.InlineHelp("FoamSettings", helpEnabled);
                        m_profile.RiverParameters.m_foamColor = m_editorUtils.ColorField("FoamColor", m_profile.RiverParameters.m_foamColor, helpEnabled);
                        m_editorUtils.LabelField("FoamAlbedoMap");
                        EditorGUILayout.BeginHorizontal();
                        m_profile.RiverParameters.m_foamAlbedoMapTiling = EditorGUILayout.Vector2Field("", m_profile.RiverParameters.m_foamAlbedoMapTiling, GUILayout.MaxWidth(EditorGUIUtility.labelWidth - 17f));
                        m_profile.RiverParameters.m_foamAlbedoMap = (Texture2D)EditorGUILayout.ObjectField(m_profile.RiverParameters.m_foamAlbedoMap, typeof(Texture2D), false, GUILayout.MaxHeight(16f), GUILayout.MaxWidth(EditorGUIUtility.currentViewWidth));
                        EditorGUILayout.EndHorizontal();
                        m_editorUtils.InlineHelp("FoamAlbedoMap", helpEnabled);
                        m_profile.RiverParameters.m_foamNormalMap = (Texture2D)m_editorUtils.ObjectField("FoamNormalMap", m_profile.RiverParameters.m_foamNormalMap, typeof(Texture2D), false, helpEnabled, GUILayout.MaxHeight(16f));
                        m_profile.RiverParameters.m_foamNormalStrength = m_editorUtils.Slider("FoamNormalStrength", m_profile.RiverParameters.m_foamNormalStrength, 0f, 2f, helpEnabled);
                        m_profile.RiverParameters.m_foamMaskMap = (Texture2D)m_editorUtils.ObjectField("FoamMaskMap", m_profile.RiverParameters.m_foamMaskMap, typeof(Texture2D), false, helpEnabled, GUILayout.MaxHeight(16f));
                        m_profile.RiverParameters.m_foamShoreBlend = m_editorUtils.Slider("FoamShoreBlend", m_profile.RiverParameters.m_foamShoreBlend, 0f, 1f, helpEnabled);
                        m_profile.RiverParameters.m_foamHeight = m_editorUtils.Slider("FoamHeight", m_profile.RiverParameters.m_foamHeight, 0f, 1f, helpEnabled);
                        m_profile.RiverParameters.m_foamRipple = m_editorUtils.Slider("FoamRipple", m_profile.RiverParameters.m_foamRipple, 0f, 1f, helpEnabled);
                        m_profile.RiverParameters.m_foamSpeed = m_editorUtils.Slider("FoamSpeed", m_profile.RiverParameters.m_foamSpeed, 0f, 2f, helpEnabled);
                        EditorGUILayout.Space();
                        m_editorUtils.Heading("SeaLevelSettings");
                        m_editorUtils.InlineHelp("SeaLevelSettings", helpEnabled);
                        m_profile.RiverParameters.m_seaLevelBlend = m_editorUtils.Slider("SeaLevelFoamBlend", m_profile.RiverParameters.m_seaLevelBlend, 0.001f, 10f, helpEnabled);
                        m_profile.RiverParameters.m_seaLevelFoamColor = m_editorUtils.ColorField("SeaLevelFoamColor", m_profile.RiverParameters.m_seaLevelFoamColor, helpEnabled);
                        m_profile.RiverParameters.m_seaLevelFoamNormalStrength = m_editorUtils.Slider("SeaLevelFoamNormalStrength", m_profile.RiverParameters.m_seaLevelFoamNormalStrength, 0f, 2f, helpEnabled);
                        m_profile.RiverParameters.m_pBRColor = EditorGUILayout.ColorField(new GUIContent(m_editorUtils.GetTextValue("SeaLevelFoamPBR"), m_editorUtils.GetTooltip("SeaLevelFoamPBR")), m_profile.RiverParameters.m_pBRColor, true, true, true);
                        m_editorUtils.InlineHelp("SeaLevelFoamPBR", helpEnabled);
                        break;
                    }
                }
            }
            else if (m_profile.RiverParameters.m_renderMode == Constants.ProfileRenderMode.Material)
            {
                m_editorUtils.Heading("ShadersSettings");
                m_editorUtils.InlineHelp("ShadersSettings", helpEnabled);
                m_profile.RiverParameters.m_builtInRiverMaterial = (Material)m_editorUtils.ObjectField("Built-InMaterial", m_profile.RiverParameters.m_builtInRiverMaterial, typeof(Material), false, helpEnabled);
                m_profile.RiverParameters.m_universalRiverMaterial = (Material)m_editorUtils.ObjectField("UniversalMaterial", m_profile.RiverParameters.m_universalRiverMaterial, typeof(Material), false, helpEnabled);
                m_profile.RiverParameters.m_highDefinitionRiverMaterial = (Material)m_editorUtils.ObjectField("HighDefinitionMaterial", m_profile.RiverParameters.m_highDefinitionRiverMaterial, typeof(Material), false, helpEnabled);
                EditorGUILayout.Space();
            }
            //New River Flow parameters
            else
            {
                m_editorUtils.Heading("ShadersSettings");
                m_editorUtils.InlineHelp("ShadersSettings", helpEnabled);
                m_profile.RiverFlowParamaters.m_builtInRiverMaterial = (Material)m_editorUtils.ObjectField("Built-InMaterial", m_profile.RiverFlowParamaters.m_builtInRiverMaterial, typeof(Material), false, helpEnabled);
                m_profile.RiverFlowParamaters.m_universalRiverMaterial = (Material)m_editorUtils.ObjectField("UniversalMaterial", m_profile.RiverFlowParamaters.m_universalRiverMaterial, typeof(Material), false, helpEnabled);
                m_profile.RiverFlowParamaters.m_highDefinitionRiverMaterial = (Material)m_editorUtils.ObjectField("HighDefinitionMaterial", m_profile.RiverFlowParamaters.m_highDefinitionRiverMaterial, typeof(Material), false, helpEnabled);
                EditorGUILayout.Space();
                m_editorUtils.Heading("ColorSettings");
                m_editorUtils.InlineHelp("ColorSettings", helpEnabled);
                m_profile.RiverFlowParamaters.m_waterFogColor = m_editorUtils.ColorField("WaterFogColor", m_profile.RiverFlowParamaters.m_waterFogColor, helpEnabled);
                m_profile.RiverFlowParamaters.m_waterFogDensity = m_editorUtils.Slider("WaterFogDensity", m_profile.RiverFlowParamaters.m_waterFogDensity, 0f, 5f, helpEnabled);
                m_profile.RiverFlowParamaters.m_wetness = m_editorUtils.Slider("Wetness", m_profile.RiverFlowParamaters.m_wetness, 0f, 1f, helpEnabled);
                m_profile.RiverFlowParamaters.m_refractionStrength = m_editorUtils.Slider("RefractionStrength", m_profile.RiverFlowParamaters.m_refractionStrength, 0f, 1f, helpEnabled);
                m_profile.RiverFlowParamaters.m_smoothness = m_editorUtils.Slider("Smoothness", m_profile.RiverFlowParamaters.m_smoothness, 0f, 1f, helpEnabled);
                EditorGUILayout.Space();
                m_editorUtils.Heading("Textures");
                m_editorUtils.InlineHelp("Textures", helpEnabled);
                m_profile.RiverFlowParamaters.m_derivMap = (Texture2D)m_editorUtils.ObjectField("DerivativeMap", m_profile.RiverFlowParamaters.m_derivMap, typeof(Texture2D), false, helpEnabled, GUILayout.MaxHeight(16f));
                m_profile.RiverFlowParamaters.m_foamMap = (Texture2D)m_editorUtils.ObjectField("FoamMap", m_profile.RiverFlowParamaters.m_foamMap, typeof(Texture2D), false, helpEnabled, GUILayout.MaxHeight(16f));
                m_profile.RiverFlowParamaters.m_noiseMap = (Texture2D)m_editorUtils.ObjectField("NoiseMap", m_profile.RiverFlowParamaters.m_noiseMap, typeof(Texture2D), false, helpEnabled, GUILayout.MaxHeight(16f));
                EditorGUILayout.Space();
                m_editorUtils.Heading("FlowInformation");
                m_editorUtils.InlineHelp("FlowInformation", helpEnabled);
                m_profile.RiverFlowParamaters.m_speed = m_editorUtils.Slider("Speed", m_profile.RiverFlowParamaters.m_speed, 0f, 1f, helpEnabled);
                m_profile.RiverFlowParamaters.m_flowStrength = m_editorUtils.Slider("FlowStrength", m_profile.RiverFlowParamaters.m_flowStrength, 0.1f, 1f, helpEnabled);
                m_profile.RiverFlowParamaters.m_scale = m_editorUtils.FloatField("Scale", m_profile.RiverFlowParamaters.m_scale, helpEnabled);
                m_profile.RiverFlowParamaters.m_randomAmount = m_editorUtils.Slider("RandomAmount", m_profile.RiverFlowParamaters.m_randomAmount, 0f, 1f, helpEnabled);
                m_profile.RiverFlowParamaters.m_slopeStrength = m_editorUtils.Slider("SlopeStrength", m_profile.RiverFlowParamaters.m_slopeStrength, 0f, 1f, helpEnabled);
                EditorGUILayout.Space();
                m_editorUtils.Heading("CausticsSettings");
                m_editorUtils.InlineHelp("CausticsSettings", helpEnabled);
                m_profile.RiverFlowParamaters.m_causticsMap = (Texture2D)m_editorUtils.ObjectField("CausticsMap", m_profile.RiverFlowParamaters.m_causticsMap, typeof(Texture2D), false, helpEnabled, GUILayout.MaxHeight(16f));
                m_profile.RiverFlowParamaters.m_causticsScale = m_editorUtils.FloatField("CausticsScale", m_profile.RiverFlowParamaters.m_causticsScale, helpEnabled);
                m_profile.RiverFlowParamaters.m_causticsDepth = m_editorUtils.Slider("CausticsDepth", m_profile.RiverFlowParamaters.m_causticsDepth, 0f, 1f, helpEnabled);
                m_profile.RiverFlowParamaters.m_causticsHeight = m_editorUtils.Slider("CausticsHeight", m_profile.RiverFlowParamaters.m_causticsHeight, 0f, 1f, helpEnabled);
                m_profile.RiverFlowParamaters.m_causticsBrightness = m_editorUtils.FloatField("CausticsBrightness", m_profile.RiverFlowParamaters.m_causticsBrightness, helpEnabled);
                EditorGUILayout.Space();
                m_editorUtils.Heading("FoamSettings");
                m_editorUtils.InlineHelp("FoamSettings", helpEnabled);
                m_profile.RiverFlowParamaters.m_foamDiffuse = (Texture2D)m_editorUtils.ObjectField("FoamDiffuse", m_profile.RiverFlowParamaters.m_foamDiffuse, typeof(Texture2D), false, helpEnabled, GUILayout.MaxHeight(16f));
                m_profile.RiverFlowParamaters.m_foamNormal = (Texture2D)m_editorUtils.ObjectField("FoamNormal", m_profile.RiverFlowParamaters.m_foamNormal, typeof(Texture2D), false, helpEnabled, GUILayout.MaxHeight(16f));
                m_profile.RiverFlowParamaters.m_foamScale = m_editorUtils.FloatField("FoamScale", m_profile.RiverFlowParamaters.m_foamScale, helpEnabled);
                m_profile.RiverFlowParamaters.m_foamSpeed = m_editorUtils.FloatField("FoamSpeed", m_profile.RiverFlowParamaters.m_foamSpeed, helpEnabled);
                m_profile.RiverFlowParamaters.m_foamSlopeStrength = m_editorUtils.Slider("FoamSlopeStrength", m_profile.RiverFlowParamaters.m_foamSlopeStrength, 0f, 1f, helpEnabled);
                EditorGUILayout.Space();
                m_editorUtils.Heading("SeaLevelInformation");
                m_editorUtils.InlineHelp("SeaLevelInformation", helpEnabled);
                m_profile.RiverFlowParamaters.m_seaLevelBlend = m_editorUtils.Slider("SeaLevelBlend", m_profile.RiverFlowParamaters.m_seaLevelBlend, 0.001f, 10f, helpEnabled);
                EditorGUILayout.Space();
            }
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(m_profile);
            }
        }
    }
}