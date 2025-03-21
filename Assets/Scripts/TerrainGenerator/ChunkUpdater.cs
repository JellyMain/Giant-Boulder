using System;
using System.Collections.Generic;
using Const;
using Factories;
using GameLoop;
using StaticData.Data;
using StaticData.Services;
using StructuresSpawner;
using TerrainGenerator.Enums;
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
        private LevelCreationWatcher levelCreationWatcher;
        private PlayerFactory playerFactory;


        [Inject]
        private void Construct(StaticDataService staticDataService, MapCreator mapCreator, StructureSpawner structureSpawner,
            LevelCreationWatcher levelCreationWatcher, PlayerFactory playerFactory)
        {
            this.structureSpawner = structureSpawner;
            this.mapCreator = mapCreator;
            this.staticDataService = staticDataService;
            this.levelCreationWatcher = levelCreationWatcher;
            this.playerFactory = playerFactory;
        }


        private void OnEnable()
        {
            levelCreationWatcher.OnLevelCreated += Init;
            playerFactory.OnPlayerCreated += SetPlayer;
        }


        private void OnDisable()
        {
            levelCreationWatcher.OnLevelCreated -= Init;
            playerFactory.OnPlayerCreated -= SetPlayer;
        }


        private void Init()
        {
            mapGenerationConfig = staticDataService.MapConfigForSeason(TerrainSeason.Summer);
            chunksInViewDistance = viewDistance / (mapGenerationConfig.chunkSize - 1);
            terrainChunks = mapCreator.TerrainChunks;
        }


        private void SetPlayer(GameObject player)
        {
            this.player = player.transform;
        }


        private void Update()
        {
            UpdateChunks();
        }


        private void UpdateChunks()
        {
            if (player != null)
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
}
