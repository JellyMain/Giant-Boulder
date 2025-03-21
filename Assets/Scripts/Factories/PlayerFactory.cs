using System;
using Const;
using PlayerCamera;
using StaticData.Services;
using UI;
using UnityEngine;
using Zenject;


namespace Factories
{
    public class PlayerFactory
    {
        private readonly DiContainer diContainer;
        public event Action<GameObject> OnPlayerCreated;


        public PlayerFactory(DiContainer diContainer)
        {
            this.diContainer = diContainer;
        }


        public GameObject CreatePlayer(Vector3 position, PlayerControlsUI playerControlsUI)
        {
            GameObject playerPrefab = Resources.Load<GameObject>(RuntimeConstants.PrefabPaths.PLAYER);

            GameObject spawnedPlayer = diContainer.InstantiatePrefab(playerPrefab, position, Quaternion.identity,
                new GameObject("Player").transform);

            ThirdPersonCameraController cameraController = spawnedPlayer.GetComponent<ThirdPersonCameraController>();

            cameraController.PlayerControlsUI = playerControlsUI;

            OnPlayerCreated?.Invoke(spawnedPlayer);

            return spawnedPlayer;
        }
    }
}
