using Factories;
using Infrastructure.GameStates;
using Infrastructure.Services;
using PlayerInput.Interfaces;
using PlayerInput.Services;
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
            if (Application.isEditor)
            {
                Container.Bind<IInput>().FromInstance(new PcInput());
            }
            else if (Application.isMobilePlatform)
            {
                Container.Bind<IInput>().FromInstance(new MobileInput());
            }
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