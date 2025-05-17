using System;
using Coins;
using Const;
using DataTrackers;
using UnityEngine;
using Upgrades;
using Zenject;


namespace Player
{
    public class CoinCollector : MonoBehaviour
    {
        [SerializeField] private float coinCollectionRadius = 1;
        [SerializeField] private float magnetRadius = 10;
        [SerializeField] private float maxMagnetPower = 5;
        private UpgradesService upgradesService;
        private readonly Collider[] magnetCoinsBuffer = new Collider[50];
        private readonly Collider[] collectedCoinsBuffer = new Collider[50];
        private int coinLayerMask;
        public event Action<Vector3> OnCoinCollected;


        [Inject]
        private void Construct(UpgradesService upgradesService)
        {
            this.upgradesService = upgradesService;
        }


        private void Start()
        {
            coinLayerMask = LayerMask.NameToLayer(RuntimeConstants.Layers.COIN_LAYER);
            coinLayerMask = 1 << coinLayerMask;
            ApplyUpgrade();
        }


        private void Update()
        {
            MagnetCoins();
        }


        private void FixedUpdate()
        {
            CollectCoins();
        }


        private void ApplyUpgrade()
        {
            float valueToAdd = 0;
            
            if (upgradesService.ActiveUpgrades.TryGetValue(UpgradeType.CoinsMagnet, out UpgradeData upgradeData))
            {
                CoinsMagnetUpgradeData coinsMagnetUpgradeData =
                    upgradeData as CoinsMagnetUpgradeData;
                
                valueToAdd = magnetRadius / 100 * coinsMagnetUpgradeData.percentUpgrade;
            }
            
            Debug.Log(valueToAdd);
            
            magnetRadius += valueToAdd;
        }


        private void CollectCoins()
        {
            Physics.OverlapSphereNonAlloc(transform.position, coinCollectionRadius, collectedCoinsBuffer,
                coinLayerMask);

            foreach (Collider coinCollider in collectedCoinsBuffer)
            {
                if (coinCollider != null)
                {
                    Coin coin = coinCollider.GetComponent<Coin>();
                    coin.Destroy();
                    OnCoinCollected?.Invoke(coin.transform.position);
                }
            }
        }


        private void MagnetCoins()
        {
            Physics.OverlapSphereNonAlloc(transform.position, magnetRadius, magnetCoinsBuffer, coinLayerMask);

            foreach (Collider coinCollider in magnetCoinsBuffer)
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


        private void OnTriggerEnter(Collider other) { }


        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, magnetRadius);
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, coinCollectionRadius);
        }
    }
}
