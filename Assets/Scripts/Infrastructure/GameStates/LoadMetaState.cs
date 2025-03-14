using Const;
using Infrastructure.GameStates.Interfaces;
using Infrastructure.Services;
using Progress;
using UI;


namespace Infrastructure.GameStates
{
    public class LoadMetaState : IGameState
    {
        private readonly GameStateMachine gameStateMachine;
        private readonly SceneLoader sceneLoader;
        private readonly SaveLoadService saveLoadService;


        public LoadMetaState(GameStateMachine gameStateMachine, SceneLoader sceneLoader, SaveLoadService saveLoadService)
        {
            this.gameStateMachine = gameStateMachine;
            this.sceneLoader = sceneLoader;
            this.saveLoadService = saveLoadService;
        }


        public void Enter()
        {
            saveLoadService.Cleanup();
            sceneLoader.Load(RuntimeConstants.Scenes.MAIN_MENU_SCENE, CreateMenu);
            saveLoadService.UpdateProgress();
        }


        private void CreateMenu()
        {
        
        }
    }
}