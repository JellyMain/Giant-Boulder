using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using StaticData.Data;
using StaticData.Services;
using UnityEngine;
using Zenject;


namespace TerrainGenerator
{
    public class MapCreator : MonoBehaviour
    {
        private ChunkGenerator chunkGenerator;
        private NoiseGenerator noiseGenerator;
        private MapGenerationConfig mapGenerationConfig;
        private int chunkSize;


        [Inject]
        private void Construct(ChunkGenerator chunkGenerator, NoiseGenerator noiseGenerator,
            StaticDataService staticDataService)
        {
            this.chunkGenerator = chunkGenerator;
            this.noiseGenerator = noiseGenerator;

            mapGenerationConfig = staticDataService.MapGenerationConfig;
            chunkSize = mapGenerationConfig.chunkSize - 1;
        }


        private void Start()
        {
            CreateMap();
        }

        
        private void CreateMap()
        {
            Vector3[] allChunkPositions = new Vector3[mapGenerationConfig.mapSize * mapGenerationConfig.mapSize];

            for (int y = 0; y < mapGenerationConfig.mapSize; y++)
            {
                for (int x = 0; x < mapGenerationConfig.mapSize; x++)
                {
                    Vector3 chunkPosition = new Vector3(x * chunkSize, 0, y * chunkSize);

                    allChunkPositions[y * mapGenerationConfig.mapSize + x] = chunkPosition;
                }
            }

            float[][,] allTerrainHeightMaps = noiseGenerator.GenerateAllTerrainHeightMaps(mapGenerationConfig.mapSize,
                mapGenerationConfig.chunkSize,
                mapGenerationConfig.noiseScale, mapGenerationConfig.persistance, mapGenerationConfig.lacunarity,
                mapGenerationConfig.octaves, mapGenerationConfig.seed, mapGenerationConfig.offset, allChunkPositions);


            for (int y = 0; y < mapGenerationConfig.mapSize; y++)
            {
                for (int x = 0; x < mapGenerationConfig.mapSize; x++)
                {
                    Vector3 position = allChunkPositions[y * mapGenerationConfig.mapSize + x];
                    float[,] heightMap = allTerrainHeightMaps[y * mapGenerationConfig.mapSize + x];
                    chunkGenerator.CreateChunk(position, heightMap);
                }
            }
        }
    }
}