using Cinemachine;
using Const;
using DataTrackers;
using Factories;
using GameLoop;
using Infrastructure.GameStates.Interfaces;
using Infrastructure.Services;
using Player;
using Progress;
using StructuresSpawner;
using TerrainGenerator;
using UI;
using UnityEngine;
using Zenject;


namespace Infrastructure.GameStates
{
    public class LoadLevelState : IGameState
    {
        private readonly SceneLoader sceneLoader;
        private readonly MapCreator mapCreator;
        private readonly PlayerFactory playerFactory;
        private readonly CameraCreator cameraCreator;
        private readonly StructureSpawner structureSpawner;
        private readonly UIFactory uiFactory;
        private readonly SaveLoadService saveLoadService;
        private readonly GameStateMachine gameStateMachine;
        private readonly LevelCreationWatcher levelCreationWatcher;
        private readonly ChunkUpdater chunkUpdater;
        private readonly ScoreTracker scoreTracker;


        public LoadLevelState(SceneLoader sceneLoader, MapCreator mapCreator, PlayerFactory playerFactory,
            CameraCreator cameraCreator, StructureSpawner structureSpawner, UIFactory uiFactory,
            SaveLoadService saveLoadService, GameStateMachine gameStateMachine, LevelCreationWatcher levelCreationWatcher)
        {
            this.sceneLoader = sceneLoader;
            this.mapCreator = mapCreator;
            this.playerFactory = playerFactory;
            this.cameraCreator = cameraCreator;
            this.structureSpawner = structureSpawner;
            this.uiFactory = uiFactory;
            this.saveLoadService = saveLoadService;
            this.gameStateMachine = gameStateMachine;
            this.levelCreationWatcher = levelCreationWatcher;
        }


        public void Enter()
        {
            saveLoadService.Cleanup();
            sceneLoader.Load(RuntimeConstants.Scenes.GAME_SCENE, CreateLevel);
            saveLoadService.UpdateProgress();

            gameStateMachine.Enter<GameLoopState>();
        }


        private void CreateLevel()
        {
            CreateMap();

            GameObject player = CreatePlayerWithControls();

            CreateCameras();
            SetScoreAndCurrencyUI(player);

            CreateStructures();

            levelCreationWatcher.LevelCreated();
        }


        private void CreateStructures()
        {
            structureSpawner.SpawnWalls();
            structureSpawner.ActivateAllSpawners();
        }


        private void CreateCameras()
        {
            Transform cameraPivot = GameObject.FindWithTag("CameraPivot").transform;
            SetVirtualCamera(cameraPivot);
        }


        private GameObject CreatePlayerWithControls()
        {
            PlayerControlsUI playerControlsUI = uiFactory.CreatePlayerControlsUI();
            GameObject player = playerFactory.CreatePlayer(new Vector3(50, 100, 50), playerControlsUI);
            return player;
        }


        private void CreateMap()
        {
            levelCreationWatcher.MapGenerationStarted();
            mapCreator.CreateMap();
        }


        private void SetScoreAndCurrencyUI(GameObject player)
        {
            CoinCollector coinCollector = player.GetComponent<CoinCollector>();
            ObjectsDestroyer objectsDestroyer = player.GetComponent<ObjectsDestroyer>();
            Camera uiCamera = cameraCreator.CreateUICamera();
            GameObject scoreAndCurrencyUI = uiFactory.CreateScoreAndCurrencyUI(uiCamera);

            CoinsUI coinsUI = scoreAndCurrencyUI.GetComponent<CoinsUI>();
            ScoreUI scoreUI = scoreAndCurrencyUI.GetComponent<ScoreUI>();

            coinsUI.Construct(coinCollector, uiCamera);
            scoreUI.Construct(objectsDestroyer, uiCamera);

            cameraCreator.StackCamera(uiCamera);
        }


        private void SetVirtualCamera(Transform cameraPivot)
        {
            CinemachineVirtualCamera virtualCamera = cameraCreator.CreateVirtualCamera();
            cameraCreator.SetUpVirtualCamera(virtualCamera, cameraPivot);
        }
    }
}
