using UnityEngine;


namespace TerrainGenerator
{
    public class FalloffGenerator
    {
        public float[,] GenerateFalloffMap(int size)
        {
            float[,] falloffMap = new float[size, size];

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float mapX = x / (float)size * 2 - 1;
                    float mapY = y / (float)size * 2 - 1;

                    float value = Mathf.Max(Mathf.Abs(mapX), Mathf.Abs(mapY));
                    falloffMap[x, y] = Evaluate(value);
                }
            }

            return falloffMap;
        }



        private float Evaluate(float value)
        {
            float a = 3;
            float b = 2.2f;

            return Mathf.Pow(value, a) / (Mathf.Pow(value, a) + Mathf.Pow(b - b * value, a));
        }
    }
}
