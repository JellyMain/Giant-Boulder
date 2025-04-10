using Const;
using Cysharp.Threading.Tasks;
using Infrastructure.GameStates.Interfaces;
using Infrastructure.Services;
using Progress;
using Quests;
using Scenes;
using StaticData.Services;
using UI;
using UnityEngine;


namespace Infrastructure.GameStates
{
    public class LoadProgressState : IGameState
    {
        private readonly GameStateMachine gameStateMachine;
        private readonly StaticDataService staticDataService;
        private readonly SaveLoadService saveLoadService;
        private readonly PersistentPlayerProgress persistentPlayerProgress;
        private readonly QuestService questService;
        private readonly SceneLoader sceneLoader;


        public LoadProgressState(GameStateMachine gameStateMachine, StaticDataService staticDataService,
            SaveLoadService saveLoadService, PersistentPlayerProgress persistentPlayerProgress,
            QuestService questService, SceneLoader sceneLoader)
        {
            this.gameStateMachine = gameStateMachine;
            this.staticDataService = staticDataService;
            this.saveLoadService = saveLoadService;
            this.persistentPlayerProgress = persistentPlayerProgress;
            this.questService = questService;
            this.sceneLoader = sceneLoader;
        }


        public async void Enter()
        {
            await staticDataService.LoadStaticData();
            LoadSavesOrCreateNew();
            questService.SetCurrentQuest(staticDataService.QuestsConfig.quests[0]);
           sceneLoader.Load(RuntimeConstants.Scenes.MAIN_MENU_SCENE, () => gameStateMachine.Enter<LoadMetaState>());  
        }
        

        private void LoadSavesOrCreateNew()
        {
            persistentPlayerProgress.PlayerProgress = saveLoadService.LoadProgress() ?? CreateNewProgress();
        }


        private PlayerProgress CreateNewProgress()
        {
            PlayerProgress playerProgress = new PlayerProgress();
            return playerProgress;
        }
    }
}
