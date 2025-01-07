using System.Collections.Generic;
using System.Linq;
using Const;
using StaticData.Data;
using UnityEngine;
using Zenject;


namespace StaticData.Services
{
    public class StaticDataService
    {
        public Dictionary<TerrainSeason, MapGenerationConfig> MapGenerationConfigs { get; private set; }


        public void LoadStaticData()
        {
            LoadMapChunkConfig();
        }


        public MapGenerationConfig MapConfigForSeason(TerrainSeason terrainSeason)
        {
            if (MapGenerationConfigs.TryGetValue(terrainSeason, out MapGenerationConfig mapGenerationConfig))
            {
                return mapGenerationConfig;
            }

            Debug.LogError($"Couldn't find map generation config with key {terrainSeason}");
            return null;
        }


        private void LoadMapChunkConfig()
        {
            MapGenerationConfigs =
                Resources.LoadAll<MapGenerationConfig>(RuntimeConstants.StaticDataPaths.MAP_GENERATION_CONFIGS)
                    .ToDictionary(x => x.terrainSeason, x => x);
        }
    }
}
