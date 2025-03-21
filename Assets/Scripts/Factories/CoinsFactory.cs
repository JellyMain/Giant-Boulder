using Coins;
using Const;
using Sounds;
using UnityEngine;


namespace Factories
{
    public class CoinsFactory
    {
        private readonly SoundPlayer soundPlayer;
        private Coin coinPrefab;


        public CoinsFactory(SoundPlayer soundPlayer)
        {
            this.soundPlayer = soundPlayer;
            LoadPrefab();
        }


        private void LoadPrefab()
        {
            coinPrefab = Resources.Load<Coin>(RuntimeConstants.PrefabPaths.COIN);
        }


        public Coin CreateCoin(Vector3 position)
        {
            Coin spawnedCoin = Object.Instantiate(coinPrefab, position, Quaternion.identity);
            spawnedCoin.Construct(soundPlayer);
            return spawnedCoin;
        }
    }
}
