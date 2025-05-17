using Assets;
using Cheats;
using Coins;
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
using Stats;
using UI;
using UnityEngine;
using Upgrades;
using Zenject;


namespace Infrastructure.Installers.Global
{
    public class InfrastructureInstaller : MonoInstaller
    {
        [SerializeField] private SessionSaveService sessionSaveServicePrefab;
        [SerializeField] private LoadingScreen loadingScreenPrefab;
        [SerializeField] private CheatConsole cheatConsolePrefab;


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
            BindStatsService();
            BindCameraCreator();
            CreateAndBindSessionSaveService();
            BindCurrencyService();
            BindUpgradesService();
            CreateAndBindCheatConsole();
        }


        private void CreateAndBindCheatConsole()
        {
            Container.Bind<CheatConsole>().FromComponentInNewPrefab(cheatConsolePrefab).AsSingle().NonLazy();
        }


        private void BindUpgradesService()
        {
            Container.Bind<UpgradesService>().AsSingle().NonLazy();
        }


        private void BindCurrencyService()
        {
            Container.BindInterfacesAndSelfTo<CurrencyService>().AsSingle().NonLazy();
        }


        private void CreateAndBindSessionSaveService()
        {
            Container.Bind<SessionSaveService>().FromComponentInNewPrefab(sessionSaveServicePrefab).AsSingle()
                .NonLazy();
        }


        private void BindCameraCreator()
        {
            Container.Bind<CameraCreator>().AsSingle();
        }


        private void BindStatsService()
        {
            Container.BindInterfacesAndSelfTo<StatsService>().AsSingle().NonLazy();
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
            Container.Bind<LoadingScreen>().FromComponentInNewPrefab(loadingScreenPrefab).AsSingle().NonLazy();
        }


        private void BindStaticDataService()
        {
            Container.Bind<StaticDataService>().AsSingle();
        }
        
        
    }
}
