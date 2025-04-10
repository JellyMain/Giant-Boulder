using System.Collections.Generic;
using StaticData.Data;
using StaticData.Services;
using TerrainGenerator;
using TerrainGenerator.Data;
using TerrainGenerator.Enums;
using Unity.Collections;
using UnityEngine;
using Zenject;


namespace Factories
{
    public class ChunkFactory
    {
        private readonly DiContainer diContainer;
        private readonly NoiseGenerator noiseGenerator;
        private readonly MeshGenerator meshGenerator;


        public ChunkFactory(DiContainer diContainer, StaticDataService staticDataService, NoiseGenerator noiseGenerator,
            MeshGenerator meshGenerator)
        {
            this.diContainer = diContainer;
            this.noiseGenerator = noiseGenerator;
            this.meshGenerator = meshGenerator;
        }


        public TerrainChunk CreateChunk(Vector3 position, float[,] heightMap, ChunkBiome chunkBiome,
            MapGenerationConfig mapGenerationConfig, Transform parent, int chunkSize)
        {
            MeshData meshData = meshGenerator.CreateMeshData(heightMap, mapGenerationConfig.noiseMultiplier,
                mapGenerationConfig.heightCurve, mapGenerationConfig.lod, mapGenerationConfig.heightGradient);

            TerrainChunk terrainChunk = new TerrainChunk(mapGenerationConfig.chunkMaterial, position, meshData,
                chunkBiome, parent, chunkSize);

            return terrainChunk;
        }


        public TerrainChunk CreateChunk(Vector3 position, float[] heightMap, ChunkBiome chunkBiome,
            MapGenerationConfig mapGenerationConfig,
            Transform parent, int chunkSize)
        {
            MeshData meshData = meshGenerator.CreateMeshData(heightMap, mapGenerationConfig.noiseMultiplier,
                mapGenerationConfig.heightCurve, mapGenerationConfig.lod, mapGenerationConfig.heightGradient);

            TerrainChunk terrainChunk = new TerrainChunk(mapGenerationConfig.chunkMaterial, position, meshData,
                chunkBiome, parent, chunkSize);

            return terrainChunk;
        }


        public Dictionary<Vector2, TerrainChunk> CreateAllChunks(Vector2[] chunksCoords, float[][] heightMaps,
            ChunkBiome[] chunkLandscapeTypes,
            MapGenerationConfig mapGenerationConfig, Transform parent, int chunkSize)
        {
            MeshData[] chunksMeshData = meshGenerator.CreateAllMeshDataParallel(heightMaps,
                mapGenerationConfig.noiseMultiplier, mapGenerationConfig.heightCurve, mapGenerationConfig.lod,
                mapGenerationConfig.heightGradient);

            Dictionary<Vector2, TerrainChunk> terrainChunks = new Dictionary<Vector2, TerrainChunk>();

            for (int i = 0; i < chunksCoords.Length; i++)
            {
                TerrainChunk terrainChunk = new TerrainChunk(mapGenerationConfig.chunkMaterial, chunksCoords[i],
                    chunksMeshData[i], chunkLandscapeTypes[i], parent, chunkSize);
                terrainChunks.Add(chunksCoords[i], terrainChunk);
            }

            return terrainChunks;
        }
    }
}
