using UnityEditor;
using UnityEngine;


namespace TerrainGenerator
{
    public class TerrainChunk
    {
        private MeshRenderer meshRenderer;
        public MeshFilter meshFilter;
        private readonly GameObject chunkGameObject;
        private readonly MeshCollider meshCollider;
        private readonly Rigidbody chunkRigidbody;


        public TerrainChunk(Material material, Vector3 position, MeshData meshData, Transform parent)
        {
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
