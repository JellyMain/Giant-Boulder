using System;
using System.Collections.Generic;
using Const;
using Cysharp.Threading.Tasks;
using StaticData.Data;
using StaticData.Services;
using StructuresSpawner;
using UnityEngine;
using Zenject;


namespace TerrainGenerator
{
    public class ChunkUpdater : MonoBehaviour
    {
        [SerializeField] private int viewDistance = 200;
        public Transform player;
        private int chunksInViewDistance;
        private Dictionary<Vector2, TerrainChunk> terrainChunks;
        private StaticDataService staticDataService;
        private MapCreator mapCreator;
        private MapGenerationConfig mapGenerationConfig;
        private StructureSpawner structureSpawner;
        

        [Inject]
        private void Init(StaticDataService staticDataService, MapCreator mapCreator, StructureSpawner structureSpawner)
        {
            this.structureSpawner = structureSpawner;
            this.mapCreator = mapCreator;
            this.staticDataService = staticDataService;
        }


        private void Start()
        {
            mapGenerationConfig = staticDataService.MapConfigForSeason(TerrainSeason.Summer);
            chunksInViewDistance = viewDistance / (mapGenerationConfig.chunkSize - 1);
            terrainChunks = mapCreator.TerrainChunks;
        }


        private void Update()
        {
            UpdateChunks();
        }


        private void UpdateChunks()
        {
            int currentChunkCoordX = Mathf.RoundToInt(player.position.x / (mapGenerationConfig.chunkSize - 1));
            int currentChunkCoordY = Mathf.RoundToInt(player.position.z / (mapGenerationConfig.chunkSize - 1));

            for (int xOffset = -chunksInViewDistance; xOffset < chunksInViewDistance; xOffset++)
            {
                for (int yOffset = -chunksInViewDistance; yOffset < chunksInViewDistance; yOffset++)
                {
                    Vector2 chunkCoord = new Vector2(Mathf.Max(0, currentChunkCoordX + xOffset),
                        Mathf.Max(0, currentChunkCoordY + yOffset));

                    if (terrainChunks.ContainsKey(chunkCoord))
                    {
                        if (!terrainChunks[chunkCoord].structuresInstantiated)
                        {
                            structureSpawner.SpawnStructuresInChunk(terrainChunks[chunkCoord]);
                        }
                    }
                }
            }
        }
    }
}
