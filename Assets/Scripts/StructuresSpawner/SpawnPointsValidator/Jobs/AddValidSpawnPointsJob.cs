using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;


[BurstCompile]
public struct AddValidSpawnPointsJob : IJobParallelFor
{
    [ReadOnly] public NativeArray<float3> vertices;
    [ReadOnly] public NativeArray<float3> normals;
    [WriteOnly] public NativeArray<int> validSpawnPointsFlags; // 1 if valid, 0 otherwise
    public int verticesPerLine;
    public int vertexCheckStep;

    public void Execute(int index)
    {
        if (index % vertexCheckStep != 0) return;

        // Check if the current vertex is valid
        float3 vertex = vertices[index];
        float3 normal = normals[index];
        float slope = math.degrees(math.acos(math.clamp(math.dot(normal, math.up()), -1f, 1f)));
        if (slope > 10) // If the current vertex is too steep, mark it as invalid
        {
            validSpawnPointsFlags[index] = 0;
            return;
        }

        // Check neighboring vertices
        int x = index % verticesPerLine;
        int y = index / verticesPerLine;

        for (int dy = -1; dy <= 1; dy++)
        {
            for (int dx = -1; dx <= 1; dx++)
            {
                int neighborX = x + dx;
                int neighborY = y + dy;

                // Skip out-of-bounds indices
                if (neighborX < 0 || neighborX >= verticesPerLine || neighborY < 0 || neighborY >= verticesPerLine)
                    continue;

                int neighborIndex = neighborY * verticesPerLine + neighborX;
                float3 neighborNormal = normals[neighborIndex];

                // Calculate slope of the neighbor
                float neighborSlope = math.degrees(math.acos(math.clamp(math.dot(neighborNormal, math.up()), -1f, 1f)));

                // If any neighbor is too steep, mark the current vertex as invalid
                if (neighborSlope > 10)
                {
                    validSpawnPointsFlags[index] = 0;
                    return;
                }
            }
        }

        // If all checks pass, mark the vertex as valid
        validSpawnPointsFlags[index] = 1;
    }
}