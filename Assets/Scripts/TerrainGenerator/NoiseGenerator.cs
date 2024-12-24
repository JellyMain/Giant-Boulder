using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Random = System.Random;


namespace TerrainGenerator
{
    public class NoiseGenerator
    {
        private float maxNoiseHeight = float.MinValue;
        private float minNoiseHeight = float.MaxValue;
        private readonly List<NativeArray<float2>> jobOctaveOffsetsList = new List<NativeArray<float2>>();



        public float[,] GenerateHeightMap(int size, float scale, float persistance, float lacunarity, int octaves,
            int seed, Vector2 offset)
        {
            Random rng = new Random(seed);

            Vector2[] octaveOffsets = new Vector2[octaves];

            for (int i = 0; i < octaves; i++)
            {
                float xOffset = rng.Next(-100000, 100000) + offset.x;
                float yOffset = rng.Next(-100000, 100000) - offset.y;

                octaveOffsets[i] = new Vector2(xOffset, yOffset);
            }

            float[,] heightMap = new float[size, size];

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float noiseHeight = 0;
                    float frequency = 1;
                    float amplitude = 1;

                    for (int i = 0; i < octaves; i++)
                    {
                        float positionX = (x - octaveOffsets[i].x) / scale * frequency;
                        float positionY = (y - octaveOffsets[i].y) / scale * frequency;

                        float perlinValue = Mathf.PerlinNoise(positionX, positionY) * 2 - 1;

                        noiseHeight += perlinValue * amplitude;

                        frequency *= lacunarity;
                        amplitude *= persistance;
                    }

                    heightMap[x, y] = noiseHeight;


                    if (noiseHeight > maxNoiseHeight)
                    {
                        maxNoiseHeight = noiseHeight;
                    }

                    if (noiseHeight < minNoiseHeight)
                    {
                        minNoiseHeight = noiseHeight;
                    }
                }
            }


            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    heightMap[x, y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, heightMap[x, y]);
                }
            }


            return heightMap;
        }



        public float[][,] GenerateAllTerrainHeightMaps(int terrainMapSize, int chunkSize, float scale,
            float persistance,
            float lacunarity, int octaves,
            int seed, Vector2 offset, Vector3[] positions)
        {
            float[][,] terrainHeightMaps = new float[terrainMapSize * terrainMapSize][,];

            for (int chunkY = 0; chunkY < terrainMapSize; chunkY++)
            {
                for (int chunkX = 0; chunkX < terrainMapSize; chunkX++)
                {
                    Random rng = new Random(seed);
                    Vector2[] octaveOffsets = new Vector2[octaves];
                    float[,] heightMap = new float[chunkSize, chunkSize];
                    Vector3 chunkPosition = positions[chunkY * terrainMapSize + chunkX];


                    for (int i = 0; i < octaves; i++)
                    {
                        float xOffset = rng.Next(-10000, 10000) + offset.x + chunkPosition.x;
                        float yOffset = rng.Next(-10000, 10000) - offset.y + chunkPosition.z;

                        octaveOffsets[i] = new Vector2(xOffset, yOffset);
                    }

                    for (int y = 0; y < chunkSize; y++)
                    {
                        for (int x = 0; x < chunkSize; x++)
                        {
                            float noiseHeight = 0;
                            float frequency = 1;
                            float amplitude = 1;

                            for (int i = 0; i < octaves; i++)
                            {
                                float positionX = (x + octaveOffsets[i].x) / scale * frequency;
                                float positionY = (y - octaveOffsets[i].y) / scale * frequency;

                                float perlinValue = Mathf.PerlinNoise(positionX, positionY) * 2 - 1;

                                noiseHeight += perlinValue * amplitude;

                                frequency *= lacunarity;
                                amplitude *= persistance;
                            }

                            heightMap[x, y] = noiseHeight;


                            if (noiseHeight > maxNoiseHeight)
                            {
                                maxNoiseHeight = noiseHeight;
                            }

                            if (noiseHeight < minNoiseHeight)
                            {
                                minNoiseHeight = noiseHeight;
                            }
                        }
                    }

                    terrainHeightMaps[chunkY * terrainMapSize + chunkX] = heightMap;
                }
            }


            for (int i = 0; i < terrainHeightMaps.Length; i++)
            {
                float[,] currentHeightMap = terrainHeightMaps[i];


                for (int y = 0; y < chunkSize; y++)
                {
                    for (int x = 0; x < chunkSize; x++)
                    {
                        currentHeightMap[x, y] =
                            Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, currentHeightMap[x, y]);
                    }
                }
            }


            return terrainHeightMaps;
        }



        public float[][] GenerateAllHeightMapsParallel(int terrainMapSize, int chunkSize, float scale,
            float persistance,
            float lacunarity, int octaves,
            int seed, Vector2 offset, Vector3[] positions)
        {
            float globalMinNoiseHeight = float.MaxValue;
            float globalMaxNoiseHeight = float.MinValue;


            NativeArray<float>[] terrainHeightMapsNative = new NativeArray<float>[terrainMapSize * terrainMapSize];
            NativeArray<JobHandle> jobHandles =
                new NativeArray<JobHandle>(terrainHeightMapsNative.Length, Allocator.TempJob);

            for (int chunkY = 0; chunkY < terrainMapSize; chunkY++)
            {
                for (int chunkX = 0; chunkX < terrainMapSize; chunkX++)
                {
                    int index = chunkY * terrainMapSize + chunkX;
                    terrainHeightMapsNative[index] = new NativeArray<float>(chunkSize * chunkSize, Allocator.TempJob);

                    Random rng = new Random(seed);
                    NativeArray<float2> octaveOffsets = new NativeArray<float2>(octaves, Allocator.TempJob);
                    Vector3 chunkPosition = positions[chunkY * terrainMapSize + chunkX];

                    for (int i = 0; i < octaves; i++)
                    {
                        float xOffset = rng.Next(-10000, 10000) + offset.x + chunkPosition.x;
                        float yOffset = rng.Next(-10000, 10000) - offset.y + chunkPosition.z;
                        octaveOffsets[i] = new float2(xOffset, yOffset);
                    }

                    jobOctaveOffsetsList.Add(octaveOffsets);

                    GenerateHeightMapJob heightMapJob = new GenerateHeightMapJob
                    {
                        chunkSize = chunkSize,
                        heightMap = terrainHeightMapsNative[index],
                        lacunarity = lacunarity,
                        octaves = octaves,
                        persistance = persistance,
                        scale = scale,
                        octaveOffsets = octaveOffsets,
                    };

                    jobHandles[index] = heightMapJob.Schedule();
                }
            }

            JobHandle.CompleteAll(jobHandles);


            float[][] terrainHeightMaps = new float[terrainMapSize * terrainMapSize][];

            for (int i = 0; i < terrainHeightMapsNative.Length; i++)
            {
                NativeArray<float> heightMap = terrainHeightMapsNative[i];

                for (int j = 0; j < heightMap.Length; j++)
                {
                    if (heightMap[j] > globalMaxNoiseHeight)
                    {
                        globalMaxNoiseHeight = heightMap[j];
                    }

                    if (heightMap[j] < globalMinNoiseHeight)
                    {
                        globalMinNoiseHeight = heightMap[j];
                    }
                }
            }


            // Texture2D heightMapCorrectionTexture =
            //     Resources.Load<Texture2D>("BaseHeightMaps/map2");
            //
            // Color[] heightMapCorrection = heightMapCorrectionTexture.GetPixels();
            //
            //
            // Color[][] chunkedHeightMapCorrection =
            //     HeightMapCorrectionHelper.DivideCorrectionMap(heightMapCorrection, chunkSize, terrainMapSize);

            for (int i = 0; i < terrainHeightMapsNative.Length; i++)
            {
                terrainHeightMaps[i] = new float[terrainHeightMapsNative[i].Length];
                // Color[] correctionMap = chunkedHeightMapCorrection[i];

                for (int j = 0; j < terrainHeightMaps[i].Length; j++)
                {
                    terrainHeightMaps[i][j] = Mathf.InverseLerp(globalMinNoiseHeight, globalMaxNoiseHeight,
                        terrainHeightMapsNative[i][j]);

                    // terrainHeightMaps[i][j] += (1f - correctionMap[j].grayscale) * 0.1f;
                    //
                    // terrainHeightMaps[i][j] = Mathf.Clamp01(terrainHeightMaps[i][j]);
                }

                terrainHeightMapsNative[i].Dispose();
            }

            foreach (var octaveOffsets in jobOctaveOffsetsList)
            {
                octaveOffsets.Dispose();
            }

            jobHandles.Dispose();
            jobOctaveOffsetsList.Clear();

            return terrainHeightMaps;
        }
    }
}
