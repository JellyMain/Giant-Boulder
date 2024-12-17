using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;


namespace TerrainGenerator
{
    public class MapCreator : MonoBehaviour
    {
        [SerializeField] private int mapSize = 10;
        private ChunkGenerator chunkGenerator;
        private List<TerrainChunk> createdChunks = new List<TerrainChunk>();


        private void Awake()
        {
            chunkGenerator = GetComponent<ChunkGenerator>();
        }


        private void Start()
        {
            CreateMap();
        }



        [Button]
        private void CreateMapEditor()
        {
            chunkGenerator = GetComponent<ChunkGenerator>();

            if (createdChunks.Count != 0)
            {
                foreach (TerrainChunk chunk in createdChunks)
                {
                    DestroyImmediate(chunk.chunkGameObject);
                }

                createdChunks.Clear();
            }

            for (int y = 0; y < mapSize; y++)
            {
                for (int x = 0; x < mapSize; x++)
                {
                    Vector3 position = new Vector3(x * chunkGenerator.Size, 0, y * chunkGenerator.Size);
                    createdChunks.Add(chunkGenerator.CreateChunk(position));
                }
            }
        }


        private void CreateMap()
        {
            chunkGenerator = GetComponent<ChunkGenerator>();

            for (int y = 0; y < mapSize; y++)
            {
                for (int x = 0; x < mapSize; x++)
                {
                    Vector3 position = new Vector3(x * chunkGenerator.Size, 0, y * chunkGenerator.Size);
                    chunkGenerator.CreateChunk(position);
                }
            }
        }
    }
}
