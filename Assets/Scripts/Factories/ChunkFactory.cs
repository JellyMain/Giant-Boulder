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
        private readonly StaticDataService staticDataService;
        private readonly NoiseGenerator noiseGenerator;
        private readonly MeshGenerator meshGenerator;
        private MapGenerationConfig mapGenerationConfig;


        public ChunkFactory(DiContainer diContainer, StaticDataService staticDataService, NoiseGenerator noiseGenerator,
            MeshGenerator meshGenerator)
        {
            this.diContainer = diContainer;
            this.staticDataService = staticDataService;
            this.noiseGenerator = noiseGenerator;
            this.meshGenerator = meshGenerator;
        }


        public TerrainChunk CreateChunk(Vector3 position, float[,] heightMap)
        {
            mapGenerationConfig = staticDataService.MapGenerationConfig;

            MeshData meshData = meshGenerator.CreateMeshData(heightMap, mapGenerationConfig.noiseMultiplier,
                mapGenerationConfig.heightCurve, mapGenerationConfig.lod, mapGenerationConfig.heightGradient);

            TerrainChunk terrainChunk = new TerrainChunk(mapGenerationConfig.chunkMaterial, position, meshData);

            return terrainChunk;
        }
        
        
        public TerrainChunk CreateChunk(Vector3 position, float[] heightMap)
        {
            mapGenerationConfig = staticDataService.MapGenerationConfig;

            MeshData meshData = meshGenerator.CreateMeshData(heightMap, mapGenerationConfig.noiseMultiplier,
                mapGenerationConfig.heightCurve, mapGenerationConfig.lod, mapGenerationConfig.heightGradient);

            TerrainChunk terrainChunk = new TerrainChunk(mapGenerationConfig.chunkMaterial, position, meshData);

            return terrainChunk;
        }
    }
}
