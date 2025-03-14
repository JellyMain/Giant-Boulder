using Infrastructure.GameStates.Interfaces;
using Infrastructure.Services;
using Progress;
using StaticData.Services;
using UnityEngine;


namespace Infrastructure.GameStates
{
    public class LoadProgressState : IGameState
    {
        private readonly GameStateMachine gameStateMachine;
        private readonly StaticDataService staticDataService;
        private readonly SaveLoadService saveLoadService;
        private readonly PersistentPlayerProgress persistentPlayerProgress;


        public LoadProgressState(GameStateMachine gameStateMachine, StaticDataService staticDataService,
            SaveLoadService saveLoadService, PersistentPlayerProgress persistentPlayerProgress)
        {
            this.gameStateMachine = gameStateMachine;
            this.staticDataService = staticDataService;
            this.saveLoadService = saveLoadService;
            this.persistentPlayerProgress = persistentPlayerProgress;
        }


        public void Enter()
        {
            LoadStaticData();
            LoadSavesOrCreateNew();
            gameStateMachine.Enter<LoadMetaState>();
        }


        private void LoadStaticData()
        {
            staticDataService.LoadStaticData();
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
