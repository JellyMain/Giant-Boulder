using System.Collections.Generic;
using Progress;
using Structures;
using Zenject;


namespace Stats
{
    public class StatsService : IInitializable, IProgressSaver, IProgressUpdater
    {
        private readonly SaveLoadService saveLoadService;

        public DestroyedObjectsCountDictionary DestroyedObjectsCount { get; private set; } =
            new DestroyedObjectsCountDictionary();
        
        public int CoinsCollected { get; private set; }



        public StatsService(SaveLoadService saveLoadService)
        {
            this.saveLoadService = saveLoadService;
        }


        public void Initialize()
        {
            saveLoadService.RegisterGlobalObject(this);
        }


        public void AddCoins(int coinsToAdd)
        {
            CoinsCollected += coinsToAdd;
        }


        public void AddDestroyedObjects(DestroyedObjectsCountDictionary destroyedObjects)
        {
            foreach (KeyValuePair<ObjectType, int> objectPair in destroyedObjects)
            {
                DestroyedObjectsCount.TryAdd(objectPair.Key, 0);
                DestroyedObjectsCount[objectPair.Key] += objectPair.Value;
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
