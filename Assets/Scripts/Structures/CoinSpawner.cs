using System;
using Coins;
using Factories;
using UnityEngine;
using Zenject;
using Random = UnityEngine.Random;


namespace Structures
{
    public class CoinSpawner : MonoBehaviour
    {
        [SerializeField] private int maxCoinsAmount = 5;
        [SerializeField] private int minCoinsAmount = 10;
        [SerializeField] private float spawnCircleDiameter = 5;
        [SerializeField] private float explosionForce = 10;
        private CoinsFactory coinsFactory;
        private StructureBase structureBase;
        private Vector3 objectCenter;


        [Inject]
        private void Construct(CoinsFactory coinsFactory)
        {
            this.coinsFactory = coinsFactory;
        }


        private void OnEnable()
        {
            structureBase.OnBuildingDestroyed += SpawnCoins;
        }


        private void OnDisable()
        {
            structureBase.OnBuildingDestroyed -= SpawnCoins;
        }


        private void Awake()
        {
            structureBase = GetComponent<StructureBase>();
        }


        private void Start()
        {
            objectCenter = GetMeshCenter();
        }



        private Vector3 GetMeshCenter()
        {
            MeshFilter meshFilter = GetComponent<MeshFilter>();

            Vector3 localCenter = meshFilter.sharedMesh.bounds.center;

            return transform.TransformPoint(localCenter);
        }


        private void SpawnCoins()
        {
            int coinsAmount = Random.Range(minCoinsAmount, maxCoinsAmount);

            for (int i = 0; i < coinsAmount; i++)
            {
                Vector2 randomPositionInUnitCircle = Random.insideUnitCircle * spawnCircleDiameter;
                Vector3 randomPosition = new Vector3(randomPositionInUnitCircle.x, 0, randomPositionInUnitCircle.y) +
                                         objectCenter;

                Coin coin = coinsFactory.CreateCoin(randomPosition);

                coin.Rb.AddExplosionForce(explosionForce,
                    new Vector3(objectCenter.x, objectCenter.y - 3, objectCenter.z), spawnCircleDiameter * 2);
            }
        }
    }
}
