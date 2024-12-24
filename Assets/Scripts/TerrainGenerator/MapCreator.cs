using System;
using System.Collections.Generic;
using Factories;
using Sirenix.OdinInspector;
using StaticData.Data;
using StaticData.Services;
using Unity.Collections;
using UnityEngine;
using Zenject;


namespace TerrainGenerator
{
    public class MapCreator
    {
        private readonly NoiseGenerator noiseGenerator;
        private readonly StaticDataService staticDataService;
        private readonly ChunkFactory chunkFactory;
        private MapGenerationConfig mapGenerationConfig;
        private int chunkSize;



        public MapCreator(NoiseGenerator noiseGenerator, StaticDataService staticDataService, ChunkFactory chunkFactory)
        {
            this.noiseGenerator = noiseGenerator;
            this.staticDataService = staticDataService;
            this.chunkFactory = chunkFactory;
        }


        public void CreateMap()
        {
            mapGenerationConfig = staticDataService.MapGenerationConfig;
            chunkSize = mapGenerationConfig.chunkSize - 1;

            Vector3[] allChunkPositions = new Vector3[mapGenerationConfig.mapSize * mapGenerationConfig.mapSize];

            for (int y = 0; y < mapGenerationConfig.mapSize; y++)
            {
                for (int x = 0; x < mapGenerationConfig.mapSize; x++)
                {
                    Vector3 chunkPosition = new Vector3(x * chunkSize, 0, y * chunkSize);

                    allChunkPositions[y * mapGenerationConfig.mapSize + x] = chunkPosition;
                }
            }

            // float[][,] allTerrainHeightMaps = noiseGenerator.GenerateAllTerrainHeightMaps(mapGenerationConfig.mapSize,
            //     mapGenerationConfig.chunkSize,
            //     mapGenerationConfig.noiseScale, mapGenerationConfig.persistance, mapGenerationConfig.lacunarity,
            //     mapGenerationConfig.octaves, mapGenerationConfig.seed, mapGenerationConfig.offset, allChunkPositions);

            float[][] allTerrainHeightMapsParallel = noiseGenerator.GenerateAllHeightMapsParallel(
                mapGenerationConfig.mapSize,
                mapGenerationConfig.chunkSize,
                mapGenerationConfig.noiseScale, mapGenerationConfig.persistance, mapGenerationConfig.lacunarity,
                mapGenerationConfig.octaves, mapGenerationConfig.seed, mapGenerationConfig.offset, allChunkPositions);

            

            for (int y = 0; y < mapGenerationConfig.mapSize; y++)
            {
                for (int x = 0; x < mapGenerationConfig.mapSize; x++)
                {
                    Vector3 position = allChunkPositions[y * mapGenerationConfig.mapSize + x];
                    float[] heightMap = allTerrainHeightMapsParallel[y * mapGenerationConfig.mapSize + x];
                    chunkFactory.CreateChunk(position, heightMap);
                }
            }
        }
    }
}
