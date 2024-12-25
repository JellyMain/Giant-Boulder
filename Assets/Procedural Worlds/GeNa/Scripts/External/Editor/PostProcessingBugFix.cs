using UnityEditor;
#if UNITY_POST_PROCESSING_STACK_V2
using UnityEditorInternal;
using UnityEngine.Rendering.PostProcessing;
#endif
namespace GeNa.Core
{
    [InitializeOnLoad]
    public class PostProcessingBugFix
    {
        private static bool m_isDirty = true;
        static PostProcessingBugFix()
        {
            EditorApplication.update += Update;
        }
        private static void Update()
        {
            if (m_isDirty)
            {
#if UNITY_POST_PROCESSING_STACK_V2
                PostProcessVolume postProcessVolume = UnityEngine.Object.FindObjectOfType<PostProcessVolume>();
                InternalEditorUtility.SetIsInspectorExpanded(postProcessVolume, true);
                InternalEditorUtility.SetIsInspectorExpanded(postProcessVolume, false);
#endif
                m_isDirty = false;
            }
        }
    }
}