
#if GeNa_HDRP
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using System.Reflection;
using UnityEditor;
using UnityEngine.Rendering;
#endif
namespace GeNa.Core
{
    public static class GeNaHDRPUtility
    {
#if GeNa_HDRP
        public static void DisplayWarnings()
        {
            var pipeline = GeNaUtility.GetActivePipeline();
            if (pipeline == Constants.RenderPipeline.HighDefinition)
            {
                var frameSettings = GetCurrentDefaultFrameSettings();
                if (!frameSettings.IsEnabled(FrameSettingsField.AfterPostprocess))
                {
                    EditorGUILayout.HelpBox("WARNING! 'AfterPostprocess' is currently Disabled in the current Pipeline asset.\n" +
                                            "In order to see visualization, you must have this turned on.", MessageType.Warning);
                    if (GUILayout.Button("Turn on AfterPostprocess"))
                    {
                        frameSettings.SetEnabled(FrameSettingsField.AfterPostprocess, true);
                        SetCurrentDefaultFrameSettings(frameSettings);
                    }
                }
            }
        }
        public static MemberInfo[] GetDefaultFrameSettingsMember(RenderPipelineAsset pipelineAsset, out object objectReference)
        {
            objectReference = pipelineAsset;
            var hdrpAssetType = pipelineAsset.GetType();
            var pipelineAssetType = pipelineAsset.GetType();
#if UNITY_2021_2_OR_NEWER
            var globalSettingsInfo = pipelineAssetType.GetProperty("globalSettings", BindingFlags.NonPublic | BindingFlags.Instance); //GetPropValue(pipelineAsset, "globalSettings");
            var globalSettings = globalSettingsInfo.GetValue(objectReference);
            pipelineAssetType = globalSettings.GetType();
            objectReference = globalSettings;
#endif
            return pipelineAssetType.GetMember("m_RenderingPathDefaultCameraFrameSettings", BindingFlags.NonPublic | BindingFlags.Instance);
        }
        public static FrameSettings GetCurrentDefaultFrameSettings()
        {
            var pipelineAsset = GraphicsSettings.defaultRenderPipeline;
            if (pipelineAsset != null)
            {
                HDRenderPipelineAsset hdrpPipelineAsset = pipelineAsset as HDRenderPipelineAsset;
                if (hdrpPipelineAsset != null)
                {
                    var defaultFrameSettings = GetDefaultFrameSettingsMember(pipelineAsset, out var objectReference);
                    foreach (MemberInfo memberInfo in defaultFrameSettings)
                    {
                        if (memberInfo.MemberType == MemberTypes.Field)
                        {
                            FieldInfo fieldInfo = memberInfo as FieldInfo;
                            return (FrameSettings)fieldInfo.GetValue(objectReference);
                        }
                    }
                }
            }
            return default;
        }
        public static void SetCurrentDefaultFrameSettings(FrameSettings frameSettings)
        {
            var pipelineAsset = GraphicsSettings.defaultRenderPipeline;
            if (pipelineAsset != null)
            {
                HDRenderPipelineAsset hdrpPipelineAsset = pipelineAsset as HDRenderPipelineAsset;
                if (hdrpPipelineAsset != null)
                {
                    var defaultFrameSettings = GetDefaultFrameSettingsMember(pipelineAsset, out var objectReference);
                    foreach (MemberInfo memberInfo in defaultFrameSettings)
                    {
                        if (memberInfo.MemberType == MemberTypes.Field)
                        {
                            FieldInfo fieldInfo = memberInfo as FieldInfo;
                            fieldInfo.SetValue(objectReference, frameSettings);
                        }
                    }
                    EditorUtility.SetDirty(pipelineAsset);
                }
            }
        }
#endif
    }
}