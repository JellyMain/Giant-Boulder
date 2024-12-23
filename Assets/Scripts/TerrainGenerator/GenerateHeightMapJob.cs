using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;


namespace TerrainGenerator
{
    [BurstCompile]
    public struct GenerateHeightMapJob : IJob
    {
        public int chunkSize;
        public float scale;
        public float persistance;
        public float lacunarity;
        public int octaves;
        [ReadOnly] public NativeArray<float2> octaveOffsets;

        public NativeArray<float> heightMap;


        public void Execute()
        {
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

                    int index = y * chunkSize + x;
                    heightMap[index] = noiseHeight;
                }
            }
        }
    }
}
