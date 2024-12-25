using UnityEngine;
using System.Collections;
using System.Collections.Generic;
namespace GeNa.Core
{
    /// <summary>
    /// Decorator for performing Physics-based Spawning
    /// </summary>
    public class GeNaPhysicsDecorator : GeNaDecorator
    {
        [SerializeField] protected PhysicsSimulatorSettings m_settings = new PhysicsSimulatorSettings();

        public PhysicsSimulatorSettings Settings
        {
            get => m_settings;
            set => m_settings = value;
        }

        public override void OnIngest(Resource resource)
        {
            resource.PhysicsEnabled = true;
        }

        public override IEnumerator OnSelfSpawned(Resource resource)
        {
            GeNaSpawnerData spawnerData = resource.SpawnerData;
            List<ResourceEntity> entities = new List<ResourceEntity>();
            Vector3 location = transform.position;
            Vector3 rotation = transform.eulerAngles;
            Vector3 scale = transform.localScale;
            ResourceEntity spawnedEntity = GeNaSpawnerInternal.GetResourceEntity(location, rotation, scale, resource);

            GameObject spawnProgress = GeNaSpawnerInternal.GetSpawnProgressParent(spawnerData);
            if (spawnedEntity != null)
            {
                spawnedEntity.GameObject = gameObject;
                entities.Add(spawnedEntity);
                // transform.SetParent(spawnProgress.transform);
            }

            // This system needs to be adjusted to work at runtime as well.
            if (spawnerData.PhysicsType == Constants.PhysicsType.Resource && !GeNaGlobalReferences.GeNaManagerInstance.Cancel)
            {
                IEnumerator simulateMethod = GeNaEvents.Simulate(entities, m_settings, this);
                while (simulateMethod.MoveNext())
                {
                    yield return simulateMethod.Current;
                }
            }
        }
    }
}