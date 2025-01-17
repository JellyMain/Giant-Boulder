using Const;
using StaticData.Services;
using UnityEngine;
using Zenject;


namespace Factories
{
    public class PlayerFactory
    {
        private readonly DiContainer diContainer;


        public PlayerFactory(StaticDataService staticDataService, DiContainer diContainer)
        {
            this.diContainer = diContainer;
        }


        public GameObject CreatePlayer(Vector3 position)
        {
            GameObject player = Resources.Load<GameObject>(RuntimeConstants.PrefabPaths.PLAYER);

            return diContainer.InstantiatePrefab(player, position, Quaternion.identity,
                new GameObject("Player").transform);
        }
    }
}