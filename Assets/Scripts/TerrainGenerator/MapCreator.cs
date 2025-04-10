using System.Collections.Generic;
using Const;
using Factories;
using StaticData.Data;
using StaticData.Services;
using TerrainGenerator.Enums;
using Unity.AI.Navigation;
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

        public Dictionary<ChunkBiome, List<TerrainChunk>> SortedChunks { get; private set; } =
            new Dictionary<ChunkBiome, List<TerrainChunk>>();



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

            ChunkBiome[] chunkLandscapeTypes = CalculateChunksLandscapeTypes(allTerrainHeightMaps);

            TerrainChunks = chunkFactory.CreateAllChunks(allChunksCoords, allTerrainHeightMaps,
                chunkLandscapeTypes,
                mapGenerationConfig,
                chunksParent.transform, chunkSize);

            SortTerrainChunks(TerrainChunks);


            int groundLayer = LayerMask.NameToLayer(RuntimeConstants.Layers.GROUND_LAYER);
            NavMeshSurface navMeshSurface = chunksParent.AddComponent<NavMeshSurface>();
            navMeshSurface.collectObjects = CollectObjects.Children;
            navMeshSurface.layerMask = 1 << groundLayer;
            
            navMeshSurface.BuildNavMesh();
        }


        private void CreateChunks(Vector2[] allChunksCoords, float[][] allTerrainHeightMaps,
            ChunkBiome[] chunkLandscapeTypes, GameObject chunksParent)
        {
            TerrainChunks = new Dictionary<Vector2, TerrainChunk>();

            for (int y = 0; y < mapGenerationConfig.mapSize; y++)
            {
                for (int x = 0; x < mapGenerationConfig.mapSize; x++)
                {
                    Vector2 chunkCoord = allChunksCoords[y * mapGenerationConfig.mapSize + x];
                    float[] heightMap = allTerrainHeightMaps[y * mapGenerationConfig.mapSize + x];
                    ChunkBiome chunkBiome = chunkLandscapeTypes[y * mapGenerationConfig.mapSize + x];
                    TerrainChunk terrainChunk = chunkFactory.CreateChunk(chunkCoord, heightMap, chunkBiome,
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
                switch (chunk.ChunkBiome)
                {
                    case ChunkBiome.Plain:
                        if (!SortedChunks.ContainsKey(ChunkBiome.Plain))
                        {
                            SortedChunks[ChunkBiome.Plain] = new List<TerrainChunk>();
                        }

                        SortedChunks[ChunkBiome.Plain].Add(chunk);

                        break;
                    case ChunkBiome.Hill:
                        if (!SortedChunks.ContainsKey(ChunkBiome.Hill))
                        {
                            SortedChunks[ChunkBiome.Hill] = new List<TerrainChunk>();
                        }

                        SortedChunks[ChunkBiome.Hill].Add(chunk);

                        break;
                    case ChunkBiome.Mountain:
                        if (!SortedChunks.ContainsKey(ChunkBiome.Mountain))
                        {
                            SortedChunks[ChunkBiome.Mountain] = new List<TerrainChunk>();
                        }

                        SortedChunks[ChunkBiome.Mountain].Add(chunk);

                        break;
                }
            }
        }


        private ChunkBiome[] CalculateChunksLandscapeTypes(float[][] heightMaps)
        {
            ChunkBiome[] chunkLandscapeTypes = new ChunkBiome[heightMaps.Length];


            for (int i = 0; i < heightMaps.Length; i++)
            {
                float[] heightMap = heightMaps[i];
                ChunkBiome chunkBiome;

                float averageHeight = 0;

                for (int j = 0; j < heightMap.Length; j++)
                {
                    averageHeight += heightMap[j];
                }

                averageHeight /= heightMap.Length;

                switch (averageHeight)
                {
                    case <= 0.45f:
                        chunkBiome = ChunkBiome.Plain;
                        chunkLandscapeTypes[i] = chunkBiome;
                        break;
                    case <= 0.6f:
                        chunkBiome = ChunkBiome.Hill;
                        chunkLandscapeTypes[i] = chunkBiome;
                        break;
                    default:
                        chunkBiome = ChunkBiome.Mountain;
                        chunkLandscapeTypes[i] = chunkBiome;
                        break;
                }
            }

            return chunkLandscapeTypes;
        }
    }
}
