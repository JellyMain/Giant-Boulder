using System.Collections.Generic;
using System.Linq;
using Const;
using StaticData.Data;
using UnityEngine;


namespace StaticData.Services
{
    public class StaticDataService
    {
        public Dictionary<TerrainSeason, MapGenerationConfig> MapGenerationConfigs { get; private set; }
        public Dictionary<int, SpawnerConfig> SpawnerConfigs { get; private set; }


        public void LoadStaticData()
        {
            LoadMapChunkConfig();
            LoadSpawnerConfigs();
        }


        public SpawnerConfig SpawnerConfigForSpawnOrder(int spawnOrder)
        {
            if (SpawnerConfigs.TryGetValue(spawnOrder, out SpawnerConfig spawnerConfig))
            {
                return spawnerConfig;
            }

            Debug.LogError($"Couldn't find spawner config with order key {spawnOrder}");
            return null;
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
        

        private void LoadSpawnerConfigs()
        {
            SpawnerConfigs =
                Resources.LoadAll<SpawnerConfig>(RuntimeConstants.StaticDataPaths.STRUCTURE_SPAWNER_CONFIGS)
                    .Where(x => x.isEnabled)
                    .ToDictionary(x => x.spawnOrder, x => x);
        }
    }
}
