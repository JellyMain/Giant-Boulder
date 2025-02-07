using System.Collections.Generic;
using System.Diagnostics;
using StaticData.Data;
using StaticData.Services;
using TerrainGenerator;
using TerrainGenerator.Data;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Debug = UnityEngine.Debug;


namespace StructuresSpawner
{
    public class SpawnPointsValidator
    {
        private readonly StaticDataService staticDataService;
        private readonly MapCreator mapCreator;
        private QuadTree spawnPointsQuadTree;
        private int vertexCheckStep = 5;
        private MapGenerationConfig mapGenerationConfig;


        public SpawnPointsValidator(StaticDataService staticDataService, MapCreator mapCreator)
        {
            this.staticDataService = staticDataService;
            this.mapCreator = mapCreator;
        }


        public void Init()
        {
            mapGenerationConfig = staticDataService.MapConfigForSeason(TerrainSeason.Summer);

            float width = mapGenerationConfig.chunkSize - 1;
            float height = mapGenerationConfig.chunkSize - 1;

            float startX = 0 - width / 2;
            float startZ = 0 - height / 2;

            Rect bounds = new Rect(startX, startZ, width, height);
            spawnPointsQuadTree = new QuadTree(bounds);
        }


        public void ComputeAllMeshesParallel()
        {
            NativeArray<NativeList<float2>> chunkValidSpawnPoints =
                new NativeArray<NativeList<float2>>(mapCreator.ChunksMeshData.Count, Allocator.TempJob);

            NativeArray<JobHandle> jobHandles =
                new NativeArray<JobHandle>(mapCreator.ChunksMeshData.Count, Allocator.Temp);

            NativeArray<NativeArray<float3>> normalsArrays =
                new NativeArray<NativeArray<float3>>(mapCreator.ChunksMeshData.Count, Allocator.Temp);
            NativeArray<NativeArray<float3>> verticesArrays =
                new NativeArray<NativeArray<float3>>(mapCreator.ChunksMeshData.Count, Allocator.Temp);

            for (int i = 0; i < mapCreator.ChunksMeshData.Count; i++)
            {
                MeshData chunkMeshData = mapCreator.ChunksMeshData[i];
                NativeArray<float3> normals = new NativeArray<float3>(chunkMeshData.Normals.Length, Allocator.TempJob);
                NativeArray<float3> vertices =
                    new NativeArray<float3>(chunkMeshData.Vertices.Length, Allocator.TempJob);

                for (int j = 0; j < chunkMeshData.Vertices.Length; j++)
                {
                    normals[j] = chunkMeshData.Normals[j];
                    vertices[j] = chunkMeshData.Vertices[j];
                }

                normalsArrays[i] = normals;
                verticesArrays[i] = vertices;

                chunkValidSpawnPoints[i] = new NativeList<float2>(Allocator.TempJob);

                AddValidSpawnPointsJob addValidSpawnPointsJob = new AddValidSpawnPointsJob()
                {
                    vertices = vertices,
                    normals = normals,
                    validSpawnPoints = chunkValidSpawnPoints[i],
                    vertexCheckStep = vertexCheckStep,
                    verticesPerLine = chunkMeshData.verticesPerLine
                };

                jobHandles[i] = addValidSpawnPointsJob.Schedule();
            }

            JobHandle.CompleteAll(jobHandles);

            NativeList<float2> validSpawnPoints = new NativeList<float2>(Allocator.Temp);

            for (int i = 0; i < chunkValidSpawnPoints.Length; i++)
            {
                validSpawnPoints.AddRange(chunkValidSpawnPoints[i]);
                chunkValidSpawnPoints[i].Dispose(); 
            }

            InsertPointsInQuadTree(validSpawnPoints);

            DisposeNativeCollections(normalsArrays, verticesArrays, jobHandles, chunkValidSpawnPoints, validSpawnPoints);
        }


        private void InsertPointsInQuadTree(NativeList<float2> validSpawnPoints)
        {
            foreach (float2 spawnPoint in validSpawnPoints)
            {
                spawnPointsQuadTree.Insert(new Vector2(spawnPoint.x, spawnPoint.y));
            }
        }


        private static void DisposeNativeCollections(NativeArray<NativeArray<float3>> normalsArrays, NativeArray<NativeArray<float3>> verticesArrays,
            NativeArray<JobHandle> jobHandles, NativeArray<NativeList<float2>> chunkValidSpawnPoints, NativeList<float2> validSpawnPoints)
        {
            for (int i = 0; i < normalsArrays.Length; i++)
            {
                normalsArrays[i].Dispose();
                verticesArrays[i].Dispose();
            }

            jobHandles.Dispose();
            chunkValidSpawnPoints.Dispose();
            normalsArrays.Dispose();
            verticesArrays.Dispose();
            validSpawnPoints.Dispose();
        }
    }
}
