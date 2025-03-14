using DataTrackers;
using DG.Tweening;
using Factories;
using PlayerInput.Interfaces;
using Progress;
using StaticData.Services;
using StructuresSpawner;
using TerrainGenerator;
using UI;
using UnityEngine;
using Zenject;


namespace Infrastructure.Installers.Global
{
    public class InfrastructureInstaller : MonoInstaller
    {
        [SerializeField] private LoadingScreen loadingScreen;


        public override void InstallBindings()
        {
            DOTween.Init();
            BindStaticDataService();
            BindPlayerFactory();
            CreateAndBindLoadingScreen();
            BindSceneLoader();
            BindUIFactory();
            BindInputService();
            BindNoiseGenerator();
            BindTextureGenerator();
            BindMeshGenerator();
            BindChunkFactory();
            BindMapCreator();
            BindCameraCreator();
            BindCoinFactory();
            BindStructureSpawner();
            BindCurrencyTracker();
            BindScoreTracker();
            BindSaveLoadService();
            BindPersistentPlayerProgress();
        }


        private void BindPersistentPlayerProgress()
        {
            Container.Bind<PersistentPlayerProgress>().AsSingle();
        }


        private void BindSaveLoadService()
        {
            Container.BindInterfacesAndSelfTo<SaveLoadService>().AsSingle();
        }


        private void BindScoreTracker()
        {
            Container.Bind<ScoreTracker>().AsSingle();
        }


        private void BindCurrencyTracker()
        {
            Container.Bind<CurrencyTracker>().AsSingle();
        }


        private void BindStructureSpawner()
        {
            Container.Bind<StructureSpawner>().AsSingle();
        }


        private void BindCoinFactory()
        {
            Container.Bind<CoinsFactory>().AsSingle().NonLazy();
        }


        private void BindCameraCreator()
        {
            Container.Bind<CameraCreator>().AsSingle();
        }


        private void BindMapCreator()
        {
            Container.Bind<MapCreator>().AsSingle();
        }


        private void BindChunkFactory()
        {
            Container.Bind<ChunkFactory>().AsSingle();
        }


        private void BindMeshGenerator()
        {
            Container.Bind<MeshGenerator>().AsSingle();
        }


        private void BindTextureGenerator()
        {
            Container.Bind<TextureGenerator>().AsSingle();
        }


        private void BindInputService()
        {
            Container.Bind<IInput>().FromInstance(new PlayerInput.Services.PlayerInput());
        }


        private void BindUIFactory()
        {
            Container.Bind<UIFactory>().AsSingle();
        }


        private void BindSceneLoader()
        {
            Container.Bind<SceneLoader>().AsSingle();
        }


        private void CreateAndBindLoadingScreen()
        {
            Container.Bind<LoadingScreen>().FromComponentInNewPrefab(loadingScreen).AsSingle().NonLazy();
        }


        private void BindPlayerFactory()
        {
            Container.Bind<PlayerFactory>().AsSingle();
        }


        private void BindStaticDataService()
        {
            Container.Bind<StaticDataService>().AsSingle();
        }


        private void BindNoiseGenerator()
        {
            Container.Bind<NoiseGenerator>().AsSingle();
        }
    }
}
