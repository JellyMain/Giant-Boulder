using Unity.Collections;
using UnityEngine;


namespace TerrainGenerator
{
    public static class HeightMapCorrectionHelper
    {
        public static Color[][] DivideCorrectionMap(Color[] correctionMap, int chunkSize, int mapSize)
        {
            int totalChunks = mapSize * mapSize;
            Color[][] chunkedCorrectionMap = new Color[totalChunks][];

            for (int i = 0; i < mapSize; i++)
            {
                for (int j = 0; j < mapSize; j++)
                {
                    Color[] correctionChunk = new Color[chunkSize * chunkSize];

                    for (int y = 0; y < chunkSize; y++)
                    {
                        for (int x = 0; x < chunkSize; x++)
                        {
                            int sourceIndex = (i * chunkSize + y) * mapSize * chunkSize + (j * chunkSize + x);
                            int chunkIndex = y * chunkSize + x;

                            correctionChunk[chunkIndex] = correctionMap[sourceIndex];
                        }
                    }

                    chunkedCorrectionMap[i * mapSize + j] = correctionChunk;
                }
            }

            return chunkedCorrectionMap;
        }
    }
}
