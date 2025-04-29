using Cinemachine;
using Cysharp.Threading.Tasks;
using DataTrackers;
using Factories;
using GameLoop;
using Infrastructure.GameStates.Interfaces;
using Infrastructure.Services;
using Progress;
using Scenes;
using StructuresSpawner;
using TerrainGenerator;
using UI;
using UI.Gameplay;
using UnityEngine;


namespace Infrastructure.GameStates
{
    public class LoadLevelState : IGameState
    {
        private readonly SceneLoader sceneLoader;
        private readonly MapCreator mapCreator;
        private readonly PlayerFactory playerFactory;
        private readonly CameraCreator cameraCreator;
        private readonly StructureSpawner structureSpawner;
        private readonly GameplayUIFactory gameplayUIFactory;
        private readonly SaveLoadService saveLoadService;
        private readonly GameStateMachine gameStateMachine;
        private readonly LevelCreationWatcher levelCreationWatcher;
        private readonly GameplayQuestTracker gameplayQuestTracker;
        private readonly ChunkUpdater chunkUpdater;
        private readonly ScoreTracker scoreTracker;


        public LoadLevelState(SceneLoader sceneLoader, MapCreator mapCreator, PlayerFactory playerFactory,
            CameraCreator cameraCreator, StructureSpawner structureSpawner, GameplayUIFactory gameplayUIFactory,
            SaveLoadService saveLoadService, GameStateMachine gameStateMachine,
            LevelCreationWatcher levelCreationWatcher, GameTimer gameTimer, GameplayQuestTracker gameplayQuestTracker)
        {
            this.sceneLoader = sceneLoader;
            this.mapCreator = mapCreator;
            this.playerFactory = playerFactory;
            this.cameraCreator = cameraCreator;
            this.structureSpawner = structureSpawner;
            this.gameplayUIFactory = gameplayUIFactory;
            this.saveLoadService = saveLoadService;
            this.gameStateMachine = gameStateMachine;
            this.levelCreationWatcher = levelCreationWatcher;
            this.gameplayQuestTracker = gameplayQuestTracker;
        }


        public async void Enter()
        {
            await CreateLevel();
            saveLoadService.UpdateProgress();

            gameStateMachine.Enter<GameLoopState>();
        }


        private async UniTask CreateLevel()
        {
            CreateMap();

            GameObject player = await CreatePlayerWithControls();

            CreateCameras();

            CreateScoreAndCurrencyUI().Forget();
            CreateRageScaleUI();

            gameplayUIFactory.CreateGameTimerUI().Forget();

            CreateStructures();

            levelCreationWatcher.LevelCreated();
            
            gameplayQuestTracker.TrackCurrentQuest();
        }


        private void CreateRageScaleUI()
        {
            gameplayUIFactory.CreateRageScaleUI().Forget();
        }


        private void CreateStructures()
        {
            structureSpawner.SpawnWalls().Forget();
            structureSpawner.ActivateSpawner();
        }


        private void CreateCameras()
        {
            Transform cameraPivot = GameObject.FindWithTag("CameraPivot").transform;
            SetVirtualCamera(cameraPivot);
        }


        private async UniTask<GameObject> CreatePlayerWithControls()
        {
            PlayerControlsUI playerControlsUI = await gameplayUIFactory.CreatePlayerControlsUI();
            GameObject player = await playerFactory.CreatePlayer(new Vector3(50, 100, 50), playerControlsUI);
            return player;
        }


        private void CreateMap()
        {
            levelCreationWatcher.MapGenerationStarted();
            mapCreator.CreateMap();
        }


        private async UniTaskVoid CreateScoreAndCurrencyUI()
        {
            Camera uiCamera = await cameraCreator.CreateUICamera();
            GameObject scoreAndCurrencyUI = await gameplayUIFactory.CreateScoreAndCurrencyUI(uiCamera);

            CoinsUI coinsUI = scoreAndCurrencyUI.GetComponent<CoinsUI>();
            ScoreUI scoreUI = scoreAndCurrencyUI.GetComponent<ScoreUI>();

            coinsUI.Construct(uiCamera);
            scoreUI.Construct(uiCamera);

            cameraCreator.StackCamera(uiCamera);
        }


        private async void SetVirtualCamera(Transform cameraPivot)
        {
            CinemachineVirtualCamera virtualCamera = await cameraCreator.CreateVirtualCamera();
            cameraCreator.SetUpVirtualCamera(virtualCamera, cameraPivot);
        }
    }
}
