using UnityEngine;


namespace TerrainGenerator
{
    public class MeshGenerator
    {
        public MeshData CreateMeshData(float[,] heightMap)
        {
            int width = heightMap.GetLength(0);
            int height = heightMap.GetLength(1);

            MeshData meshData = new MeshData(width);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int currentIndex = y * width + x;

                    meshData.vertices[currentIndex] = new Vector3(x, heightMap[x, y], y);
                    meshData.uvs[currentIndex] = new Vector2((float)x / width, (float)y / height);

                    if (x < width - 1 && y < height - 1)
                    {
                        meshData.AddTriangle(currentIndex, currentIndex + width, currentIndex + width + 1);
                        meshData.AddTriangle(currentIndex + width + 1, currentIndex + 1, currentIndex);
                    }

                }
            }

            return meshData;
        }
    }
}
