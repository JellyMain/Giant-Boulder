using System.Collections.Generic;
using StaticData.Services;
using Unity.Collections;
using UnityEngine;


namespace TerrainGenerator
{
    public class MeshGenerator
    {
        public MeshData CreateMeshData(float[,] heightMap, float noiseMultiplier, AnimationCurve heightCurve,
            int lod, Gradient gradient, TerrainRegion[] regions)
        {
            int width = heightMap.GetLength(0);
            int height = heightMap.GetLength(1);

            float topLeftX = (width - 1) / -2f;
            float topLeftY = (height - 1) / 2f;

            int verticesPerLine = (width - 1) / lod;

            MeshData meshData = new MeshData(verticesPerLine);


            for (int y = 0; y < height - 1; y += lod)
            {
                for (int x = 0; x < width - 1; x += lod)
                {
                    float vertexAMultiplier = heightCurve.Evaluate(heightMap[x, y]) * noiseMultiplier;
                    float vertexBMultiplier = heightCurve.Evaluate(heightMap[x + lod, y]) * noiseMultiplier;
                    float vertexCMultiplier = heightCurve.Evaluate(heightMap[x, y + lod]) * noiseMultiplier;
                    float vertexDMultiplier = heightCurve.Evaluate(heightMap[x + lod, y + lod]) * noiseMultiplier;

                    Vector3 vertexA = new Vector3(topLeftX + x, heightMap[x, y] * vertexAMultiplier, topLeftY - y);
                    Vector3 vertexB = new Vector3(topLeftX + x + lod, heightMap[x + lod, y] * vertexBMultiplier,
                        topLeftY - y);
                    Vector3 vertexC = new Vector3(topLeftX + x, heightMap[x, y + lod] * vertexCMultiplier,
                        topLeftY - y - lod);
                    Vector3 vertexD = new Vector3(topLeftX + x + lod, heightMap[x + lod, y + lod] * vertexDMultiplier,
                        topLeftY - y - lod);


                    Color firstTriangleColor = EvaluateVertexColorGradient(vertexA, vertexB, vertexD, vertexAMultiplier,
                        vertexBMultiplier, vertexDMultiplier, gradient);

                    meshData.AddTriangle(vertexA, vertexB, vertexD, firstTriangleColor);


                    Color secondTriangleColor = EvaluateVertexColorGradient(vertexC, vertexA, vertexD,
                        vertexCMultiplier,
                        vertexAMultiplier, vertexDMultiplier, gradient);

                    meshData.AddTriangle(vertexC, vertexA, vertexD, secondTriangleColor);
                }
            }


            return meshData;
        }



        public MeshData CreateMeshData(float[] heightMap, float noiseMultiplier, AnimationCurve heightCurve,
            int lod, Gradient gradient, TerrainRegion[] regions)
        {
            AnimationCurve currentHeightCurve = new AnimationCurve(heightCurve.keys);

            int width = (int)Mathf.Sqrt(heightMap.Length);
            int height = (int)Mathf.Sqrt(heightMap.Length);

            float topLeftX = (width - 1) / -2f;
            float topLeftY = (height - 1) / 2f;

            int verticesPerLine = (width - 1) / lod;

            MeshData meshData = new MeshData(verticesPerLine);


            for (int y = 0; y < height - 1; y += lod)
            {
                for (int x = 0; x < width - 1; x += lod)
                {
                    float vertexAMultiplier = currentHeightCurve.Evaluate(heightMap[y * width + x]) * noiseMultiplier;
                    float vertexBMultiplier =
                        currentHeightCurve.Evaluate(heightMap[y * width + x + lod]) * noiseMultiplier;
                    float vertexCMultiplier =
                        currentHeightCurve.Evaluate(heightMap[(y + lod) * width + x]) * noiseMultiplier;
                    float vertexDMultiplier = currentHeightCurve.Evaluate(heightMap[(y + lod) * width + x + lod]) *
                                              noiseMultiplier;

                    Vector3 vertexA = new Vector3(topLeftX + x, heightMap[y * width + x] * vertexAMultiplier,
                        topLeftY - y);
                    Vector3 vertexB = new Vector3(topLeftX + x + lod,
                        heightMap[y * width + x + lod] * vertexBMultiplier,
                        topLeftY - y);
                    Vector3 vertexC = new Vector3(topLeftX + x, heightMap[(y + lod) * width + x] * vertexCMultiplier,
                        topLeftY - y - lod);
                    Vector3 vertexD = new Vector3(topLeftX + x + lod,
                        heightMap[(y + lod) * width + x + lod] * vertexDMultiplier,
                        topLeftY - y - lod);


                    Color firstTriangleColor = EvaluateVertexColorGradient(vertexA, vertexB, vertexD, vertexAMultiplier,
                        vertexBMultiplier, vertexDMultiplier, gradient);

                    meshData.AddTriangle(vertexA, vertexB, vertexD, firstTriangleColor);


                    Color secondTriangleColor = EvaluateVertexColorGradient(vertexC, vertexA, vertexD,
                        vertexCMultiplier,
                        vertexAMultiplier, vertexDMultiplier, gradient);

                    meshData.AddTriangle(vertexC, vertexA, vertexD, secondTriangleColor);
                }
            }


            return meshData;
        }



        private Color EvaluateVertexColorGradient(Vector3 vertexA, Vector3 vertexB, Vector3 vertexC,
            float vertexScaleA, float vertexScaleB, float vertexScaleC, Gradient gradient)
        {
            float heightA = vertexA.y / vertexScaleA;
            float heightB = vertexB.y / vertexScaleB;
            float heightC = vertexC.y / vertexScaleC;

            float averageHeight = (heightA + heightB + heightC) / 3;

            Color color = gradient.Evaluate(averageHeight);

            return color;
        }


        private Color EvaluateVertexColorRegions(Vector3 vertexA, Vector3 vertexB, Vector3 vertexC,
            float vertexScaleA, float vertexScaleB, float vertexScaleC, TerrainRegion[] regions)
        {
            float heightA = vertexA.y / vertexScaleA;
            float heightB = vertexB.y / vertexScaleB;
            float heightC = vertexC.y / vertexScaleC;

            float averageHeight = (heightA + heightB + heightC) / 3;

            foreach (TerrainRegion region in regions)
            {
                if (region.regionHeight > averageHeight)
                {
                    return region.regionColor;
                }
            }

            return Color.magenta;
        }
    }
}
