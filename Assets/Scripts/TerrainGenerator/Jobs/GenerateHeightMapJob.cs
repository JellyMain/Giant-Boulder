using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;


namespace TerrainGenerator.Jobs
{
    [BurstCompile]
    public struct GenerateHeightMapJob : IJob
    {
        public uint seed;
        public float2 offset;
        public int chunkSize;
        public float scale;
        public float persistance;
        public float lacunarity;
        public int octaves;
        public float2 chunkPosition;
        public NativeArray<float> heightMap;


        public void Execute()
        {
            NativeArray<float2> octaveOffsets = new NativeArray<float2>(octaves, Allocator.Temp);
            Random rng = new Random(seed);

            for (int i = 0; i < octaves; i++)
            {
                float xOffset = rng.NextInt(-10000, 10000) + offset.x + chunkPosition.x * (chunkSize - 1);
                float yOffset = rng.NextInt(-10000, 10000) - offset.y + chunkPosition.y * (chunkSize - 1);
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
                    heightMap[ind] = noiseHeight;
                }
            }

            octaveOffsets.Dispose();
        }
    }
}
