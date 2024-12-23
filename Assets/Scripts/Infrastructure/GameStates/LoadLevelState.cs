using Constants;
using Factories;
using Infrastructure.GameStates.Interfaces;
using Infrastructure.Services;
using TerrainGenerator;
using UI;


namespace Infrastructure.GameStates
{
    public class LoadLevelState : IGameState
    {
        private readonly SceneLoader sceneLoader;
        private readonly MapCreator mapCreator;


        public LoadLevelState(SceneLoader sceneLoader, MapCreator mapCreator)
        {
            this.sceneLoader = sceneLoader;
            this.mapCreator = mapCreator;
        }


        public void Enter()
        {
            sceneLoader.Load(RuntimeConstants.Scenes.GAME_SCENE, CreateLevel);
        }


        private void CreateLevel()
        {
            mapCreator.CreateMap();
        }
    }
}
