using UnityEngine;


namespace TerrainGenerator
{
    public class TextureGenerator
    {
        private Texture2D CreateTextureFromColorMap(int size, Color[] colorMap)
        {
            Texture2D texture = new Texture2D(size, size);

            texture.SetPixels(colorMap);
            texture.filterMode = FilterMode.Point;
            texture.Apply();

            return texture;
        }


        public Texture2D CreateTextureFromHeightMap(float[,] heightMap, int size)
        {
            Color[] colorMap = new Color[size * size];

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    colorMap[y * size + x] = Color.Lerp(Color.white, Color.black, heightMap[x, y]);
                }
            }

            return CreateTextureFromColorMap(size, colorMap);
        }


        public Texture2D CreateTextureFromGradient(float[,] heightMap, Gradient gradient, int size)
        {
            Color[] colorMap = new Color[size * size];

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    colorMap[y * size + x] = gradient.Evaluate(heightMap[x, y]);
                }
            }

            return CreateTextureFromColorMap(size, colorMap);
        }


        public Texture2D CreateTextureFromRegions(float[,] heightMap, TerrainRegion[] terrainRegions, int size)
        {
            Color[] colorMap = new Color[size * size];

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float currentHeight = heightMap[x, y];

                    for (int i = 0; i < terrainRegions.Length; i++)
                    {
                        if (currentHeight <= terrainRegions[i].regionHeight)
                        {
                            colorMap[y * size + x] = terrainRegions[i].regionColor;
                        }
                    }
                }
            }

            return CreateTextureFromColorMap(size, colorMap);
        }
        
    }
}
