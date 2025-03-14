using System;
using Coins;
using Const;
using DataTrackers;
using UnityEngine;
using Zenject;


namespace Player
{
    public class CoinCollector : MonoBehaviour
    {
        [SerializeField] private float magnetRadius = 10;
        [SerializeField] private float maxMagnetPower = 5;
        private readonly Collider[] coinCollidersBuffer =  new Collider[50];
        private int coinLayerMask;
        private CurrencyTracker currencyTracker;
        public event Action<Vector3> OnCoinCollected;


        [Inject]
        private void Construct(CurrencyTracker currencyTracker)
        {
            this.currencyTracker = currencyTracker;
        }


        private void Start()
        {
            coinLayerMask = LayerMask.NameToLayer(RuntimeConstants.Layers.COIN_LAYER);
            coinLayerMask = 1 << coinLayerMask;
        }


        private void Update()
        {
            MagnetCoins();
        }


        private void MagnetCoins()
        {
            Physics.OverlapSphereNonAlloc(transform.position, magnetRadius, coinCollidersBuffer, coinLayerMask);

            foreach (Collider coinCollider in coinCollidersBuffer)
            {
                if (coinCollider != null)
                {
                    float distanceToPlayer = Vector3.Distance(coinCollider.transform.position, transform.position);
                    float magnetPower = Mathf.Lerp(0, maxMagnetPower, 1 - distanceToPlayer / magnetRadius);

                    coinCollider.transform.position = Vector3.MoveTowards(coinCollider.transform.position,
                        transform.position, magnetPower * Time.deltaTime);
                }
            }
        }


        private void OnCollisionEnter(Collision other)
        {
            if (other.gameObject.TryGetComponent(out Coin coin))
            {
                coin.Destroy();
                OnCoinCollected?.Invoke(coin.transform.position);
            }
        }


        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, magnetRadius);
        }
    }
}
