using Const;
using Infrastructure.GameStates.Interfaces;
using Infrastructure.Services;
using UI;


namespace Infrastructure.GameStates
{
    public class LoadMetaState : IGameState
    {
        private readonly GameStateMachine gameStateMachine;
        private readonly SceneLoader sceneLoader;


        public LoadMetaState(GameStateMachine gameStateMachine, SceneLoader sceneLoader)
        {
            this.gameStateMachine = gameStateMachine;
            this.sceneLoader = sceneLoader;
        }


        public void Enter()
        {
            sceneLoader.Load(RuntimeConstants.Scenes.MAIN_MENU_SCENE, CreateMenu);
        }


        private void CreateMenu()
        {
        
        }
    }
}