using UnityEngine;

namespace GeNa.Core
{
    public static class GeNaRuntimeEvents
    {
        public static System.Action onBeforeSceneLoad;
        public static System.Action onAfterSceneLoad;
        public static System.Action onRuntimeLoad;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void OnBeforeSceneLoadRuntimeMethod()
        {
            onBeforeSceneLoad?.Invoke();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void OnAfterSceneLoadRuntimeMethod()
        {
            onAfterSceneLoad?.Invoke();
        }

        [RuntimeInitializeOnLoadMethod]
        static void OnRuntimeLoadMethod()
        {
            onRuntimeLoad?.Invoke();
            Initialize();
        }

#if UNITY_2023_1_OR_NEWER
        public static Object[] FindObjectsByType(System.Type type)
        {
            return Object.FindObjectsByType(type, FindObjectsSortMode.None);
        }
#endif

        public static void Initialize()
        {
#if UNITY_2023_1_OR_NEWER
            GeNaEvents.findObjectOfType = Object.FindFirstObjectByType;
            GeNaEvents.findObjectsOfType = FindObjectsByType;
#else
            GeNaEvents.findObjectOfType = Object.FindObjectOfType;
            GeNaEvents.findObjectsOfType = Object.FindObjectsOfType;
#endif
        }
    }
}