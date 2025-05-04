using DataTrackers;
using Factories;
using GameLoop;
using Infrastructure.GameStates;
using Infrastructure.Services;
using Quests;
using StructuresSpawner;
using TerrainGenerator;
using UnityEngine;
using Zenject;


namespace Infrastructure.Installers.Scene
{
    public class GameplaySceneInstaller : MonoInstaller
    {
        [SerializeField] private ChunkUpdater chunkUpdater;
        [SerializeField] private GameTimer gameTimer;
        [SerializeField] private RageScale rageScale;
        [SerializeField] private GrassSpawner grassSpawner;


        public override void InstallBindings()
        {
            BindGameStates();
            BindRageScale();
            BindChunkUpdater();
            BindGameTimer();
            BindMapCreator();
            BindPlayerFactory();
            BindStructureSpawner();
            BindLevelCreationWatcher();
            BindNoiseGenerator();
            BindMeshGenerator();
            BindChunkFactory();
            BindCoinFactory();
            BindGameplayUIFactory();
            BindGameOverHandler();
            BindScoreTracker();
            BindGameCurrencyTracker();
            BindGameplayQuestTracker();
            BindQuestFactory();
            BindDestroyedObjectsTracker();
            BindGrassSpawner();
        }


        private void BindGrassSpawner()
        {
            Container.Bind<GrassSpawner>().FromComponentInNewPrefab(grassSpawner).AsSingle().NonLazy();
        }


        private void BindDestroyedObjectsTracker()
        {
            Container.BindInterfacesAndSelfTo<DestroyedObjectsTracker>().AsSingle();
        }


        private void BindQuestFactory()
        {
            Container.Bind<QuestProgressUpdaterFactory>().AsSingle();
        }


        private void BindGameplayQuestTracker()
        {
            Container.BindInterfacesAndSelfTo<GameplayQuestTracker>().AsSingle().NonLazy();
        }


        private void BindGameCurrencyTracker()
        {
            Container.BindInterfacesAndSelfTo<GameCurrencyTracker>().AsSingle();
        }
        

        private void BindRageScale()
        {
            Container.Bind<RageScale>().FromComponentInNewPrefab(rageScale).AsSingle().NonLazy();
        }


        private void BindScoreTracker()
        {
            Container.BindInterfacesAndSelfTo<ScoreTracker>().AsSingle();
        }
        

        private void BindGameplayUIFactory()
        {
            Container.Bind<GameplayUIFactory>().AsSingle().NonLazy();
        }


        private void BindCoinFactory()
        {
            Container.BindInterfacesAndSelfTo<CoinsFactory>().AsSingle().NonLazy();
        }
        
        
        private void BindChunkFactory()
        {
            Container.Bind<ChunkFactory>().AsSingle();
        }
        
        
        private void BindMeshGenerator()
        {
            Container.Bind<MeshGenerator>().AsSingle();
        }
        
        
        private void BindNoiseGenerator()
        {
            Container.Bind<NoiseGenerator>().AsSingle();
        }
        
        
        private void BindLevelCreationWatcher()
        {
            Container.Bind<LevelCreationWatcher>().AsSingle();
        }
        
        
        private void BindStructureSpawner()
        {
            Container.Bind<StructureSpawner>().AsSingle();
        }
        
        
       

        
        private void BindPlayerFactory()
        {
            Container.Bind<PlayerFactory>().AsSingle();
        }
        
        
        private void BindMapCreator()
        {
            Container.Bind<MapCreator>().AsSingle();
        }


        private void BindGameStates()
        {
            Container.Bind<LoadLevelState>().AsSingle().NonLazy();
            Container.Bind<GameLoopState>().AsSingle().NonLazy();
        }


        private void BindGameTimer()
        {
            Container.Bind<GameTimer>().FromComponentInNewPrefab(gameTimer).AsSingle().NonLazy();
        }


        private void BindChunkUpdater()
        {
            Container.Bind<ChunkUpdater>().FromComponentInNewPrefab(chunkUpdater).AsSingle().NonLazy();
        }


        private void BindGameOverHandler()
        {
            Container.BindInterfacesAndSelfTo<GameOverHandler>().AsSingle();
        }
    }
}