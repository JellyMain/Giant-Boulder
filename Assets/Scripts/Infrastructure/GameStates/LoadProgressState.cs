using Const;
using Infrastructure.GameStates.Interfaces;
using Infrastructure.Services;
using Progress;
using Quests;
using Scenes;
using StaticData.Services;
using Upgrades;


namespace Infrastructure.GameStates
{
    public class LoadProgressState : IGameState
    {
        private readonly GameStateMachine gameStateMachine;
        private readonly StaticDataService staticDataService;
        private readonly SaveLoadService saveLoadService;
        private readonly PersistentPlayerProgress persistentPlayerProgress;
        private readonly QuestsService questsService;
        private readonly SceneLoader sceneLoader;
        private readonly UpgradesService upgradesService;


        public LoadProgressState(GameStateMachine gameStateMachine, StaticDataService staticDataService,
            SaveLoadService saveLoadService, PersistentPlayerProgress persistentPlayerProgress,
            QuestsService questsService, SceneLoader sceneLoader, UpgradesService upgradesService)
        {
            this.gameStateMachine = gameStateMachine;
            this.staticDataService = staticDataService;
            this.saveLoadService = saveLoadService;
            this.persistentPlayerProgress = persistentPlayerProgress;
            this.questsService = questsService;
            this.sceneLoader = sceneLoader;
            this.upgradesService = upgradesService;
        }


        public async void Enter()
        {
            await staticDataService.LoadStaticData();
            LoadSavesOrCreateNew();
            sceneLoader.Load(RuntimeConstants.Scenes.MAIN_MENU_SCENE, () => gameStateMachine.Enter<LoadMetaState>());
        }


        private void LoadSavesOrCreateNew()
        {
            persistentPlayerProgress.PlayerProgress = LoadOrCreateNewProgress();
            
            // upgradesService.SetSavedUpgrades();

            questsService.GetQuestsProgresses();
        }



        private PlayerProgress LoadOrCreateNewProgress()
        {
            PlayerProgress playerProgress = saveLoadService.LoadProgress() ?? new PlayerProgress();
            return playerProgress;
        }
    }
}
