using System.Collections.Generic;
using Assets;
using Const;
using Cysharp.Threading.Tasks;
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
        private readonly AssetProvider assetProvider;
        private MapGenerationConfig mapGenerationConfig;
        private Dictionary<ChunkBiome, List<TerrainChunk>> availableChunks;


        public StructureSpawner(StaticDataService staticDataService, MapCreator mapCreator, DiContainer diContainer,
            AssetProvider assetProvider)
        {
            this.staticDataService = staticDataService;
            this.mapCreator = mapCreator;
            this.diContainer = diContainer;
            this.assetProvider = assetProvider;
        }


        private void Init()
        {
            mapGenerationConfig = staticDataService.MapConfigForSeason(TerrainSeason.Summer);
            availableChunks = mapCreator.SortedChunks;
        }


        public void ActivateSpawner()
        {
            Init();


            foreach (KeyValuePair<ChunkBiome, List<StructuresPercentagePair>> biomeStructuresPair in staticDataService
                         .SpawnerConfig.allSpawners)
            {
                SpawnStructures(biomeStructuresPair);
            }
        }


        private void SpawnStructures(KeyValuePair<ChunkBiome, List<StructuresPercentagePair>> biomeStructuresPair)
        {
            ChunkBiome chunkBiome = biomeStructuresPair.Key;
            List<StructuresPercentagePair> structuresPercentagePairs = biomeStructuresPair.Value;
            List<TerrainChunk> chunks = availableChunks[chunkBiome];

            foreach (StructuresPercentagePair structurePercentagePair in structuresPercentagePairs)
            {
                if (availableChunks[chunkBiome].Count == 0)
                {
                    Debug.LogError($"Doesn't have any available chunks of type {chunkBiome}");
                    break;
                }

                int structuresCount = chunks.Count * structurePercentagePair.spawnRate / 100;

                for (int i = 0; i < structuresCount; i++)
                {
                    TerrainChunk randomChunk = chunks[Random.Range(0, chunks.Count)];

                    Vector3 rayStart = randomChunk.position +
                                       Vector3.up * staticDataService.SpawnerConfig.raycastHeight;

                    if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, Mathf.Infinity,
                            staticDataService.SpawnerConfig.groundLayer))
                    {
                        randomChunk.structures.Add(hit, structurePercentagePair.structurePrefab);
                    }

                    availableChunks[chunkBiome].Remove(randomChunk);
                }
            }
        }


        public void SpawnStructuresInChunk(TerrainChunk terrainChunk)
        {
            foreach (KeyValuePair<RaycastHit, StructureRoot> keyValuePair in terrainChunk.structures)
            {
                Quaternion rotation = Quaternion.FromToRotation(Vector3.up, keyValuePair.Key.normal);
                GameObject spawnedObject = diContainer.InstantiatePrefab(keyValuePair.Value, keyValuePair.Key.point,
                    rotation,
                    terrainChunk.chunkGameObject.transform);
                StructureRoot spawnedStructureRoot = spawnedObject.GetComponent<StructureRoot>();

                ApplyStructureSettings(spawnedStructureRoot.structureChildSettings);
                spawnedStructureRoot.BatchObjects();
            }

            terrainChunk.structuresInstantiated = true;
        }


        private void ApplyStructureSettings(List<StructureObject> structureSpawnSettings)
        {
            foreach (StructureObject childObject in structureSpawnSettings)
            {
                if (childObject.snapToGround)
                {
                    SnapToGround(childObject.transform);
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


        private void SnapToGround(Transform objectToSnap)
        {
            Vector3 rayStart = objectToSnap.position + Vector3.up * staticDataService.SpawnerConfig.raycastHeight;

            if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, Mathf.Infinity,
                    staticDataService.SpawnerConfig.groundLayer))
            {
                float currentYRotation = objectToSnap.eulerAngles.y;

                Quaternion groundAlignment = Quaternion.FromToRotation(Vector3.up, hit.normal);

                objectToSnap.rotation = groundAlignment * Quaternion.Euler(0, currentYRotation, 0);

                objectToSnap.position = hit.point;
            }
        }


        private bool IsSlopeSteep(Vector3 position, float radius, int raysCount, float maxSlopeAngle)
        {
            float totalAngle = 0;

            for (int i = 0; i < raysCount; i++)
            {
                Vector2 randomOffset = Random.insideUnitCircle * radius;
                Vector3 randomPositionInRadius =
                    new Vector3(randomOffset.x + position.x, position.y, randomOffset.y + position.z);

                if (Physics.Raycast(randomPositionInRadius, Vector3.down, out RaycastHit hit, Mathf.Infinity,
                        staticDataService.SpawnerConfig.groundLayer))
                {
                    float angle = Vector3.Angle(hit.normal, Vector3.up);
                    totalAngle += angle;
                }
            }

            totalAngle /= raysCount;

            return totalAngle > maxSlopeAngle;
        }


        private bool HasStructuresInRadius(Vector3 position, float radius)
        {
            int collidersNumber = Physics.OverlapSphereNonAlloc(position, radius, collidersAllocation,
                staticDataService.SpawnerConfig.structuresLayer);

            return collidersNumber > 0;
        }


        public async UniTaskVoid SpawnWalls()
        {
            GameObject wallPrefab = await assetProvider.LoadAsset<GameObject>(RuntimeConstants.PrefabAddresses.MOUNTAINS);

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
