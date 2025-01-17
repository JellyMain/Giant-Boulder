using System;
using Coins;
using UnityEngine;


namespace Player
{
    public class CoinCollector : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out Coin coin))
            {
                Destroy(coin.gameObject);
            }
        }
    }
}
