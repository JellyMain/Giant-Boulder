using Cinemachine;
using Const;
using Factories;
using Infrastructure.GameStates.Interfaces;
using Infrastructure.Services;
using StructuresSpawner;
using TerrainGenerator;
using UI;
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
        private readonly SpawnPointsValidator spawnPointsValidator;


        public LoadLevelState(SceneLoader sceneLoader, MapCreator mapCreator, PlayerFactory playerFactory,
            CameraCreator cameraCreator, StructureSpawner structureSpawner, SpawnPointsValidator spawnPointsValidator)
        {
            this.sceneLoader = sceneLoader;
            this.mapCreator = mapCreator;
            this.playerFactory = playerFactory;
            this.cameraCreator = cameraCreator;
            this.structureSpawner = structureSpawner;
            this.spawnPointsValidator = spawnPointsValidator;
        }


        public void Enter()
        {
            sceneLoader.Load(RuntimeConstants.Scenes.GAME_SCENE, CreateLevel);
        }


        private void CreateLevel()
        {
            spawnPointsValidator.Init(); //TODO: Move initializer to other place
            
            mapCreator.CreateMap();
            GameObject player = playerFactory.CreatePlayer(new Vector3(50, 100, 50));
            Transform cameraPivot = GameObject.FindWithTag("CameraPivot").transform;
            
            SetCamera(cameraPivot);
            
            spawnPointsValidator.ComputeAllMeshesParallel();
            
            // structureSpawner.ActivateAllSpawners();
        }


        private void SetCamera(Transform cameraPivot)
        {
            CinemachineVirtualCamera virtualCamera = cameraCreator.CreateVirtualCamera();
            cameraCreator.SetUpVirtualCamera(virtualCamera, cameraPivot);
        }
    }
}