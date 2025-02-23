using System.Collections.Generic;
using Factories;
using StaticData.Data;
using StaticData.Services;
using TerrainGenerator.Enums;
using UnityEngine;
using Utils;


namespace TerrainGenerator
{
    public class MapCreator
    {
        private readonly NoiseGenerator noiseGenerator;
        private readonly StaticDataService staticDataService;
        private readonly ChunkFactory chunkFactory;
        private MapGenerationConfig mapGenerationConfig;
        private int chunkSize;
        public Dictionary<Vector2, TerrainChunk> TerrainChunks { get; private set; }

        public Dictionary<ChunkLandscapeType, List<TerrainChunk>> SortedChunks { get; private set; } =
            new Dictionary<ChunkLandscapeType, List<TerrainChunk>>();



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

            Vector2[] allChunksCoords = new Vector2[mapGenerationConfig.mapSize * mapGenerationConfig.mapSize];

            for (int y = 0; y < mapGenerationConfig.mapSize; y++)
            {
                for (int x = 0; x < mapGenerationConfig.mapSize; x++)
                {
                    Vector3 chunkCoord = new Vector2(x, y);

                    allChunksCoords[y * mapGenerationConfig.mapSize + x] = chunkCoord;
                }
            }

            float[][] allTerrainHeightMaps = noiseGenerator.GenerateAllHeightMapsParallel(
                mapGenerationConfig.mapSize,
                mapGenerationConfig.chunkSize,
                mapGenerationConfig.noiseScale, mapGenerationConfig.persistance, mapGenerationConfig.lacunarity,
                mapGenerationConfig.octaves, mapGenerationConfig.seed, mapGenerationConfig.offset, allChunksCoords);

            ChunkLandscapeType[] chunkLandscapeTypes = CalculateChunksLandscapeTypes(allTerrainHeightMaps);

            TerrainChunks = chunkFactory.CreateAllChunks(allChunksCoords, allTerrainHeightMaps,
                chunkLandscapeTypes,
                mapGenerationConfig,
                chunksParent.transform, chunkSize);

            SortTerrainChunks(TerrainChunks);
        }


        private void CreateChunks(Vector2[] allChunksCoords, float[][] allTerrainHeightMaps,
            ChunkLandscapeType[] chunkLandscapeTypes, GameObject chunksParent)
        {
            TerrainChunks = new Dictionary<Vector2, TerrainChunk>();

            for (int y = 0; y < mapGenerationConfig.mapSize; y++)
            {
                for (int x = 0; x < mapGenerationConfig.mapSize; x++)
                {
                    Vector2 chunkCoord = allChunksCoords[y * mapGenerationConfig.mapSize + x];
                    float[] heightMap = allTerrainHeightMaps[y * mapGenerationConfig.mapSize + x];
                    ChunkLandscapeType chunkLandscapeType = chunkLandscapeTypes[y * mapGenerationConfig.mapSize + x];
                    TerrainChunk terrainChunk = chunkFactory.CreateChunk(chunkCoord, heightMap, chunkLandscapeType,
                        mapGenerationConfig,
                        chunksParent.transform, mapGenerationConfig.chunkSize - 1);

                    TerrainChunks.Add(chunkCoord, terrainChunk);
                }
            }
        }


        private void SortTerrainChunks(Dictionary<Vector2, TerrainChunk> terrainChunks)
        {
            foreach (TerrainChunk chunk in terrainChunks.Values)
            {
                switch (chunk.ChunkLandscapeType)
                {
                    case ChunkLandscapeType.Plain:
                        if (!SortedChunks.ContainsKey(ChunkLandscapeType.Plain))
                        {
                            SortedChunks[ChunkLandscapeType.Plain] = new List<TerrainChunk>();
                        }

                        SortedChunks[ChunkLandscapeType.Plain].Add(chunk);

                        break;
                    case ChunkLandscapeType.Hill:
                        if (!SortedChunks.ContainsKey(ChunkLandscapeType.Hill))
                        {
                            SortedChunks[ChunkLandscapeType.Hill] = new List<TerrainChunk>();
                        }

                        SortedChunks[ChunkLandscapeType.Hill].Add(chunk);

                        break;
                    case ChunkLandscapeType.Mountain:
                        if (!SortedChunks.ContainsKey(ChunkLandscapeType.Mountain))
                        {
                            SortedChunks[ChunkLandscapeType.Mountain] = new List<TerrainChunk>();
                        }

                        SortedChunks[ChunkLandscapeType.Mountain].Add(chunk);

                        break;
                }
            }
        }


        private ChunkLandscapeType[] CalculateChunksLandscapeTypes(float[][] heightMaps)
        {
            ChunkLandscapeType[] chunkLandscapeTypes = new ChunkLandscapeType[heightMaps.Length];


            for (int i = 0; i < heightMaps.Length; i++)
            {
                float[] heightMap = heightMaps[i];
                ChunkLandscapeType chunkLandscapeType;

                float averageHeight = 0;

                for (int j = 0; j < heightMap.Length; j++)
                {
                    averageHeight += heightMap[j];
                }

                averageHeight /= heightMap.Length;

                switch (averageHeight)
                {
                    case <= 0.45f:
                        chunkLandscapeType = ChunkLandscapeType.Plain;
                        chunkLandscapeTypes[i] = chunkLandscapeType;
                        break;
                    case <= 0.6f:
                        chunkLandscapeType = ChunkLandscapeType.Hill;
                        chunkLandscapeTypes[i] = chunkLandscapeType;
                        break;
                    default:
                        chunkLandscapeType = ChunkLandscapeType.Mountain;
                        chunkLandscapeTypes[i] = chunkLandscapeType;
                        break;
                }
            }

            return chunkLandscapeTypes;
        }
    }
}