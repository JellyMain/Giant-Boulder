using System;
using Assets;
using Const;
using Cysharp.Threading.Tasks;
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
        private readonly AssetProvider assetProvider;
        public event Action<GameObject> OnPlayerCreated;


        public PlayerFactory(DiContainer diContainer, AssetProvider assetProvider)
        {
            this.diContainer = diContainer;
            this.assetProvider = assetProvider;
        }


        public async UniTask<GameObject> CreatePlayer(Vector3 position, PlayerControlsUI playerControlsUI)
        {
            GameObject playerPrefab =
                await assetProvider.LoadAsset<GameObject>(RuntimeConstants.PrefabAddresses.PLAYER);

            GameObject spawnedPlayer = diContainer.InstantiatePrefab(playerPrefab, position, Quaternion.identity,
                new GameObject("Player").transform);

            ThirdPersonCameraController cameraController = spawnedPlayer.GetComponent<ThirdPersonCameraController>();

            cameraController.PlayerControlsUI = playerControlsUI;

            OnPlayerCreated?.Invoke(spawnedPlayer);

            return spawnedPlayer;
        }
    }
}
