using System;
using Factories;
using GameLoop;
using Player;
using Progress;
using Stats;
using Structures;
using UnityEngine;


namespace DataTrackers
{
    public class DestroyedObjectsTracker : IDisposable
    {
        private readonly PlayerFactory playerFactory;
        private readonly StatsService statsService;
        private readonly GameLoopStatesHandler gameLoopStatesHandler;
        private ObjectsDestroyer objectsDestroyer;
        public event Action<ObjectType> OnDestroyedObjectAdded;

        private readonly DestroyedObjectsCountDictionary destroyedObjects = new DestroyedObjectsCountDictionary();


        public DestroyedObjectsTracker(PlayerFactory playerFactory, StatsService statsService,
            GameLoopStatesHandler gameLoopStatesHandler)
        {
            this.playerFactory = playerFactory;
            this.statsService = statsService;
            this.gameLoopStatesHandler = gameLoopStatesHandler;
        }


        public void Init()
        {
            gameLoopStatesHandler.OnGameSessionOver += PassDestroyedObjects;
            SetPlayer();
        }


        private void SetPlayer()
        {
            if (playerFactory.Player != null)
            {
                SetAndSubscribeObjectDestroyer(playerFactory.Player);
            }
            else
            {
                playerFactory.OnPlayerCreated += SetAndSubscribeObjectDestroyer;
            }
        }


        public void Dispose()
        {
            playerFactory.OnPlayerCreated -= SetAndSubscribeObjectDestroyer;
            gameLoopStatesHandler.OnGameSessionOver -= PassDestroyedObjects;
        }


        private void SetAndSubscribeObjectDestroyer(GameObject player)
        {
            objectsDestroyer = player.GetComponent<ObjectsDestroyer>();
            objectsDestroyer.OnObjectDestroyed += AddDestroyedObject;
        }


        private void AddDestroyedObject(ObjectType objectType)
        {
            AddDestroyedObjectByType(objectType);
            OnDestroyedObjectAdded?.Invoke(objectType);
        }


        private void PassDestroyedObjects()
        {
            statsService.AddDestroyedObjects(destroyedObjects);
        }


        private void AddDestroyedObjectByType(ObjectType objectType)
        {
            if (objectType != ObjectType.None)
            {
                destroyedObjects.TryAdd(objectType, 0);
                destroyedObjects[objectType]++;
            }
            else
            {
                Debug.LogError($"Object has type {objectType}");
            }
        }
    }
}
