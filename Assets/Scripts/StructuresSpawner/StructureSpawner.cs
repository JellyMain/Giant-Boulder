using System.Collections.Generic;
using StaticData.Data;
using StaticData.Services;
using Structures;
using TerrainGenerator;
using TerrainGenerator.Enums;
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
        private readonly MapCreator mapCreator;
        private readonly DiContainer diContainer;
        private int spawnerCounter;
        private MapGenerationConfig mapGenerationConfig;
        private Dictionary<ChunkLandscapeType, List<TerrainChunk>> availableChunks;


        public StructureSpawner(StaticDataService staticDataService, MapCreator mapCreator, DiContainer diContainer)
        {
            this.staticDataService = staticDataService;
            this.mapCreator = mapCreator;
            this.diContainer = diContainer;
        }


        private void Init()
        {
            spawnerCounter = 0;

            mapGenerationConfig = staticDataService.MapConfigForSeason(TerrainSeason.Summer);
            availableChunks = mapCreator.SortedChunks;
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
            ChunkLandscapeType chunkLandscapeType = spawnerConfig.chunkLandscapeType;

            List<TerrainChunk> chunks = availableChunks[chunkLandscapeType];


            foreach (StructureRoot structurePrefab in spawnerConfig.structurePrefabCountPair.Keys)
            {
                for (int i = 0; i < spawnerConfig.structurePrefabCountPair[structurePrefab]; i++)
                {
                    if (availableChunks[chunkLandscapeType].Count == 0)
                    {
                        Debug.LogError($"Doesn't have any available chunks of type {chunkLandscapeType}");
                        break;
                    }
                    
                    Debug.Log(chunks.Count);
                    
                    TerrainChunk randomChunk = chunks[Random.Range(0, chunks.Count)];

                    Vector3 rayStart = randomChunk.position + Vector3.up * spawnerConfig.raycastHeight;

                    if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, Mathf.Infinity,
                            spawnerConfig.groundLayer))
                    {
                        randomChunk.structures.Add(hit, structurePrefab);
                        randomChunk.spawnerConfig = spawnerConfig;
                    }
                    
                    availableChunks[chunkLandscapeType].Remove(randomChunk);
                }

                if (availableChunks[chunkLandscapeType].Count == 0)
                {
                    break;
                }
            }

            spawnerCounter++;
        }


        public void SpawnStructuresInChunk(TerrainChunk terrainChunk)
        {
            foreach (KeyValuePair<RaycastHit, StructureRoot> keyValuePair in terrainChunk.structures)
            {
                Quaternion rotation = Quaternion.FromToRotation(Vector3.up, keyValuePair.Key.normal);
                GameObject spawnedObject =  diContainer.InstantiatePrefab(keyValuePair.Value, keyValuePair.Key.point, rotation,
                    terrainChunk.chunkGameObject.transform);
                StructureRoot spawnedStructureRoot = spawnedObject.GetComponent<StructureRoot>();
                
                ApplyStructureSettings(spawnedStructureRoot.structureChildSettings, terrainChunk.spawnerConfig);
                spawnedStructureRoot.BatchObjects();
            }
            
            terrainChunk.structuresInstantiated = true;
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


        public void SpawnWalls()
        {
            GameObject wallPrefab = Resources.Load<GameObject>("RuntimePrefabs/BorderMountains/Mountains");

            Vector3 leftWallPosition = new Vector3(-175, 0, 30);
            Vector3 backWallPosition = new Vector3(0, 0, -175);
            Vector3 frontWallPosition = new Vector3(30, 0, 3075);
            Vector3 rightWallPosition = new Vector3(3075, 0, 30);


            Object.Instantiate(wallPrefab, leftWallPosition, Quaternion.identity);
            Object.Instantiate(wallPrefab, rightWallPosition, Quaternion.identity);
            Object.Instantiate(wallPrefab, backWallPosition, Quaternion.Euler(0, 90, 0));
            Object.Instantiate(wallPrefab, frontWallPosition, Quaternion.Euler(0, 90, 0));
        }
    }
}
