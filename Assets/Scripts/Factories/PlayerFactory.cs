using System;
using Assets;
using Const;
using Cysharp.Threading.Tasks;
using PlayerCamera;
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
        public GameObject Player { get; private set; }


        public PlayerFactory(DiContainer diContainer, AssetProvider assetProvider)
        {
            this.diContainer = diContainer;
            this.assetProvider = assetProvider;
        }


        public async UniTask<GameObject> CreatePlayer(Vector3 position, PlayerControlsUI playerControlsUI)
        {
            GameObject playerPrefab =
                await assetProvider.LoadAsset<GameObject>(RuntimeConstants.PrefabAddresses.PLAYER);


            Vector3 rayStart = position + Vector3.up * 1000;

            int groundLayerMask = 1 << LayerMask.NameToLayer(RuntimeConstants.Layers.GROUND_LAYER);
            
            Vector3 spawnPosition = position;

            if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, Mathf.Infinity, groundLayerMask))
            {
                spawnPosition = hit.point + Vector3.up;
            }
            else
            {
                Debug.LogError("Didn't found ground under player");
            }

            GameObject spawnedPlayer = diContainer.InstantiatePrefab(playerPrefab, spawnPosition, Quaternion.identity,
                new GameObject("Player").transform);

            ThirdPersonCameraController cameraController = spawnedPlayer.GetComponent<ThirdPersonCameraController>();

            cameraController.PlayerControlsUI = playerControlsUI;

            OnPlayerCreated?.Invoke(spawnedPlayer);

            Player = spawnedPlayer;

            return spawnedPlayer;
        }
    }
}
