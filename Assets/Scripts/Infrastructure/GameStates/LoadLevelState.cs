using Cinemachine;
using Const;
using DataTrackers;
using Factories;
using Infrastructure.GameStates.Interfaces;
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
        private readonly DiContainer diContainer;
        private readonly UIFactory uiFactory;
        private readonly SaveLoadService saveLoadService;
        private readonly ScoreTracker scoreTracker;


        public LoadLevelState(SceneLoader sceneLoader, MapCreator mapCreator, PlayerFactory playerFactory,
            CameraCreator cameraCreator, StructureSpawner structureSpawner, DiContainer diContainer,
            UIFactory uiFactory, SaveLoadService saveLoadService)
        {
            this.sceneLoader = sceneLoader;
            this.mapCreator = mapCreator;
            this.playerFactory = playerFactory;
            this.cameraCreator = cameraCreator;
            this.structureSpawner = structureSpawner;
            this.diContainer = diContainer;
            this.uiFactory = uiFactory;
            this.saveLoadService = saveLoadService;

            //TODO: Remove di container and move Chunk Updater to another place; 
        }


        public void Enter()
        {
            saveLoadService.Cleanup();
            sceneLoader.Load(RuntimeConstants.Scenes.GAME_SCENE, CreateLevel);
            saveLoadService.UpdateProgress();
        }


        private void CreateLevel()
        {
            mapCreator.CreateMap();
            
            PlayerControlsUI playerControlsUI = uiFactory.CreatePlayerControlsUI();

            GameObject player = playerFactory.CreatePlayer(new Vector3(50, 100, 50), playerControlsUI.lookArea);
            Transform cameraPivot = GameObject.FindWithTag("CameraPivot").transform;
            SetVirtualCamera(cameraPivot);

            SetScoreAndCurrencyUI(player);

            structureSpawner.SpawnWalls();
            structureSpawner.ActivateAllSpawners();

            ChunkUpdater chunkUpdater = CreateChunkUpdater();
            chunkUpdater.player = player.transform;
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


        private ChunkUpdater CreateChunkUpdater()
        {
            ChunkUpdater chunkUpdaterPrefab = Resources.Load<ChunkUpdater>("RuntimePrefabs/ChunkUpdater");
            GameObject parentObject = new GameObject("ChunkUpdater");
            return diContainer.InstantiatePrefab(chunkUpdaterPrefab, parentObject.transform)
                .GetComponent<ChunkUpdater>();
        }
    }
}
