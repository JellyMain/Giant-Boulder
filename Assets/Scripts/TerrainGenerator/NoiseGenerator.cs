using UnityEngine;
using Random = System.Random;


namespace TerrainGenerator
{
    public class NoiseGenerator
    {
        private float maxNoiseHeight = float.MinValue;
        private float minNoiseHeight = float.MaxValue;


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
    }
}
