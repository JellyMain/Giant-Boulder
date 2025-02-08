using System.Collections.Generic;
using StaticData.Data;
using StaticData.Services;
using StructuresSpawner.SpawnPointsValidator.Jobs;
using TerrainGenerator;
using TerrainGenerator.Data;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;


namespace StructuresSpawner.SpawnPointsValidator
{
    public class SpawnPointsValidator
    {
        private readonly StaticDataService staticDataService;
        private readonly MapCreator mapCreator;
        private readonly int vertexCheckStep = 2;
        private QuadTree spawnPointsQuadTree;
        private MapGenerationConfig mapGenerationConfig;


        public SpawnPointsValidator(StaticDataService staticDataService, MapCreator mapCreator)
        {
            this.staticDataService = staticDataService;
            this.mapCreator = mapCreator;
        }


        public void Init()
        {
            mapGenerationConfig = staticDataService.MapConfigForSeason(TerrainSeason.Summer);

            float mapWidth = (mapGenerationConfig.chunkSize - 1) * mapGenerationConfig.mapSize;
            float mapHeight = (mapGenerationConfig.chunkSize - 1) * mapGenerationConfig.mapSize;
            float chunkWidth = mapGenerationConfig.chunkSize - 1;
            float chunkHeight = mapGenerationConfig.chunkSize - 1;

            float startX = 0 - chunkWidth / 2;
            float startZ = 0 - chunkHeight / 2;

            Rect bounds = new Rect(startX, startZ, mapWidth, mapHeight);
            spawnPointsQuadTree = new QuadTree(bounds, mapGenerationConfig.mapSize);
        }


        public void ComputeAllMeshesParallel()
        {
            int totalChunks = mapGenerationConfig.mapSize * mapGenerationConfig.mapSize;
            int chunkResolution = mapGenerationConfig.chunkSize * mapGenerationConfig.chunkSize;
            int totalVertices = totalChunks * chunkResolution;

            NativeArray<float3> allVertices = new NativeArray<float3>(totalVertices, Allocator.TempJob);
            NativeArray<float3> allNormals = new NativeArray<float3>(totalVertices, Allocator.TempJob);
            NativeArray<int> validSpawnPointsFlags = new NativeArray<int>(totalVertices, Allocator.TempJob);
            NativeList<float2> validSpawnPoints = new NativeList<float2>(Allocator.TempJob);

            for (int i = 0; i < mapCreator.TerrainChunks.Length; i++)
            {
                int startIndex = chunkResolution * i;
                MeshData chunkMeshData = mapCreator.TerrainChunks[i].meshData;

                for (int j = 0; j < chunkMeshData.Vertices.Length; j++)
                {
                    allVertices[startIndex + j] = chunkMeshData.Vertices[j];
                    allNormals[startIndex + j] = chunkMeshData.Normals[j];
                }
            }

            AddValidSpawnPointsJob slopeJob = new AddValidSpawnPointsJob
            {
                vertices = allVertices,
                normals = allNormals,
                validSpawnPointsFlags = validSpawnPointsFlags,
                verticesPerLine = mapGenerationConfig.chunkSize,
                vertexCheckStep = vertexCheckStep
            };
            JobHandle slopeJobHandle = slopeJob.Schedule(totalVertices, 64);

            CollectValidSpawnPointsJob collectJob = new CollectValidSpawnPointsJob
            {
                validSpawnPointsFlags = validSpawnPointsFlags,
                vertices = allVertices,
                validSpawnPoints = validSpawnPoints
            };
            JobHandle collectJobHandle = collectJob.Schedule(slopeJobHandle);

            collectJobHandle.Complete();

            InsertPointsInQuadTree(validSpawnPoints, mapCreator.TerrainChunks);

            allVertices.Dispose();
            allNormals.Dispose();
            validSpawnPointsFlags.Dispose();
            validSpawnPoints.Dispose();
        }


        private void InsertPointsInQuadTree(NativeList<float2> validSpawnPoints, TerrainChunk[] terrainChunks)
        {
            int validPointsPerChunk =
                validSpawnPoints.Length / (mapGenerationConfig.mapSize * mapGenerationConfig.mapSize);

            for (int i = 0; i < terrainChunks.Length; i++)
            {
                int pointsStartPosition = i * validPointsPerChunk;
                Vector3 terrainChunkPosition = terrainChunks[i].position;

                for (int j = 0; j < validPointsPerChunk; j++)
                {
                    float2 spawnPoint = validSpawnPoints[pointsStartPosition + j];
                    Vector2 globalPosition = new Vector2(spawnPoint.x + terrainChunkPosition.x,
                        spawnPoint.y + terrainChunkPosition.z);

                    spawnPointsQuadTree.Insert(globalPosition);
                }
            }

            QuadTreeDrawer.quadTree = spawnPointsQuadTree;
        }
    }
}
