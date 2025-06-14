using System;
using System.Collections.Generic;
using Const;
using Cysharp.Threading.Tasks;
using StaticData.Data;
using Structures;
using TerrainGenerator.Data;
using TerrainGenerator.Enums;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEngine;


namespace TerrainGenerator
{
    public class TerrainChunk
    {
        private readonly MeshRenderer meshRenderer;
        private readonly MeshFilter meshFilter;
        private readonly MeshCollider meshCollider;
        public readonly GameObject chunkGameObject;
        public ChunkBiome ChunkBiome { get; private set; }
        public Bounds bounds;
        public MeshData meshData;
        public Vector3 position;
        public bool structuresInstantiated;
        public PollutionStructureRoot currentPollutionStructure;
        public PollutionStructureRoot pollutionStructurePrefab;
        public NatureStructureRoot natureStructurePrefab;
        public NatureStructureRoot currentNatureStructure;
        public event Action<TerrainChunk> OnChunkCleared;


        public TerrainChunk(Material material, Vector2 chunkCoord, MeshData meshData,
            ChunkBiome chunkBiome, Transform parent, int chunkSize)
        {
            this.meshData = meshData;
            position = new Vector3(chunkCoord.x * chunkSize, 0, chunkCoord.y * chunkSize);
            bounds = new Bounds(position, new Vector3(1 * chunkSize, 100, 1 * chunkSize));
            ChunkBiome = chunkBiome;

            int groundLayer = LayerMask.NameToLayer("Ground");

            chunkGameObject = new GameObject($"Terrain Chunk {ChunkBiome.ToString()}");
            chunkGameObject.layer = groundLayer;

            chunkGameObject.transform.SetParent(parent);

            chunkGameObject.transform.position = position;

            meshRenderer = chunkGameObject.AddComponent<MeshRenderer>();
            meshFilter = chunkGameObject.AddComponent<MeshFilter>();

            meshRenderer.renderingLayerMask = 1 << 1;

            meshFilter.mesh = meshData.CreateMesh();
            meshCollider = chunkGameObject.AddComponent<MeshCollider>();
            meshCollider.sharedMesh = meshFilter.mesh;
            meshRenderer.material = material;

            DisableRender();
        }


        public void SubscribeOnPollutionStructure()
        {
            currentPollutionStructure.OnPollutionCleared += ChunkCleared;
        }


        private void ChunkCleared()
        {
            OnChunkCleared?.Invoke(this);
            Debug.Log("Chunk Cleared");
        }
        

        public void Render()
        {
            if (currentPollutionStructure != null)
            {
                currentPollutionStructure.EnableAllMeshRenderers();
            }

            meshRenderer.enabled = true;
        }


        public void DisableRender()
        {
            if (currentPollutionStructure != null)
            {
                currentPollutionStructure.DisableAllMeshRenderers();
            }

            meshRenderer.enabled = false;
        }
        
    }
}
