using Sirenix.OdinInspector;
using StaticData.Data;
using StaticData.Services;
using UnityEngine;
using Zenject;


namespace TerrainGenerator
{
    public class ChunkGenerator
    {
        private readonly MapGenerationConfig mapGenerationConfig;
        private StaticDataService staticDataService;
        private readonly NoiseGenerator noiseGenerator;
        private readonly TextureGenerator textureGenerator;
        private readonly MeshGenerator meshGenerator;


        public ChunkGenerator(StaticDataService staticDataService, NoiseGenerator noiseGenerator,
            TextureGenerator textureGenerator, MeshGenerator meshGenerator)
        {
            this.staticDataService = staticDataService;
            this.noiseGenerator = noiseGenerator;
            this.textureGenerator = textureGenerator;
            this.meshGenerator = meshGenerator;
            mapGenerationConfig = staticDataService.MapGenerationConfig;
        }


        public TerrainChunk CreateChunk(Vector3 position, float[,] heightMap)
        {
            TerrainChunk terrainChunk = new TerrainChunk(noiseGenerator, textureGenerator, meshGenerator,
                mapGenerationConfig.chunkSize,
                mapGenerationConfig.noiseMultiplier, mapGenerationConfig.heightCurve,
                mapGenerationConfig.lod, mapGenerationConfig.chunkMaterial,
                mapGenerationConfig.heightGradient, position, heightMap, mapGenerationConfig.terrainRegions);

            return terrainChunk;
        }
    }
}
