using UnityEngine;


namespace TerrainGenerator
{
    public class MeshGenerator
    {   
        public MeshData CreateMeshData(float[,] heightMap, float noiseMultiplier, AnimationCurve animationCurve,
            int lod)
        {
            int width = heightMap.GetLength(0);
            int height = heightMap.GetLength(1);

            float topLeftX = (width - 1) / -2f;
            float topLeftY = (height - 1) / 2f;

            int verticesPerLine = (width - 1) / lod + 1;
            
            MeshData meshData = new MeshData(verticesPerLine);
            
            int currentIndex = 0;

            for (int y = 0; y < height; y += lod)
            {
                for (int x = 0; x < width; x += lod)
                {
                    float currentMultiplier = animationCurve.Evaluate(heightMap[x, y]) * noiseMultiplier;

                    meshData.vertices[currentIndex] =
                        new Vector3(topLeftX + x, heightMap[x, y] * currentMultiplier, topLeftY - y);
                    meshData.uvs[currentIndex] = new Vector2((float)x / width, (float)y / height);

                    if (x < width - lod && y < height - lod)
                    {
                        meshData.AddTriangle(currentIndex, currentIndex + verticesPerLine + 1,
                            currentIndex + verticesPerLine);
                        meshData.AddTriangle(currentIndex + verticesPerLine + 1, currentIndex, currentIndex + 1);
                    }

                    currentIndex++;
                }
            }

            return meshData;
        }
    }
}
