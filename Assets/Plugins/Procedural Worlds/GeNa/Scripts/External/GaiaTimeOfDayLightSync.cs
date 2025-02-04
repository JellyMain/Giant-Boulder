using UnityEngine;

namespace GeNa.Core
{
    public enum LightSyncRenderMode { NightOnly, AlwaysOn, AlwaysOnOptimized }

    [ExecuteAlways]
    public class GaiaTimeOfDayLightSync : MonoBehaviour
    {
        public LightSyncRenderMode m_renderMode = LightSyncRenderMode.NightOnly;
        public bool m_useRenderDistance = true;
        public float m_renderDistance = 150f;
        public bool m_useShadowDistanceCulling = true;
        public float m_shadowRenderDistance = 50f;
        public Light m_lightSource;
        public GameObject m_emissionObject;

        [SerializeField] private GaiaTimeOfDayLightSyncManager m_manager;
        private bool m_validated = false;

        public void OnEnable()
        {
            if (m_lightSource == null)
            {
                m_lightSource = GetComponent<Light>();
            }

            m_validated = m_lightSource != null;

            if (m_manager == null)
            {
                m_manager = GaiaTimeOfDayLightSyncManager.Instance;
            }

            if (m_manager != null)
            {
                m_manager.AddLightSyncComponent(this);
                SetRenderState(m_manager.IsNightTime());
            }
        }
        public void OnDisable()
        {
            if (m_manager != null)
            {
                m_manager.RemoveLightSyncComponent(this);
            }
        }

        /// <summary>
        /// Refreshes the system
        /// </summary>
        public void Refresh()
        {
            if (m_manager != null)
            {
                SetRenderState(m_manager.IsNightTime());
            }
        }
        /// <summary>
        /// Refreshes the system
        /// </summary>
        /// <param name="manager"></param>
        public void ValidateComponents(GaiaTimeOfDayLightSyncManager manager)
        {
            m_manager = manager;
            OnEnable();
        }
        /// <summary>
        /// Refreshes the system
        /// </summary>
        public void ValidateComponents()
        {
            m_manager = GaiaTimeOfDayLightSyncManager.Instance;
            OnEnable();
        }
        /// <summary>
        /// Sets the render state
        /// </summary>
        /// <param name="value"></param>
        public void SetRenderState(bool value)
        {
            if (m_validated)
            {
                switch (m_renderMode)
                {
                    case LightSyncRenderMode.NightOnly:
                    {
                        m_lightSource.enabled = value;
                        if (m_emissionObject != null)
                        {
                            m_emissionObject.SetActive(value);
                        }
                        break;
                    }
                    case LightSyncRenderMode.AlwaysOnOptimized:
                    {
                        m_lightSource.enabled = true;
                        if (m_emissionObject != null)
                        {
                            m_emissionObject.SetActive(true);
                        }
                        break;
                    }
                    default:
                    {
                        m_lightSource.enabled = true;
                        if (m_emissionObject != null)
                        {
                            m_emissionObject.SetActive(true);
                        }
                        break;
                    }
                }
            }
        }
        /// <summary>
        /// Sets the culling state if it's enabled
        /// </summary>
        /// <param name="value"></param>
        /// <param name="player"></param>
        public bool SetCullingState(bool value, Transform player)
        {
            if (m_renderMode == LightSyncRenderMode.AlwaysOn)
            {
                m_lightSource.enabled = true;
                return true;
            }

            if (m_useRenderDistance && m_validated || m_renderMode == LightSyncRenderMode.AlwaysOnOptimized && m_validated)
            {
                if (m_renderMode == LightSyncRenderMode.NightOnly && !value)
                {
                    m_lightSource.enabled = false;
                    return false;
                }

                if (player != null)
                {
                    float distance = Vector3.Distance(transform.position, player.position);
                    //Light enabled
                    if (distance > m_renderDistance)
                    {
                        m_lightSource.enabled = false;
                        return false;
                    }
                    else
                    {
                        m_lightSource.enabled = true;
                        //Shadow culling
                        if (m_useShadowDistanceCulling)
                        {
                            m_lightSource.shadows = distance > m_shadowRenderDistance ? LightShadows.None : m_manager.DefaultShadowCastingMode;
                        }
                        return true;
                    }
                }
            }

            return false;
        }
    }
}