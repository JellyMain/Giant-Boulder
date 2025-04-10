using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Zenject;


namespace Assets
{
    public class AssetProvider : IInitializable
    {
        private readonly Dictionary<string, AsyncOperationHandle> assetsCache =
            new Dictionary<string, AsyncOperationHandle>();

        private readonly Dictionary<string, List<AsyncOperationHandle>> allHandles =
            new Dictionary<string, List<AsyncOperationHandle>>();


        public void Initialize()
        {
            WarmUp();
        }


        private void WarmUp()
        {
            Addressables.InitializeAsync();
        }


        public async UniTask<IList<T>> LoadAssets<T>(string assetsFolderAddress, Action<T> everyAssetCallback)
            where T : class
        {
            AsyncOperationHandle<IList<T>> handle =
                Addressables.LoadAssetsAsync(assetsFolderAddress, everyAssetCallback);


            if (!allHandles.ContainsKey(assetsFolderAddress))
            {
                allHandles[assetsFolderAddress] = new List<AsyncOperationHandle>();
            }

            allHandles[assetsFolderAddress].Add(handle);

            return await handle.Task;
        }


        public async UniTask<T> LoadAsset<T>(string assetAddress) where T : class
        {
            if (assetsCache.TryGetValue(assetAddress, out AsyncOperationHandle assetHandle))
            {
                return assetHandle.Result as T;
            }

            AsyncOperationHandle<T> handle = Addressables.LoadAssetAsync<T>(assetAddress);

            if (!allHandles.ContainsKey(assetAddress))
            {
                allHandles[assetAddress] = new List<AsyncOperationHandle>();
            }

            allHandles[assetAddress].Add(handle);

            handle.Completed += (asyncOperationHandle) => { assetsCache[assetAddress] = asyncOperationHandle; };

            return await handle.Task;
        }
    }
}
