using UnityEngine;
using Random = System.Random;


namespace TerrainGenerator
{
    public class NoiseGenerator
    {
        private readonly FalloffGenerator falloffGenerator;
        private float maxNoiseHeight = float.MinValue;
        private float minNoiseHeight = float.MaxValue;



        public NoiseGenerator(FalloffGenerator falloffGenerator)
        {
            this.falloffGenerator = falloffGenerator;
        }


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
                        float xOffset = rng.Next(-100000, 100000) + offset.x + chunkPosition.x;
                        float yOffset = rng.Next(-100000, 100000) - offset.y + chunkPosition.z;

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

                            heightMap[x, y] = noiseHeight ;


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
    }
}
