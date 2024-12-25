using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GeNa.Core
{
    [ExecuteAlways]
    public class GaiaTimeOfDayLightSyncManager : MonoBehaviour
    {
        #region Static Properties

        public static GaiaTimeOfDayLightSyncManager Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = GeNaEvents.FindObjectOfType<GaiaTimeOfDayLightSyncManager>();
                    if (m_instance == null)
                    {
                        GeNaManager genaManager = GeNaManager.GetInstance();
                        if (genaManager != null)
                        {
                            m_instance = genaManager.gameObject.AddComponent<GaiaTimeOfDayLightSyncManager>();
                        }
                        else
                        {
                            GameObject manager = new GameObject("Light Sync Manager");
                            m_instance = manager.AddComponent<GaiaTimeOfDayLightSyncManager>();
                        }
                    }
                }

                return m_instance;
            }
        }

        [SerializeField] private static GaiaTimeOfDayLightSyncManager m_instance;

        #endregion

        #region Public Properties

        public Camera MainCamera;
        public LightShadows DefaultShadowCastingMode = LightShadows.Soft;

        public bool EnableSystem
        {
            get { return m_enableSystem; }
            set
            {
                if (m_enableSystem != value)
                {
                    m_enableSystem = value;
                    ResetSyncComponenetsLightState();
                    if (value)
                    {
                        RebuildSystem();
                    }
                }
            }
        }

        [SerializeField] private bool m_enableSystem = true;

        #endregion

        #region Private Properties

        [SerializeField]
        private List<GaiaTimeOfDayLightSync> m_lightSyncComponenets = new List<GaiaTimeOfDayLightSync>();

        private int m_currentActiveCount = 0;
        private bool m_lastIsNightValue = false;

        #endregion

        private void OnEnable()
        {
            if (MainCamera == null)
            {
                MainCamera = Camera.main;
            }

            bool isNight = IsNightTime();
            UpdateRenderState(isNight);
            m_lastIsNightValue = isNight;

#if UNITY_EDITOR
            if (Application.isPlaying)
            {
                UnityEditor.EditorApplication.update -= EditorUpdate;
            }
            else
            {
                UnityEditor.EditorApplication.update -= EditorUpdate;
                UnityEditor.EditorApplication.update += EditorUpdate;
            }
#endif
        }

        private void LateUpdate()
        {
            if (Application.isPlaying)
            {
                UpdateSyncSystem(MainCamera, IsNightTime());
            }
        }

        /// <summary>
        /// Adds a light componenet to the system
        /// </summary>
        /// <param name="componenet"></param>
        public void AddLightSyncComponent(GaiaTimeOfDayLightSync componenet)
        {
            m_lightSyncComponenets.Add(componenet);
        }

        /// <summary>
        /// Removes the light componenet to the system
        /// </summary>
        /// <param name="componenet"></param>
        public void RemoveLightSyncComponent(GaiaTimeOfDayLightSync componenet)
        {
            m_lightSyncComponenets.Remove(componenet);
        }

        /// <summary>
        /// Returns bool if it's day time
        /// </summary>
        /// <returns></returns>
        public bool IsNightTime()
        {
#if GAIA_PRO_PRESENT && !HDRPTIMEOFDAY
            if (Gaia.ProceduralWorldsGlobalWeather.Instance != null)
            {
                return Gaia.ProceduralWorldsGlobalWeather.Instance.CheckIsNight();
            }
#elif HDPipeline && HDRPTIMEOFDAY
            if (ProceduralWorlds.HDRPTOD.HDRPTimeOfDay.Instance != null)
            {
                return !ProceduralWorlds.HDRPTOD.HDRPTimeOfDay.Instance.IsDayTime();
            }
#endif
            return false;
        }

        /// <summary>
        /// Gets the current count of the light sync
        /// </summary>
        /// <returns></returns>
        public int GetCurrentSyncCount(out int activeCount)
        {
            activeCount = m_currentActiveCount;
            return m_lightSyncComponenets.Count;
        }

        /// <summary>
        /// Rebuilds the whole system and sets up all light sources witht he sync componenet
        /// </summary>
        private void RebuildSystem()
        {
            m_lightSyncComponenets.Clear();
            GaiaTimeOfDayLightSync[] lightSyncs = GeNaEvents.FindObjectsOfType<GaiaTimeOfDayLightSync>();
            if (lightSyncs.Length > 0)
            {
                foreach (GaiaTimeOfDayLightSync lightSync in lightSyncs)
                {
                    if (lightSync != null)
                    {
                        lightSync.ValidateComponents(this);
                    }
                }
            }
        }

        /// <summary>
        /// Updates the sync system
        /// </summary>
        private void UpdateSyncSystem(Camera mainCamera, bool isNight)
        {
            if (mainCamera != null)
            {
                if (EnableSystem && m_lightSyncComponenets.Count > 0)
                {
                    if (m_lastIsNightValue != isNight)
                    {
                        m_lastIsNightValue = isNight;
                        for (int i = 0; i < m_lightSyncComponenets.Count; i++)
                        {
                            if (m_lightSyncComponenets[i] != null)
                            {
                                m_lightSyncComponenets[i].SetRenderState(isNight);
                            }
                        }

                        return;
                    }

                    m_currentActiveCount = 0;
                    for (int i = 0; i < m_lightSyncComponenets.Count; i++)
                    {
                        if (m_lightSyncComponenets[i] != null)
                        {
                            if (m_lightSyncComponenets[i].SetCullingState(isNight, mainCamera.transform))
                            {
                                m_currentActiveCount++;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Upates the render state
        /// </summary>
        /// <param name="isDay"></param>
        private void UpdateRenderState(bool isNight)
        {
            for (int i = 0; i < m_lightSyncComponenets.Count; i++)
            {
                if (m_lightSyncComponenets[i] != null)
                {
                    m_lightSyncComponenets[i].SetRenderState(isNight);
                }
            }
        }

        /// <summary>
        /// Resets the light component to be on and rendering shadows
        /// </summary>
        private void ResetSyncComponenetsLightState()
        {
            for (int i = 0; i < m_lightSyncComponenets.Count; i++)
            {
                if (m_lightSyncComponenets[i] != null)
                {
                    m_lightSyncComponenets[i].SetRenderState(true);
                    if (m_lightSyncComponenets[i].m_useShadowDistanceCulling)
                    {
                        if (m_lightSyncComponenets[i].m_lightSource != null)
                        {
                            m_lightSyncComponenets[i].m_lightSource.shadows = DefaultShadowCastingMode;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Editor update used to keep the light sync updated in the editor
        /// </summary>
        private void EditorUpdate()
        {
#if UNITY_EDITOR
            SceneView sceneView = SceneView.lastActiveSceneView;
            if (sceneView != null)
            {
                UpdateSyncSystem(sceneView.camera, IsNightTime());
            }
#endif
        }
    }
}