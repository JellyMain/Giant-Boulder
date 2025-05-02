using System;
using Coins;
using Cysharp.Threading.Tasks;
using Factories;
using UnityEngine;
using Utils;
using Zenject;
using Random = UnityEngine.Random;


namespace Structures
{
    public class CoinSpawner : MonoBehaviour
    {
        [SerializeField] private int maxCoinsAmount = 10;
        [SerializeField] private int minCoinsAmount = 5;
        [SerializeField] private float spawnCircleDiameter = 5;
        [SerializeField] private float explosionForce = 40;
        private Collider col;
        private CoinsFactory coinsFactory;
        private DestructibleObjectBase destructibleObjectBase;
        private Vector3 objectCenter;


        [Inject]
        private void Construct(CoinsFactory coinsFactory)
        {
            this.coinsFactory = coinsFactory;
        }


        private void OnEnable()
        {
            destructibleObjectBase.OnDestroyed += SpawnCoins;
        }


        private void OnDisable()
        {
            destructibleObjectBase.OnDestroyed -= SpawnCoins;
        }


        private void Awake()
        {
            col = GetComponent<Collider>();
            destructibleObjectBase = GetComponent<DestructibleObjectBase>();
        }


        private void Start()
        {
            objectCenter = RuntimeMeshUtility.GetMeshCenter(transform, col);
        }



        private async void SpawnCoins(DestructibleObjectBase _)
        {
            int coinsAmount = Random.Range(minCoinsAmount, maxCoinsAmount);

            for (int i = 0; i < coinsAmount; i++)
            {
                Vector2 randomPositionInUnitCircle = Random.insideUnitCircle * spawnCircleDiameter;
                Vector3 randomPosition = new Vector3(randomPositionInUnitCircle.x, 0, randomPositionInUnitCircle.y) +
                                         objectCenter;

                Coin coin = await coinsFactory.CreateCoin(randomPosition);

                coin.Rb.AddExplosionForce(explosionForce,
                    new Vector3(objectCenter.x, objectCenter.y - 3, objectCenter.z), explosionForce);
            }
        }
    }
}
