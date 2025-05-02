using System.Collections.Generic;
using GameLoop;
using TerrainGenerator;
using TerrainGenerator.Enums;
using UnityEngine;
using Zenject;
using Random = UnityEngine.Random;


namespace StructuresSpawner
{
    public class GrassSpawner : MonoBehaviour
    {
        [SerializeField] private Mesh grassMesh;
        [SerializeField] private Material grassMaterial;
        [SerializeField] private int grassAmountPerChunk = 2000;
        [SerializeField] private LayerMask groundLayer;
        private MapCreator mapCreator;
        private LevelCreationWatcher levelCreationWatcher;
        private List<TerrainChunk> grassChunks;
        public Dictionary<TerrainChunk, List<Matrix4x4>> ChunkGrassMatrices { get; private set; }
        private readonly List<Matrix4x4> visibleChunksGrassMatrices = new List<Matrix4x4>();



        [Inject]
        private void Construct(MapCreator mapCreator, LevelCreationWatcher levelCreationWatcher)
        {
            this.mapCreator = mapCreator;
            this.levelCreationWatcher = levelCreationWatcher;
        }


        private void OnEnable()
        {
            levelCreationWatcher.OnLevelCreated += Init;
        }


        private void OnDisable()
        {
            levelCreationWatcher.OnLevelCreated -= Init;
        }


        private void Init()
        {
            GetGrassChunks();
            Dictionary<TerrainChunk, List<(Vector3 Position, Quaternion rotation)>> chunkGrassPositionsAndRotations =
                GetGrassPositions();
            CreateMatrices(chunkGrassPositionsAndRotations);
        }


        private void GetGrassChunks()
        {
            grassChunks = mapCreator.SortedChunks[ChunkBiome.Plain];
            grassChunks.AddRange(mapCreator.SortedChunks[ChunkBiome.Hill]);
        }


        private void CreateMatrices(
            Dictionary<TerrainChunk, List<(Vector3 position, Quaternion rotation)>> chunkGrassPositions)
        {
            ChunkGrassMatrices = new Dictionary<TerrainChunk, List<Matrix4x4>>();

            foreach (TerrainChunk chunk in grassChunks)
            {
                ChunkGrassMatrices[chunk] = new List<Matrix4x4>(grassAmountPerChunk);

                for (int i = 0; i < grassAmountPerChunk; i++)
                {
                    Vector3 grassPosition = chunkGrassPositions[chunk][i].position;
                    Quaternion grassRotation = chunkGrassPositions[chunk][i].rotation;
                    Matrix4x4 grassMatrix = Matrix4x4.TRS(grassPosition, grassRotation, Vector3.one);

                    ChunkGrassMatrices[chunk].Add(grassMatrix);
                }
            }
        }


        private (Vector3, Quaternion) GetSnappedPosition(Vector3 initialPosition, Quaternion initialRotation)
        {
            Vector3 rayStart = initialPosition + Vector3.up * 1000;

            (Vector3 position, Quaternion rotation) snappedResult = (Vector3.zero, Quaternion.identity);

            if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, Mathf.Infinity, groundLayer))
            {
                float currentYRotation = initialRotation.eulerAngles.y;

                Quaternion groundAlignment = Quaternion.FromToRotation(Vector3.up, hit.normal);

                Quaternion snappedRotation = groundAlignment * Quaternion.Euler(0, currentYRotation, 0);

                Vector3 snappedPosition = hit.point;

                snappedResult = (snappedPosition, snappedRotation);
            }

            return snappedResult;
        }


        private Dictionary<TerrainChunk, List<(Vector3 position, Quaternion rotation)>> GetGrassPositions()
        {
            Dictionary<TerrainChunk, List<(Vector3 position, Quaternion rotation)>> chunkGrassPositions =
                new Dictionary<TerrainChunk, List<(Vector3 position, Quaternion rotation)>>();

            foreach (TerrainChunk chunk in grassChunks)
            {
                chunkGrassPositions[chunk] = new List<(Vector3 position, Quaternion rotation)>(grassAmountPerChunk);

                for (int i = 0; i < grassAmountPerChunk; i++)
                {
                    float chunkSize = chunk.bounds.size.x / 2;

                    float randomXPosition =
                        Random.Range(chunk.position.x - chunkSize, chunk.position.x + chunkSize);
                    float randomZPosition =
                        Random.Range(chunk.position.z - chunkSize, chunk.position.z + chunkSize);

                    float randomYRotation = Random.Range(0, 360);
                    Vector3 randomPosition = new Vector3(randomXPosition, 0, randomZPosition);
                    Quaternion randomRotation = Quaternion.Euler(0, randomYRotation, 0);

                    (Vector3 position, Quaternion rotation) snappedResult =
                        GetSnappedPosition(randomPosition, randomRotation);

                    chunkGrassPositions[chunk].Add(snappedResult);
                }
            }

            return chunkGrassPositions;
        }


        public void RenderGrassChunk(TerrainChunk terrainChunk)
        {
            if (ChunkGrassMatrices.TryGetValue(terrainChunk, out List<Matrix4x4> chunkGrassMatrices))
            {
                Graphics.DrawMeshInstanced(grassMesh, 0, grassMaterial, chunkGrassMatrices);
            }
        }


        public void RenderAllVisibleGrassChunks(List<TerrainChunk> visibleChunks)
        {
            visibleChunksGrassMatrices.Clear();

            foreach (var chunk in visibleChunks)
            {
                visibleChunksGrassMatrices.AddRange(ChunkGrassMatrices[chunk]);
            }

            Graphics.DrawMeshInstanced(grassMesh, 0, grassMaterial, visibleChunksGrassMatrices);
        }
    }
}
