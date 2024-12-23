using System.Collections.Generic;
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

            int verticesPerLine = (width - 1) / lod ;

            MeshData meshData = new MeshData(verticesPerLine);

            
            for (int y = 0; y < height - 1; y += lod)
            {
                for (int x = 0; x < width - 1; x += lod)
                {
                    float vertexAMultiplier = animationCurve.Evaluate(heightMap[x, y]) * noiseMultiplier;
                    float vertexBMultiplier = animationCurve.Evaluate(heightMap[x + lod, y]) * noiseMultiplier;
                    float vertexCMultiplier = animationCurve.Evaluate(heightMap[x, y + lod]) * noiseMultiplier;
                    float vertexDMultiplier = animationCurve.Evaluate(heightMap[x + lod, y + lod]) * noiseMultiplier;

                    Vector3 vertexA = new Vector3(topLeftX + x, heightMap[x, y] * vertexAMultiplier, topLeftY - y);
                    Vector3 vertexB = new Vector3(topLeftX + x + lod, heightMap[x + lod, y] * vertexBMultiplier,
                        topLeftY - y);
                    Vector3 vertexC = new Vector3(topLeftX + x, heightMap[x, y + lod] * vertexCMultiplier,
                        topLeftY - y - lod);
                    Vector3 vertexD = new Vector3(topLeftX + x + lod, heightMap[x + lod, y + lod] * vertexDMultiplier,
                        topLeftY - y - lod);


                    Color firstTriangleColor = EvaluateVertexColor(vertexA, vertexB, vertexD, vertexAMultiplier,
                        vertexBMultiplier, vertexDMultiplier, gradient);

                    meshData.AddTriangle(vertexA, vertexB, vertexD, firstTriangleColor);


                    Color secondTriangleColor = EvaluateVertexColor(vertexC, vertexA, vertexD, vertexCMultiplier,
                        vertexAMultiplier, vertexDMultiplier, gradient);

                    meshData.AddTriangle(vertexC, vertexA, vertexD, secondTriangleColor);
                    
                }
            }


            return meshData;
        }


        private Color EvaluateVertexColor(Vector3 vertexA, Vector3 vertexB, Vector3 vertexC,
            float vertexScaleA, float vertexScaleB, float vertexScaleC, Gradient gradient)
        {
            float heightA = vertexA.y / vertexScaleA;
            float heightB = vertexB.y / vertexScaleB;
            float heightC = vertexC.y / vertexScaleC;

            float averageHeight = (heightA + heightB + heightC) / 3;

            Color color = gradient.Evaluate(averageHeight);
            return color;
        }
    }
}
