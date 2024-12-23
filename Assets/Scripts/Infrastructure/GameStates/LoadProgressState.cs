using Infrastructure.GameStates.Interfaces;
using Infrastructure.Services;
using StaticData.Services;
using UnityEngine;


namespace Infrastructure.GameStates
{
    public class LoadProgressState : IGameState
    {
        private readonly GameStateMachine gameStateMachine;
        private readonly StaticDataService staticDataService;


        public LoadProgressState(GameStateMachine gameStateMachine, StaticDataService staticDataService)
        {
            this.gameStateMachine = gameStateMachine;
            this.staticDataService = staticDataService;
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
            
        }
    }
}
