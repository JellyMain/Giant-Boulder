using Progress;
using Structures;
using UnityEngine;
using Zenject;


namespace DataTrackers
{
    public class StatsTracker : IInitializable, IProgressSaver, IProgressUpdater
    {
        private readonly SaveLoadService saveLoadService;

        public DestroyedObjectsCountDictionary DestroyedObjectsCount { get; private set; } =
            new DestroyedObjectsCountDictionary();

        public int CoinsCollected { get; private set; }



        public StatsTracker(SaveLoadService saveLoadService)
        {
            this.saveLoadService = saveLoadService;
        }


        public void Initialize()
        {
            saveLoadService.RegisterGlobalObject(this);
        }


        public void AddCoin()
        {
            CoinsCollected++;
        }


        public void AddDestroyedObjectByType(ObjectType objectType)
        {
            if (objectType != ObjectType.None)
            {
                DestroyedObjectsCount.TryAdd(objectType, 0);
                DestroyedObjectsCount[objectType]++;
            }
            else
            {
                Debug.LogError($"Object has type {objectType}");
            }
        }


        public void SaveProgress(PlayerProgress playerProgress)
        {
            playerProgress.statsData.destroyedObjectsCount = DestroyedObjectsCount;
            playerProgress.statsData.coinsCollected = CoinsCollected;
        }


        public void UpdateProgress(PlayerProgress playerProgress)
        {
            DestroyedObjectsCount = playerProgress.statsData.destroyedObjectsCount;
            CoinsCollected = playerProgress.statsData.coinsCollected;
        }
    }
}
