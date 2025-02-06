using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;


namespace StructuresSpawner
{
    [BurstCompile]
    public struct AddValidSpawnPointsJob : IJob
    {
        [ReadOnly] public NativeArray<float3> vertices;
        [ReadOnly] public NativeArray<float3> normals;
        public NativeList<float2> validSpawnPoints;
        public int verticesPerLine;
        public int vertexCheckStep;


        public void Execute()
        {
            for (int i = 2; i < vertices.Length; i += vertexCheckStep)
            {
                float3 vertex = vertices[i];
                float3 normal = normals[i];

                float slope = math.degrees(math.acos(math.clamp(math.dot(normal, math.up()), -1f, 1f)));

                float totalSlope = slope;
                int count = 1;


                for (int x = -2; x < 2; x++)
                {
                    for (int y = -2; y < 2; y++)
                    {
                        int neighborIndex = i + x + y * verticesPerLine;

                        if (neighborIndex >= 0 && neighborIndex < vertices.Length)
                        {
                            float3 neighborNormal = normals[neighborIndex];
                            float neighborSlope =
                                math.degrees(math.acos(math.clamp(math.dot(neighborNormal, math.up()), -1f, 1f)));

                            totalSlope += neighborSlope;
                            count++;
                        }
                    }
                }

                float averageSlope = totalSlope / count;

                if (averageSlope <= 45)
                {
                    validSpawnPoints.Add(new Vector2(vertex.x, vertex.z));
                }
            }
        }
    }
}
