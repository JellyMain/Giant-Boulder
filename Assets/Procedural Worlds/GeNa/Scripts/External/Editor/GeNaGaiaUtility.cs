using UnityEditor;
using UnityEngine;
#if GAIA_2_PRESENT
using Gaia;
using ProceduralWorlds.WaterSystem;
#endif

namespace GeNa.Core
{
    [InitializeOnLoad]
    public class GeNaGaiaUtility
    {
        static GeNaGaiaUtility()
        {
            // If GeNa Present && Gaia Present
#if GAIA_2_PRESENT
            GeNaUtility.Gaia2Present = true;
            GeNaEvents.SetSeaLevel = SetSeaLevel;
            GeNaUtility.GaiaSeaLevelValue = GetRuntimeSeaLevel();
            GeNaEvents.GetSeaLevel = GetRuntimeSeaLevel;
#if GAIA_PRO_PRESENT
            GeNaEvents.HasTerrainsAsScenes = GetRuntimeHasTerrainsAsScenes;
#endif
#else
            GeNaUtility.Gaia2Present = false;
#endif
            GeNaEvents.SetupRiverWeatherSync = SetupRiverWeatherController;
        }
        /// <summary>
        /// Sets the sea level on the spawner (Min Height)
        /// </summary>
        /// <param name="spawner"></param>
        public static float SetSeaLevel(GeNaSpawnerData spawner)
        {
            if (spawner == null)
            {
                return 0f;
            }
            float seaLevel = 0f;
#if GAIA_2_PRESENT
            seaLevel = GetGaiaSeaLevel();
            spawner.SpawnCriteria.SeaLevel = seaLevel;
#endif
            return seaLevel;
        }
        /// <summary>
        /// Gets the gaia sea level value
        /// </summary>
        /// <returns></returns>
        private static float GetGaiaSeaLevel()
        {
            float seaLevel = 0f;
#if GAIA_2_PRESENT
            GaiaSessionManager manager = Object.FindObjectOfType<GaiaSessionManager>();
            if (manager != null)
            {
                seaLevel = manager.GetSeaLevel();
            }
            else
            {
                PWS_WaterSystem waterSystem = PWS_WaterSystem.Instance;
                if (waterSystem != null)
                {
                    seaLevel = waterSystem.SeaLevel;
                }
            }
#endif
            return seaLevel;
        }
        /// <summary>
        /// Gets the sea level from the water system
        /// </summary>
        /// <returns></returns>
        public static float GetRuntimeSeaLevel(float defaultValue = 0f)
        {
            float seaLevel = defaultValue;
#if GAIA_2_PRESENT
            PWS_WaterSystem waterSystem = PWS_WaterSystem.Instance;
            if (waterSystem != null)
            {
                seaLevel = waterSystem.SeaLevel;
            }
#endif
            return seaLevel;
        }
        public static bool GetRuntimeHasTerrainsAsScenes()
        {
            bool hasTerrainScenes = false;
#if GAIA_PRO_PRESENT
            hasTerrainScenes = GaiaUtils.HasDynamicLoadedTerrains();
#endif
            return hasTerrainScenes;
        }
        public static bool SetupRiverWeatherController(GameObject go, GeNaRiverProfile profile, float seaLevel, bool isEnabled)
        {
            GeNaRiverWeatherController controller = go.GetComponent<GeNaRiverWeatherController>();
            if (isEnabled)
            {
                if (controller == null)
                {
                    controller = go.AddComponent<GeNaRiverWeatherController>();
                }
                controller.m_riverProfile = profile;
                controller.SeaLevel = seaLevel;
                return true;
            }
            else
            {
                if (controller != null)
                {
                    GeNaEvents.Destroy(controller);
                }
                return false;
            }
        }
    }
}