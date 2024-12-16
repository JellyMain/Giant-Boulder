using UnityEngine;


namespace TerrainGenerator
{
    public class NoiseGenerator
    {
        public float[,] GenerateHeightMap(int size, float scale)
        {
            float[,] heightMap = new float[size, size];


            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float positionX = x / scale;
                    float positionY = y / scale;

                    heightMap[x, y] = Mathf.PerlinNoise(positionX, positionY);
                }
            }

            return heightMap;
        }
    }
}