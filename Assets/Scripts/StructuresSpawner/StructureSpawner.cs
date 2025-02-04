using System;
using System.Collections;
using System.Collections.Generic;
using StaticData.Data;
using StaticData.Services;
using Structures;
using UnityEngine;
using Zenject;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;


namespace StructuresSpawner
{
    public class StructureSpawner
    {
        private readonly Collider[] collidersAllocation = new Collider[1];
        private readonly StaticDataService staticDataService;
        private float maxXPosition;
        private float maxYPosition;
        private int spawnerCounter;


        public StructureSpawner(StaticDataService staticDataService)
        {
            this.staticDataService = staticDataService;
        }


        private void Init()
        {
            spawnerCounter = 0;

            MapGenerationConfig mapGenerationConfig = staticDataService.MapConfigForSeason(TerrainSeason.Summer);

            maxXPosition = mapGenerationConfig.mapSize * (mapGenerationConfig.chunkSize - 1) -
                           (mapGenerationConfig.chunkSize - 1);

            maxYPosition = mapGenerationConfig.mapSize * (mapGenerationConfig.chunkSize - 1) -
                           (mapGenerationConfig.chunkSize - 1);
        }


        public void ActivateAllSpawners()
        {
            Init();

            for (int i = 0; i < staticDataService.SpawnerConfigs.Count; i++)
            {
                SpawnerConfig spawnerConfig = staticDataService.SpawnerConfigForSpawnOrder(spawnerCounter);
                ActivateSpawner(spawnerConfig);
            }
        }


        private void ActivateSpawner(SpawnerConfig spawnerConfig)
        {
            foreach (StructureRoot structurePrefab in spawnerConfig.structurePrefabCountPair.Keys)
            {
                for (int i = 0; i < spawnerConfig.structurePrefabCountPair[structurePrefab]; i++)
                {
                    SpawnStructure(structurePrefab, spawnerConfig);
                }
            }

            spawnerCounter++;
        }



        private void SpawnStructure(StructureRoot structurePrefab, SpawnerConfig spawnerConfig)
        {
            bool foundValidPosition = false;

            for (int i = 0; i < spawnerConfig.spawnAttempts; i++)
            {
                if (foundValidPosition)
                {
                    break;
                }

                float positionX = Random.Range(0, maxXPosition);
                float positionZ = Random.Range(0, maxYPosition);

                Vector3 position = new Vector3(positionX, 0, positionZ);

                Vector3 rayStart = position + Vector3.up * spawnerConfig.raycastHeight;

                if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, Mathf.Infinity,
                        spawnerConfig.groundLayer))
                {
                    if (!IsSlopeSteep(rayStart, structurePrefab.StructureRadius,
                            spawnerConfig.slopeCheckRaysAmount, structurePrefab.MaxSlopeAngle, spawnerConfig))
                    {
                        if (!HasStructuresInRadius(hit.point, structurePrefab.StructureRadius, spawnerConfig))
                        {
                            StructureRoot spawnedObject = Object.Instantiate(structurePrefab);

                            spawnedObject.transform.position = hit.point;

                            spawnedObject.transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);

                            ApplyStructureSettings(spawnedObject.structureChildSettings, spawnerConfig);

                            foundValidPosition = true;

                            Physics.SyncTransforms();
                        }
                    }
                }
                else
                {
                    Debug.LogWarning("No ground detected at the specified position.");
                }
            }

            if (!foundValidPosition)
            {
                Debug.LogWarning($"Haven't found valid position in {spawnerConfig.spawnAttempts} attempts");
            }
        }


        private void ApplyStructureSettings(List<StructureSpawnSettings> structureSpawnSettings,
            SpawnerConfig spawnerConfig)
        {
            foreach (StructureSpawnSettings childObject in structureSpawnSettings)
            {
                if (childObject.snapToGround)
                {
                    SnapToGround(childObject.transform, spawnerConfig);
                }

                int randomNumber = Random.Range(0, 100);

                if (randomNumber > childObject.SpawnChance)
                {
                    Object.Destroy(childObject.gameObject);
                    continue;
                }

                if (childObject.RotationModifier != Vector2.zero)
                {
                    float objectYRotation =
                        Random.Range(childObject.RotationModifier.x, childObject.RotationModifier.y);

                    childObject.transform.localRotation = Quaternion.Euler(childObject.transform.rotation.x,
                        objectYRotation,
                        childObject.transform.rotation.z);
                }
            }
        }



        private void SnapToGround(Transform objectToSnap, SpawnerConfig spawnerConfig)
        {
            Vector3 rayStart = objectToSnap.position + Vector3.up * spawnerConfig.raycastHeight;

            if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, Mathf.Infinity, spawnerConfig.groundLayer))
            {
                float currentYRotation = objectToSnap.eulerAngles.y;

                Quaternion groundAlignment = Quaternion.FromToRotation(Vector3.up, hit.normal);

                objectToSnap.rotation = groundAlignment * Quaternion.Euler(0, currentYRotation, 0);

                objectToSnap.position = hit.point;
            }
        }


        private bool IsSlopeSteep(Vector3 position, float radius, int raysCount, float maxSlopeAngle,
            SpawnerConfig spawnerConfig)
        {
            float totalAngle = 0;

            for (int i = 0; i < raysCount; i++)
            {
                Vector2 randomOffset = Random.insideUnitCircle * radius;
                Vector3 randomPositionInRadius =
                    new Vector3(randomOffset.x + position.x, position.y, randomOffset.y + position.z);

                if (Physics.Raycast(randomPositionInRadius, Vector3.down, out RaycastHit hit, Mathf.Infinity,
                        spawnerConfig.groundLayer))
                {
                    float angle = Vector3.Angle(hit.normal, Vector3.up);
                    totalAngle += angle;
                }
            }

            totalAngle /= raysCount;

            return totalAngle > maxSlopeAngle;
        }


        private bool HasStructuresInRadius(Vector3 position, float radius, SpawnerConfig spawnerConfig)
        {
            int collidersNumber = Physics.OverlapSphereNonAlloc(position, radius, collidersAllocation,
                spawnerConfig.structuresLayer);

            return collidersNumber > 0;
        }
    }
}
