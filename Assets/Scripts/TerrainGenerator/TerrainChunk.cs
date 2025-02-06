using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace TerrainGenerator
{
    public class TerrainChunk
    {
        private readonly MeshRenderer meshRenderer;
        private readonly MeshFilter meshFilter;
        private readonly GameObject chunkGameObject;
        private readonly MeshCollider meshCollider;
        public MeshData meshData;
        

        public TerrainChunk(Material material, Vector3 position, MeshData meshData, Transform parent)
        {
            this.meshData = meshData;
            
            chunkGameObject = new GameObject("Terrain Chunk");
            chunkGameObject.layer = LayerMask.NameToLayer("Ground");

            chunkGameObject.transform.SetParent(parent);

            chunkGameObject.transform.position = position;

            meshRenderer = chunkGameObject.AddComponent<MeshRenderer>();
            meshFilter = chunkGameObject.AddComponent<MeshFilter>();

            meshFilter.mesh = meshData.CreateMesh();
            
            meshCollider = chunkGameObject.AddComponent<MeshCollider>();
            meshCollider.sharedMesh = meshFilter.mesh;
            meshRenderer.material = material;
            
        }
    }
}
