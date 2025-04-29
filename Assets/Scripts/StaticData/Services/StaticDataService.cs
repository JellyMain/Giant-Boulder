using System.Collections.Generic;
using System.Linq;
using Assets;
using Const;
using Cysharp.Threading.Tasks;
using Quests;
using StaticData.Data;
using TerrainGenerator;
using TerrainGenerator.Enums;
using UnityEngine;


namespace StaticData.Services
{
    public class StaticDataService
    {
        private readonly AssetProvider assetProvider;
        public Dictionary<TerrainSeason, MapGenerationConfig> MapGenerationConfigs { get; private set; }
        public SpawnerConfig SpawnerConfig { get; private set; }
        public SoundConfig SoundConfig { get; private set; }
        public GameConfig GameConfig { get; private set; }
        public QuestsConfig QuestsConfig { get; private set; }
        public AnimationsConfig AnimationsConfig { get; private set; }


        public StaticDataService(AssetProvider assetProvider)
        {
            this.assetProvider = assetProvider;
        }


        public async UniTask LoadStaticData()
        {
            UniTask loadMapChunkConfigUniTask = LoadMapChunkConfig();
            UniTask loadGlobalSpawnerConfigUniTask = LoadGlobalSpawnerConfig();
            UniTask loadSoundsConfigUniTask = LoadSoundsConfig();
            UniTask loadGameConfigUniTask = LoadGameConfig();
            UniTask loadQuestsConfigUniTask = LoadQuestsConfig();
            UniTask loadAnimationsConfigUniTask = LoadAnimationsConfig();

            UniTask[] loadTasks = new[]
            {
                loadMapChunkConfigUniTask,
                loadGlobalSpawnerConfigUniTask,
                loadSoundsConfigUniTask,
                loadGameConfigUniTask,
                loadQuestsConfigUniTask,
                loadAnimationsConfigUniTask
            };

            await UniTask.WhenAll(loadTasks);
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


        private async UniTask LoadAnimationsConfig()
        {
            AnimationsConfig =
                await assetProvider.LoadAsset<AnimationsConfig>(RuntimeConstants.StaticDataAddresses.ANIMATIONS_CONFIG);
        }


        private async UniTask LoadQuestsConfig()
        {
            QuestsConfig =
                await assetProvider.LoadAsset<QuestsConfig>(RuntimeConstants.StaticDataAddresses.QUESTS_CONFIG);
        }


        private async UniTask LoadGameConfig()
        {
            GameConfig = await assetProvider.LoadAsset<GameConfig>(RuntimeConstants.StaticDataAddresses.GAME_CONFIG);
        }


        private async UniTask LoadSoundsConfig()
        {
            SoundConfig =
                await assetProvider.LoadAsset<SoundConfig>(RuntimeConstants.StaticDataAddresses.SOUNDS_CONFIG);
        }


        private async UniTask LoadMapChunkConfig()
        {
            IList<MapGenerationConfig> mapGenerationConfigsList =
                await assetProvider.LoadAssets<MapGenerationConfig>(
                    RuntimeConstants.StaticDataAddresses.MAP_GENERATION_CONFIGS, null);

            MapGenerationConfigs = mapGenerationConfigsList.ToDictionary(x => x.terrainSeason, x => x);
        }


        private async UniTask LoadGlobalSpawnerConfig()
        {
            SpawnerConfig =
                await assetProvider.LoadAsset<SpawnerConfig>(RuntimeConstants.StaticDataAddresses.SPAWNER_CONFIG);
        }
    }
}
