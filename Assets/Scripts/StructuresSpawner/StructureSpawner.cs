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
        private StaticDataService staticDataService;
        private float maxXPosition;
        private float maxYPosition;
        private StructureSpawnerConfig structureSpawnerConfig;



        public StructureSpawner(StaticDataService staticDataService)
        {
            this.staticDataService = staticDataService;
        }


        private void Init()
        {
            structureSpawnerConfig = staticDataService.StructureSpawnerConfig;
            MapGenerationConfig mapGenerationConfig = staticDataService.MapConfigForSeason(TerrainSeason.Summer);

            maxXPosition = mapGenerationConfig.mapSize * (mapGenerationConfig.chunkSize - 1) -
                           (mapGenerationConfig.chunkSize - 1);

            maxYPosition = mapGenerationConfig.mapSize * (mapGenerationConfig.chunkSize - 1) -
                           (mapGenerationConfig.chunkSize - 1);
        }
        


        public void SpawnAllStructures()
        {
            Init();
            
            for (int i = 0; i < structureSpawnerConfig.structuresCount; i++)
            {
                SpawnRandomStructure();
            }
        }



        private void SpawnRandomStructure()
        {
            bool foundValidPosition = false;

            StructureRoot structurePrefab =
                structureSpawnerConfig.structurePrefabs[Random.Range(0, structureSpawnerConfig.structurePrefabs.Count)];

            for (int i = 0; i < structureSpawnerConfig.spawnAttempts; i++)
            {
                if (foundValidPosition)
                {
                    break;
                }

                float positionX = Random.Range(100, maxXPosition);
                float positionZ = Random.Range(100, maxYPosition);

                Vector3 position = new Vector3(positionX, 0, positionZ);

                Vector3 rayStart = position + Vector3.up * structureSpawnerConfig.raycastHeight;

                if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, Mathf.Infinity,
                        structureSpawnerConfig.groundLayer))
                {
                    if (!IsSlopeSteep(hit, structurePrefab.MaxSlopeAngle))
                    {
                        if (!HasStructuresInRadius(hit.point, structurePrefab.StructureRadius))
                        {
                            StructureRoot spawnedObject = Object.Instantiate(structurePrefab);

                            spawnedObject.transform.position = hit.point;

                            spawnedObject.transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);

                            ApplyStructureSettings(spawnedObject.structureChildSettings);

                            foundValidPosition = true;

                            Physics.SyncTransforms();
                        }
                    }
                    else
                    {
                        Debug.LogWarning("Has some structures there");
                    }
                }
                else
                {
                    Debug.LogWarning("No ground detected at the specified position.");
                }
            }

            if (!foundValidPosition)
            {
                Debug.LogWarning($"Haven't found valid position in {structureSpawnerConfig.spawnAttempts} attempts");
            }
        }


        private void ApplyStructureSettings(List<StructureChildSpawnSettings> structureSpawnSettings)
        {
            foreach (StructureChildSpawnSettings childObject in structureSpawnSettings)
            {
                if (childObject.SnapToGround)
                {
                    SnapToGround(childObject.transform, structureSpawnerConfig.groundLayer);
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



        private void SnapToGround(Transform objectToSnap, LayerMask groundLayer)
        {
            Vector3 rayStart = objectToSnap.position + Vector3.up * structureSpawnerConfig.raycastHeight;

            if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, Mathf.Infinity, groundLayer))
            {
                float currentYRotation = objectToSnap.eulerAngles.y;

                Quaternion groundAlignment = Quaternion.FromToRotation(Vector3.up, hit.normal);

                objectToSnap.rotation = groundAlignment * Quaternion.Euler(0, currentYRotation, 0);

                objectToSnap.position = hit.point;
            }
        }


        private bool IsSlopeSteep(RaycastHit hit, float maxSlopeAngle)
        {
            float angle = Vector3.Angle(hit.normal, Vector3.up);

            return angle > maxSlopeAngle;
        }


        private bool HasStructuresInRadius(Vector3 position, float radius)
        {
            Collider[] colliders = Physics.OverlapSphere(position, radius, structureSpawnerConfig.structuresLayer);

            return colliders.Length > 0;
        }
    }
}
