using System;
using System.Collections.Generic;
using System.Diagnostics;
using Factories;
using Sirenix.OdinInspector;
using StaticData.Data;
using StaticData.Services;
using StructuresSpawner;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using Utils;
using Zenject;
using Debug = UnityEngine.Debug;


namespace TerrainGenerator
{
    public class MapCreator
    {
        private readonly NoiseGenerator noiseGenerator;
        private readonly StaticDataService staticDataService;
        private readonly ChunkFactory chunkFactory;
        private MapGenerationConfig mapGenerationConfig;
        private int chunkSize;
        public List<MeshData> ChunksMeshData { get; private set; } = new List<MeshData>();


        public MapCreator(NoiseGenerator noiseGenerator, StaticDataService staticDataService, ChunkFactory chunkFactory)
        {
            this.noiseGenerator = noiseGenerator;
            this.staticDataService = staticDataService;
            this.chunkFactory = chunkFactory;
        }


        public void CreateMap()
        {
            GameObject chunksParent = new GameObject("TerrainChunks");

            TerrainSeason randomMapSeason = DataUtility.GetRandomEnumValue<TerrainSeason>(true);

            mapGenerationConfig = staticDataService.MapConfigForSeason(TerrainSeason.Summer);

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


            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            
            float[][] allTerrainHeightMaps = noiseGenerator.GenerateAllHeightMapsParallel(
                mapGenerationConfig.mapSize,
                mapGenerationConfig.chunkSize,
                mapGenerationConfig.noiseScale, mapGenerationConfig.persistance, mapGenerationConfig.lacunarity,
                mapGenerationConfig.octaves, mapGenerationConfig.seed, mapGenerationConfig.offset, allChunkPositions);

            
            stopwatch.Stop();
            Debug.Log(stopwatch.ElapsedMilliseconds);
            
            chunkFactory.CreateAllChunks(allChunkPositions, allTerrainHeightMaps, mapGenerationConfig,
                chunksParent.transform);

            // CreateChunks(allChunkPositions, allTerrainHeightMaps, chunksParent);
        }


        private void CreateChunks(Vector3[] allChunkPositions, float[][] allTerrainHeightMaps, GameObject chunksParent)
        {
            for (int y = 0; y < mapGenerationConfig.mapSize; y++)
            {
                for (int x = 0; x < mapGenerationConfig.mapSize; x++)
                {
                    Vector3 position = allChunkPositions[y * mapGenerationConfig.mapSize + x];
                    float[] heightMap = allTerrainHeightMaps[y * mapGenerationConfig.mapSize + x];
                    TerrainChunk terrainChunk = chunkFactory.CreateChunk(position, heightMap, mapGenerationConfig,
                        chunksParent.transform);
                    
                    ChunksMeshData.Add(terrainChunk.meshData);
                }
            }
        }
    }
}
