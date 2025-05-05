using Const;
using Infrastructure.GameStates.Interfaces;
using Infrastructure.Services;
using Progress;
using Quests;
using Scenes;
using StaticData.Services;


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


        public LoadProgressState(GameStateMachine gameStateMachine, StaticDataService staticDataService,
            SaveLoadService saveLoadService, PersistentPlayerProgress persistentPlayerProgress,
            QuestsService questsService, SceneLoader sceneLoader)
        {
            this.gameStateMachine = gameStateMachine;
            this.staticDataService = staticDataService;
            this.saveLoadService = saveLoadService;
            this.persistentPlayerProgress = persistentPlayerProgress;
            this.questsService = questsService;
            this.sceneLoader = sceneLoader;
        }


        public async void Enter()
        {
            await staticDataService.LoadStaticData();
            LoadSavesOrCreateNew();
            sceneLoader.Load(RuntimeConstants.Scenes.MAIN_MENU_SCENE, () => gameStateMachine.Enter<LoadMetaState>());
        }


        private void LoadSavesOrCreateNew()
        {
            persistentPlayerProgress.PlayerProgress = saveLoadService.LoadProgress();

            if (persistentPlayerProgress.PlayerProgress == null)
            {
                persistentPlayerProgress.PlayerProgress = CreateNewProgress();
                questsService.SetNewQuests();
            }
            else
            {
                questsService.SetSavedQuests();
            }
        }



        private PlayerProgress CreateNewProgress()
        {
            PlayerProgress playerProgress = new PlayerProgress();
            return playerProgress;
        }
    }
}
