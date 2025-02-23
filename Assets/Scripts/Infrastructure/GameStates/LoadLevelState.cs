using Cinemachine;
using Const;
using Factories;
using Infrastructure.GameStates.Interfaces;
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


        public LoadLevelState(SceneLoader sceneLoader, MapCreator mapCreator, PlayerFactory playerFactory,
            CameraCreator cameraCreator, StructureSpawner structureSpawner, DiContainer diContainer)
        {
            this.sceneLoader = sceneLoader;
            this.mapCreator = mapCreator;
            this.playerFactory = playerFactory;
            this.cameraCreator = cameraCreator;
            this.structureSpawner = structureSpawner;
            this.diContainer = diContainer;

            //TODO: Remove di container and move Chunk Updater to another place; 
        }


        public void Enter()
        {
            sceneLoader.Load(RuntimeConstants.Scenes.GAME_SCENE, CreateLevel);
        }


        private void CreateLevel()
        {
            mapCreator.CreateMap();

            GameObject player = playerFactory.CreatePlayer(new Vector3(50, 100, 50));
            Transform cameraPivot = GameObject.FindWithTag("CameraPivot").transform;
            SetCamera(cameraPivot);

            structureSpawner.ActivateAllSpawners();

            ChunkUpdater chunkUpdater = CreateChunkUpdater();
            chunkUpdater.player = player.transform;
        }


        private void SetCamera(Transform cameraPivot)
        {
            CinemachineVirtualCamera virtualCamera = cameraCreator.CreateVirtualCamera();
            cameraCreator.SetUpVirtualCamera(virtualCamera, cameraPivot);
        }


        private ChunkUpdater CreateChunkUpdater()
        {
            ChunkUpdater chunkUpdaterPrefab = Resources.Load<ChunkUpdater>("RuntimePrefabs/ChunkUpdater");
            GameObject parentObject = new GameObject("ChunkUpdater");
            return diContainer.InstantiatePrefab(chunkUpdaterPrefab, parentObject.transform).GetComponent<ChunkUpdater>();
        }
    }
}
