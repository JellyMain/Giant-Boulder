using Coins;
using Const;
using UnityEngine;


namespace Factories
{
    public class CoinsFactory
    {
        private Coin coinPrefab;


        public CoinsFactory()
        {
            LoadPrefab();
        }


        private void LoadPrefab()
        {
            coinPrefab = Resources.Load<Coin>(RuntimeConstants.PrefabPaths.COIN);
        }


        public Coin CreateCoin(Vector3 position)
        {
            return Object.Instantiate(coinPrefab, position, Quaternion.identity);
        }
    }
}
