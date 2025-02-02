using StaticData.Data;
using StaticData.Services;
using TerrainGenerator;
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


        public TerrainChunk CreateChunk(Vector3 position, float[,] heightMap, MapGenerationConfig mapGenerationConfig, Transform parent)
        {

            MeshData meshData = meshGenerator.CreateMeshData(heightMap, mapGenerationConfig.noiseMultiplier,
                mapGenerationConfig.heightCurve, mapGenerationConfig.lod, mapGenerationConfig.heightGradient,
                mapGenerationConfig.terrainRegions);

            TerrainChunk terrainChunk = new TerrainChunk(mapGenerationConfig.chunkMaterial, position, meshData, parent);

            return terrainChunk;
        }

 
        public TerrainChunk CreateChunk(Vector3 position, float[] heightMap, MapGenerationConfig mapGenerationConfig, Transform parent)
        {

            MeshData meshData = meshGenerator.CreateMeshData(heightMap, mapGenerationConfig.noiseMultiplier,
                mapGenerationConfig.heightCurve, mapGenerationConfig.lod, mapGenerationConfig.heightGradient,
                mapGenerationConfig.terrainRegions);

            TerrainChunk terrainChunk = new TerrainChunk(mapGenerationConfig.chunkMaterial, position, meshData, parent);

            return terrainChunk;
        }
    }
}
