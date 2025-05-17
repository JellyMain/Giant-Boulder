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
        [SerializeField] private int chunkUpdateDistance = 200;
        [SerializeField] private int grassRenderDistance = 100;
        public Transform player;
        private int chunksInViewDistance;
        private int grassChunksInViewDistance;
        private Dictionary<Vector2, TerrainChunk> terrainChunks;
        private StaticDataService staticDataService;
        private MapCreator mapCreator;
        private MapGenerationConfig mapGenerationConfig;
        private StructureSpawner structureSpawner;
        private LevelCreationWatcher levelCreationWatcher;
        private PlayerFactory playerFactory;
        private GrassSpawner grassSpawner;
        private Camera mainCamera;
        private readonly List<TerrainChunk> grassChunks = new List<TerrainChunk>();
        private readonly List<TerrainChunk> visibleChunks = new List<TerrainChunk>();
        private readonly List<TerrainChunk> lastFrameVisibleChunks = new List<TerrainChunk>();


        [Inject]
        private void Construct(StaticDataService staticDataService, MapCreator mapCreator,
            StructureSpawner structureSpawner,
            LevelCreationWatcher levelCreationWatcher, PlayerFactory playerFactory, GrassSpawner grassSpawner)
        {
            this.structureSpawner = structureSpawner;
            this.mapCreator = mapCreator;
            this.staticDataService = staticDataService;
            this.levelCreationWatcher = levelCreationWatcher;
            this.playerFactory = playerFactory;
            this.grassSpawner = grassSpawner;
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
            chunksInViewDistance = chunkUpdateDistance / (mapGenerationConfig.chunkSize - 1);
            grassChunksInViewDistance = grassRenderDistance / (mapGenerationConfig.chunkSize - 1);
            terrainChunks = mapCreator.TerrainChunks;
            mainCamera = Camera.main;
            SubscribeOnTerrainChunks();
        }


        private void SetPlayer(GameObject player)
        {
            this.player = player.transform;
        }


        private void Update()
        {
            GetVisibleChunks();
            GetGrassChunks();
            UpdateVisibleChunks();
        }


        private void SubscribeOnTerrainChunks()
        {
            foreach (TerrainChunk terrainChunk in terrainChunks.Values)
            {
                terrainChunk.OnChunkCleared += SpawnNatureStructure;
            }
        }


        private void GetGrassChunks()
        {
            if (player != null)
            {
                int currentChunkCoordX = Mathf.RoundToInt(player.position.x / (mapGenerationConfig.chunkSize - 1));
                int currentChunkCoordY = Mathf.RoundToInt(player.position.z / (mapGenerationConfig.chunkSize - 1));

                grassChunks.Clear();

                for (int xOffset = -grassChunksInViewDistance; xOffset < grassChunksInViewDistance; xOffset++)
                {
                    for (int yOffset = -grassChunksInViewDistance; yOffset < grassChunksInViewDistance; yOffset++)
                    {
                        Vector2 chunkCoord = new Vector2(Mathf.Max(0, currentChunkCoordX + xOffset),
                            Mathf.Max(0, currentChunkCoordY + yOffset));

                        if (terrainChunks.TryGetValue(chunkCoord, out TerrainChunk chunk))
                        {
                            Bounds chunkBounds = chunk.bounds;

                            if (IsInCameraView(mainCamera, chunkBounds))
                            {
                                grassChunks.Add(chunk);
                            }
                        }
                    }
                }
            }
        }
        

        private void UpdateVisibleChunks()
        {
            foreach (TerrainChunk chunk in lastFrameVisibleChunks)
            {
                chunk.DisableRender();
            }

            lastFrameVisibleChunks.Clear();

            foreach (TerrainChunk chunk in visibleChunks)
            {
                chunk.Render();
                lastFrameVisibleChunks.Add(chunk);
            }

            foreach (TerrainChunk grassChunk in grassChunks)
            {
                grassSpawner.RenderGrassChunk(grassChunk);
            }
        }



        private void GetVisibleChunks()
        {
            if (player != null)
            {
                int currentChunkCoordX = Mathf.RoundToInt(player.position.x / (mapGenerationConfig.chunkSize - 1));
                int currentChunkCoordY = Mathf.RoundToInt(player.position.z / (mapGenerationConfig.chunkSize - 1));

                visibleChunks.Clear();

                for (int xOffset = -chunksInViewDistance; xOffset < chunksInViewDistance; xOffset++)
                {
                    for (int yOffset = -chunksInViewDistance; yOffset < chunksInViewDistance; yOffset++)
                    {
                        Vector2 chunkCoord = new Vector2(Mathf.Max(0, currentChunkCoordX + xOffset),
                            Mathf.Max(0, currentChunkCoordY + yOffset));

                        if (terrainChunks.ContainsKey(chunkCoord))
                        {
                            TerrainChunk chunk = terrainChunks[chunkCoord];
                            Bounds chunkBounds = chunk.bounds;

                            if (IsInCameraView(mainCamera, chunkBounds))
                            {
                                visibleChunks.Add(chunk);
                            }

                            if (!chunk.structuresInstantiated)
                            {
                                structureSpawner.SpawnStructureInChunk(terrainChunks[chunkCoord]);
                            }
                        }
                    }
                }
            }
        }


        private void SpawnNatureStructure(TerrainChunk terrainChunk)
        {
            structureSpawner.SpawnNatureInChunk(terrainChunk);
        }


        private bool IsInCameraView(Camera currentCamera, Bounds objectBounds)
        {
            Plane[] frustumPlanes = GeometryUtility.CalculateFrustumPlanes(currentCamera);

            if (GeometryUtility.TestPlanesAABB(frustumPlanes, objectBounds))
            {
                return true;
            }

            return false;
        }
    }
}
