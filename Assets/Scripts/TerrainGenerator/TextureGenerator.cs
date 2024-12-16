using UnityEngine;


namespace TerrainGenerator
{
    public class TextureGenerator
    {
        public Texture2D CreateTextureFromColorMap(int size, Color[] colorMap)
        {
            Texture2D texture = new Texture2D(size, size);

            texture.SetPixels(colorMap);
            texture.filterMode = FilterMode.Point;
            texture.Apply();

            return texture;
        }


        public Texture2D CreateTextureFromHeightMap(float[,] heightMap)
        {
            int width = heightMap.GetLength(0);
            int height = heightMap.GetLength(1);

            Color[] colorMap = new Color[width * height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    colorMap[y * width + x] = Color.Lerp(Color.white, Color.black, heightMap[x, y]);
                }
            }
            
            return CreateTextureFromColorMap(width, colorMap);
        }
    }
}
