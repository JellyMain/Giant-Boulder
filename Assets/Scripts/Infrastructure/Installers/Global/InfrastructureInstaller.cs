using Assets;
using DataTrackers;
using DG.Tweening;
using Factories;
using Infrastructure.Services;
using PlayerInput.Interfaces;
using Progress;
using Quests;
using Scenes;
using Sounds;
using StaticData.Services;
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
            CreateAndBindLoadingScreen();
            BindSceneLoader();
            BindInputService();
            BindSaveLoadService();
            BindPersistentPlayerProgress();
            BindSoundPlayer();
            BindContainerManager();
            BindLocalContainerPasser();
            BindQuestService();
            BindAssetProvider();
            BindStatsTracker();
            BindCameraCreator();
        }

        
        private void BindCameraCreator()
        {
            Container.Bind<CameraCreator>().AsSingle();
        }
        

        private void BindStatsTracker()
        {
            Container.BindInterfacesAndSelfTo<StatsTracker>().AsSingle().NonLazy();
        }


        private void BindAssetProvider()
        {
            Container.BindInterfacesAndSelfTo<AssetProvider>().AsSingle().NonLazy();
        }


        private void BindQuestService()
        {
            Container.BindInterfacesAndSelfTo<QuestsService>().AsSingle().NonLazy();
        }


        private void BindLocalContainerPasser()
        {
            Container.Bind<LocalContainerPasser>().AsSingle().CopyIntoDirectSubContainers().NonLazy();
        }


        private void BindContainerManager()
        {
            Container.Bind<ContainerService>().AsSingle().NonLazy();
        }


        private void BindSoundPlayer()
        {
            Container.Bind<SoundPlayer>().AsSingle();
        }


        private void BindPersistentPlayerProgress()
        {
            Container.Bind<PersistentPlayerProgress>().AsSingle();
        }


        private void BindSaveLoadService()
        {
            Container.Bind<SaveLoadService>().AsSingle();
        }
        
        
        private void BindInputService()
        {
            Container.Bind<IInput>().FromInstance(new PlayerInput.Services.PlayerInput());
        }


        private void BindSceneLoader()
        {
            Container.Bind<SceneLoader>().AsSingle();
        }


        private void CreateAndBindLoadingScreen()
        {
            Container.Bind<LoadingScreen>().FromComponentInNewPrefab(loadingScreen).AsSingle().NonLazy();
        }


        private void BindStaticDataService()
        {
            Container.Bind<StaticDataService>().AsSingle();
        }
    }
}
