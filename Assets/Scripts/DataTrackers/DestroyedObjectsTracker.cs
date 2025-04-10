using System;
using Factories;
using Player;
using Structures;
using UnityEngine;
using Zenject;


namespace DataTrackers
{
    public class DestroyedObjectsTracker: IInitializable, IDisposable
    {
        private readonly PlayerFactory playerFactory;
        private readonly StatsTracker statsTracker;
        private ObjectsDestroyer objectsDestroyer;
        public event Action<ObjectType> OnDestroyedObjectAdded;

        
        public DestroyedObjectsTracker(PlayerFactory playerFactory, StatsTracker statsTracker)
        {
            this.playerFactory = playerFactory;
            this.statsTracker = statsTracker;
        }


        public void Initialize()
        {
            playerFactory.OnPlayerCreated += SetAndSubscribeObjectDestroyer;
        }
        
        
        public void Dispose()
        {
            playerFactory.OnPlayerCreated -= SetAndSubscribeObjectDestroyer;
        }
        

        private void SetAndSubscribeObjectDestroyer(GameObject player)
        {
            objectsDestroyer = player.GetComponent<ObjectsDestroyer>();
            objectsDestroyer.OnObjectDestroyed += AddDestroyedObject;
        }
        

        private void AddDestroyedObject(ObjectType objectType)
        {
            statsTracker.AddDestroyedObjectByType(objectType);
            OnDestroyedObjectAdded?.Invoke(objectType);
        }

        
        
        
    }
}
