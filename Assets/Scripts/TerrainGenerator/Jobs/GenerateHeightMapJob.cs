using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;


[BurstCompile]
public struct GenerateHeightMapJob : IJobParallelFor
{
    public uint seed;
    public float2 offset;
    public int chunkSize;
    public float scale;
    public float persistance;
    public float lacunarity;
    public int octaves;
    [ReadOnly] public NativeArray<float3> chunkPositions;
    [NativeDisableParallelForRestriction] public NativeArray<float> allHeightMaps;


    public void Execute(int index)
    {
        float3 chunkPosition = chunkPositions[index];
        int chunkResolution = chunkSize * chunkSize;
        int startIndex = index * chunkResolution;

        NativeArray<float2> octaveOffsets = new NativeArray<float2>(octaves, Allocator.Temp);
        Random rng = new Random(seed);

        for (int i = 0; i < octaves; i++)
        {
            float xOffset = rng.NextInt(-10000, 10000) + offset.x + chunkPosition.x;
            float yOffset = rng.NextInt(-10000, 10000) - offset.y + chunkPosition.z;
            octaveOffsets[i] = new float2(xOffset, yOffset);
        }

        for (int y = 0; y < chunkSize; y++)
        {
            for (int x = 0; x < chunkSize; x++)
            {
                float noiseHeight = 0f;
                float frequency = 1f;
                float amplitude = 1f;

                for (int i = 0; i < octaves; i++)
                {
                    float positionX = (x + octaveOffsets[i].x) / scale * frequency;
                    float positionY = (y - octaveOffsets[i].y) / scale * frequency;
                    float2 position = new float2(positionX, positionY);

                    float perlinValue = noise.pnoise(position, new float2(1000, 1000)) * 2 - 1;
                    noiseHeight += perlinValue * amplitude;

                    frequency *= lacunarity;
                    amplitude *= persistance;
                }

                int ind = y * chunkSize + x;
                allHeightMaps[startIndex + ind] = noiseHeight;
            }
        }

        octaveOffsets.Dispose();
    }
}
