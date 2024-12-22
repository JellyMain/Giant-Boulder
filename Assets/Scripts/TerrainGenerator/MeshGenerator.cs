using StaticData.Services;
using UnityEngine;


namespace TerrainGenerator
{
    public class MeshGenerator
    {
        public MeshData CreateMeshData(float[,] heightMap, float noiseMultiplier, AnimationCurve animationCurve,
            int lod, Gradient gradient)
        {
            int width = heightMap.GetLength(0);
            int height = heightMap.GetLength(1);

            float topLeftX = (width - 1) / -2f;
            float topLeftY = (height - 1) / 2f;

            int verticesPerLine = (width - 1) / lod + 1;

            MeshData meshData = new MeshData(verticesPerLine);

            int currentIndex = 0;

            float[] heightMultipliers = new float[verticesPerLine * verticesPerLine];

            for (int y = 0; y < height; y += lod)
            {
                for (int x = 0; x < width; x += lod)
                {
                    float currentMultiplier = animationCurve.Evaluate(heightMap[x, y]) * noiseMultiplier;
                    heightMultipliers[currentIndex] = currentMultiplier;

                    meshData.vertices[currentIndex] =
                        new Vector3(topLeftX + x, heightMap[x, y] * currentMultiplier, topLeftY - y);

                    if (x < width - lod && y < height - lod)
                    {
                        meshData.AddTriangle(currentIndex, currentIndex + verticesPerLine + 1,
                            currentIndex + verticesPerLine);
                        meshData.AddTriangle(currentIndex + verticesPerLine + 1, currentIndex, currentIndex + 1);
                    }

                    currentIndex++;
                }
            }


            for (int i = 0; i < meshData.triangles.Length; i += 3)
            {
                int vertexIndexA = meshData.triangles[i];
                int vertexIndexB = meshData.triangles[i + 1];
                int vertexIndexC = meshData.triangles[i + 2];

                float vertexHeightA = meshData.vertices[vertexIndexA].y / heightMultipliers[vertexIndexA];
                float vertexHeightB = meshData.vertices[vertexIndexB].y / heightMultipliers[vertexIndexB];
                float vertexHeightC = meshData.vertices[vertexIndexC].y / heightMultipliers[vertexIndexC];

                var normal = Vector3.Cross(meshData.vertices[vertexIndexB] - meshData.vertices[vertexIndexA],
                    meshData.vertices[vertexIndexC] - meshData.vertices[vertexIndexA]);

                float averageHeight = (vertexHeightA + vertexHeightB + vertexHeightC) / 3;

                Color triangleColor = gradient.Evaluate(averageHeight);

                meshData.uvs[vertexIndexA] = Vector2.zero;
                meshData.uvs[vertexIndexB] = Vector2.zero;
                meshData.uvs[vertexIndexC] = Vector2.zero;

                meshData.normals[vertexIndexA] = normal;
                meshData.normals[vertexIndexB] = normal;
                meshData.normals[vertexIndexC] = normal;

                meshData.colors[vertexIndexA] = triangleColor;
                meshData.colors[vertexIndexB] = triangleColor;
                meshData.colors[vertexIndexC] = triangleColor;
            }


            return meshData;
        }


        private Color EvaluateVertexColor(float vertexHeightA, float vertexHeightB, float vertexHeightC,
            Gradient gradient, float vertexScaleA, float vertexScaleB, float vertexScaleC)
        {
            float heightA = vertexHeightA / vertexScaleA;
            float heightB = vertexHeightB / vertexScaleB;
            float heightC = vertexHeightC / vertexScaleC;

            float averageHeight = (heightA + heightB + heightC) / 3;

            Color color = gradient.Evaluate(averageHeight);
            return color;
        }
    }
}
