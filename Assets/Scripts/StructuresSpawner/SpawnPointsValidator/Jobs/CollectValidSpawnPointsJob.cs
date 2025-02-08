using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;


namespace StructuresSpawner.SpawnPointsValidator.Jobs
{
    [BurstCompile]
    public struct CollectValidSpawnPointsJob : IJob
    {
        [ReadOnly] public NativeArray<int> validSpawnPointsFlags;
        [ReadOnly] public NativeArray<float3> vertices;
        public NativeList<float2> validSpawnPoints;


        public void Execute()
        {
            for (int i = 0; i < validSpawnPointsFlags.Length; i++)
            {
                if (validSpawnPointsFlags[i] == 1)
                {
                    float3 vertex = vertices[i];
                    validSpawnPoints.Add(new float2(vertex.x, vertex.z));
                }
            }
        }
    }
}
