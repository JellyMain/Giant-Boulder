using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;


namespace TerrainGenerator
{
    [BurstCompile]
    public struct CreateMeshDataJob : IJob
    {
        [ReadOnly] public NativeArray<Keyframe> heightCurveKeys;
        [ReadOnly] public NativeArray<float> heightMap;
        public int lod;
        public float noiseMultiplier;
        public MeshDataBurstCompatible meshDataBurstCompatible;


        public void Execute()
        {
            int width = (int)math.sqrt(heightMap.Length);
            int height = (int)math.sqrt(heightMap.Length);

            float topLeftX = (width - 1) / -2f;
            float topLeftY = (height - 1) / 2f;


            for (int y = 0; y < height - 1; y += lod)
            {
                for (int x = 0; x < width - 1; x += lod)
                {
                    float vertexAMultiplier =
                        CurveSampling.ThreadSafe.Evaluate(heightCurveKeys, heightMap[y * width + x]) * noiseMultiplier;
                    float vertexBMultiplier =
                        CurveSampling.ThreadSafe.Evaluate(heightCurveKeys, heightMap[y * width + x + lod]) *
                        noiseMultiplier;
                    float vertexCMultiplier =
                        CurveSampling.ThreadSafe.Evaluate(heightCurveKeys, heightMap[(y + lod) * width + x]) *
                        noiseMultiplier;
                    float vertexDMultiplier =
                        CurveSampling.ThreadSafe.Evaluate(heightCurveKeys, heightMap[(y + lod) * width + x + lod]) *
                        noiseMultiplier;

                    float3 vertexA = new float3(topLeftX + x, heightMap[y * width + x] * vertexAMultiplier,
                        topLeftY - y);
                    float3 vertexB = new float3(topLeftX + x + lod,
                        heightMap[y * width + x + lod] * vertexBMultiplier,
                        topLeftY - y);
                    float3 vertexC = new float3(topLeftX + x, heightMap[(y + lod) * width + x] * vertexCMultiplier,
                        topLeftY - y - lod);
                    float3 vertexD = new float3(topLeftX + x + lod,
                        heightMap[(y + lod) * width + x + lod] * vertexDMultiplier,
                        topLeftY - y - lod);


                    float2 uvA = new float2((float)x / (width - 1), (float)y / (height - 1));
                    float2 uvB = new float2((float)(x + lod) / (width - 1), (float)y / (height - 1));
                    float2 uvC = new float2((float)x / (width - 1), (float)(y + lod) / (height - 1));
                    float2 uvD = new float2((float)(x + lod) / (width - 1), (float)(y + lod) / (height - 1));

                    // Color firstTriangleColor = EvaluateVertexColorGradient(vertexA, vertexB, vertexD, vertexAMultiplier,
                    //     vertexBMultiplier, vertexDMultiplier);

                    Color firstTriangleColor = Color.gray;

                    meshDataBurstCompatible.AddTriangle(vertexA, vertexB, vertexD, firstTriangleColor, uvA, uvB, uvD);


                    // Color secondTriangleColor = EvaluateVertexColorGradient(vertexC, vertexA, vertexD,
                    //     vertexCMultiplier,
                    //     vertexAMultiplier, vertexDMultiplier);

                    Color secondTriangleColor = Color.gray;

                    meshDataBurstCompatible.AddTriangle(vertexC, vertexA, vertexD, secondTriangleColor, uvC, uvA, uvD);
                }
            }
        }


        // private Color EvaluateVertexColorGradient(float3 vertexA, float3 vertexB, float3 vertexC,
        //     float vertexScaleA, float vertexScaleB, float vertexScaleC)
        // {
        //     float heightA = vertexA.y / vertexScaleA;
        //     float heightB = vertexB.y / vertexScaleB;
        //     float heightC = vertexC.y / vertexScaleC;
        //
        //     float averageHeight = (heightA + heightB + heightC) / 3;
        //
        //     Color color = gradient.Evaluate(averageHeight);
        //
        //     return color;
        // }
    }
}
