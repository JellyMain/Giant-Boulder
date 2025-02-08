using System.Diagnostics;
using TerrainGenerator.Data;
using TerrainGenerator.GradientBurst;
using TerrainGenerator.Jobs;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Debug = UnityEngine.Debug;


namespace TerrainGenerator
{
    public class MeshGenerator
    {
        public MeshData[] CreateAllMeshDataParallel(float[][] heightMaps, float noiseMultiplier,
            AnimationCurve heightCurve,
            int lod, Gradient gradient)
        {
            NativeArray<JobHandle> jobHandles = new NativeArray<JobHandle>(heightMaps.GetLength(0), Allocator.Temp);
            NativeArray<NativeArray<float>> nativeHeightMaps =
                new NativeArray<NativeArray<float>>(heightMaps.GetLength(0), Allocator.Temp);
            NativeArray<Keyframe> heightCurveKeys =
                new NativeArray<Keyframe>(heightCurve.keys.Length, Allocator.TempJob);
            NativeArray<MeshDataNative> meshDataNatives =
                new NativeArray<MeshDataNative>(heightMaps.GetLength(0), Allocator.Temp);

            GradientStruct.ReadOnly gradientReadOnly = gradient.DirectAccessReadOnly();


            SetHeightCurveKeys(heightCurve, heightCurveKeys);


            for (int i = 0; i < nativeHeightMaps.Length; i++)
            {
                int verticesPerLine = (int)math.sqrt(heightMaps[i].Length) / lod;

                nativeHeightMaps[i] = new NativeArray<float>(heightMaps[i].Length, Allocator.TempJob);
                NativeArray<float> nativeHeightMap = nativeHeightMaps[i];

                for (int j = 0; j < heightMaps[i].Length; j++)
                {
                    nativeHeightMap[j] = heightMaps[i][j];
                }

                MeshDataNative meshDataNative = new MeshDataNative(verticesPerLine);
                meshDataNatives[i] = meshDataNative;

                CreateMeshDataJob createMeshDataJob = new CreateMeshDataJob()
                {
                    heightCurveKeys = heightCurveKeys,
                    heightMap = nativeHeightMap,
                    lod = lod,
                    noiseMultiplier = noiseMultiplier,
                    meshDataNative = meshDataNatives[i],
                    gradientReadOnly = gradientReadOnly
                };

                jobHandles[i] = createMeshDataJob.Schedule();
            }

            JobHandle.CompleteAll(jobHandles);

            MeshData[] allMeshData = CopyToMeshData(heightMaps.GetLength(0), meshDataNatives, nativeHeightMaps);

            jobHandles.Dispose();
            heightCurveKeys.Dispose();
            meshDataNatives.Dispose();
            nativeHeightMaps.Dispose();
            
            return allMeshData;
        }


        private static MeshData[] CopyToMeshData(int meshDataArraySize, NativeArray<MeshDataNative> meshDataNatives,
            NativeArray<NativeArray<float>> nativeHeightMaps)
        {
            MeshData[] allMeshData = new MeshData[meshDataArraySize];

            for (int i = 0; i < allMeshData.Length; i++)
            {
                MeshDataNative meshDataNative = meshDataNatives[i];
                MeshData meshData = new MeshData(meshDataNative.verticesPerLine);

                for (int j = 0; j < meshDataNative.vertices.Length; j++)
                {
                    meshData.Vertices[j] = meshDataNative.vertices[j];
                    meshData.Triangles[j] = meshDataNative.triangles[j];
                    meshData.Uvs[j] = meshDataNative.uvs[j];
                    meshData.Colors[j] = meshDataNative.colors[j];
                }

                allMeshData[i] = meshData;
                nativeHeightMaps[i].Dispose();
                meshDataNative.Dispose();
            }

            return allMeshData;
        }


        private static void SetHeightCurveKeys(AnimationCurve heightCurve, NativeArray<Keyframe> heightCurveKeys)
        {
            for (int i = 0; i < heightCurveKeys.Length; i++)
            {
                heightCurveKeys[i] = heightCurve[i];
            }
        }
        

        public MeshData CreateMeshData(float[] heightMap, float noiseMultiplier, AnimationCurve heightCurve,
            int lod, Gradient gradient)
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


                    Vector2 uvA = new Vector2((float)x / (width - 1), (float)y / (height - 1));
                    Vector2 uvB = new Vector2((float)(x + lod) / (width - 1), (float)y / (height - 1));
                    Vector2 uvC = new Vector2((float)x / (width - 1), (float)(y + lod) / (height - 1));
                    Vector2 uvD = new Vector2((float)(x + lod) / (width - 1), (float)(y + lod) / (height - 1));

                    Color firstTriangleColor = EvaluateVertexColorGradient(vertexA, vertexB, vertexD, vertexAMultiplier,
                        vertexBMultiplier, vertexDMultiplier, gradient);

                    meshData.AddTriangle(vertexA, vertexB, vertexD, firstTriangleColor, uvA, uvB, uvD);


                    Color secondTriangleColor = EvaluateVertexColorGradient(vertexC, vertexA, vertexD,
                        vertexCMultiplier,
                        vertexAMultiplier, vertexDMultiplier, gradient);

                    meshData.AddTriangle(vertexC, vertexA, vertexD, secondTriangleColor, uvC, uvA, uvD);
                }
            }


            return meshData;
        }


        public MeshData CreateMeshData(float[,] heightMap, float noiseMultiplier, AnimationCurve heightCurve,
            int lod, Gradient gradient)
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

                    Vector2 uvA = new Vector2((float)x / (width - 1), (float)y / (height - 1));
                    Vector2 uvB = new Vector2((float)(x + lod) / (width - 1), (float)y / (height - 1));
                    Vector2 uvC = new Vector2((float)x / (width - 1), (float)(y + lod) / (height - 1));
                    Vector2 uvD = new Vector2((float)(x + lod) / (width - 1), (float)(y + lod) / (height - 1));


                    Color firstTriangleColor = EvaluateVertexColorGradient(vertexA, vertexB, vertexD, vertexAMultiplier,
                        vertexBMultiplier, vertexDMultiplier, gradient);

                    meshData.AddTriangle(vertexA, vertexB, vertexD, firstTriangleColor, uvA, uvB, uvD);


                    Color secondTriangleColor = EvaluateVertexColorGradient(vertexC, vertexA, vertexD,
                        vertexCMultiplier,
                        vertexAMultiplier, vertexDMultiplier, gradient);

                    meshData.AddTriangle(vertexC, vertexA, vertexD, secondTriangleColor, uvC, uvA, uvD);
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
    }
}
