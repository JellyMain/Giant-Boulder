using Constants;
using Factories;
using Infrastructure.GameStates.Interfaces;
using Infrastructure.Services;
using UI;


namespace Infrastructure.GameStates
{
    public class LoadLevelState : IGameState
    {
        private readonly SceneLoader sceneLoader;


        public LoadLevelState(GameStateMachine gameStateMachine, SceneLoader sceneLoader,
            UIFactory uiFactory, PlayerFactory playerFactory)
        {
            this.sceneLoader = sceneLoader;
        }


        public void Enter()
        {
            sceneLoader.Load(RuntimeConstants.Scenes.GAME_SCENE, CreateLevel);
        }


        private void CreateLevel()
        {
        }
    }
}
