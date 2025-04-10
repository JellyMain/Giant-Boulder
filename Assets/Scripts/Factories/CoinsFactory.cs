using Assets;
using Coins;
using Const;
using Cysharp.Threading.Tasks;
using Sounds;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Zenject;


namespace Factories
{
    public class CoinsFactory : IInitializable
    {
        private readonly SoundPlayer soundPlayer;
        private readonly AssetProvider assetProvider;


        public CoinsFactory(SoundPlayer soundPlayer, AssetProvider assetProvider)
        {
            this.soundPlayer = soundPlayer;
            this.assetProvider = assetProvider;
        }


        public void Initialize()
        {
            WarmUpPrefab().Forget();
        }


        private async UniTaskVoid WarmUpPrefab()
        {
            await assetProvider.LoadAsset<GameObject>(RuntimeConstants.PrefabAddresses.COIN);
        }


        public async UniTask<Coin> CreateCoin(Vector3 position)
        {
            GameObject coinPrefab = await assetProvider.LoadAsset<GameObject>(RuntimeConstants.PrefabAddresses.COIN);

            Coin spawnedCoin = Object.Instantiate(coinPrefab, position, Quaternion.identity).GetComponent<Coin>();
            spawnedCoin.Construct(soundPlayer);
            return spawnedCoin;
        }
    }
}
