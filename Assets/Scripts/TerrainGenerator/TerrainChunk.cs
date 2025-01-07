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


        public TerrainChunk(Material material, Vector3 position, MeshData meshData)
        {
            chunkGameObject = new GameObject("Terrain Chunk");
            chunkGameObject.transform.position = position;

            meshRenderer = chunkGameObject.AddComponent<MeshRenderer>();
            meshFilter = chunkGameObject.AddComponent<MeshFilter>();

            meshFilter.mesh = meshData.CreateMesh();
            
           meshCollider =  chunkGameObject.AddComponent<MeshCollider>();
           meshCollider.sharedMesh = meshFilter.mesh;

            meshRenderer.material = material;
        }
    }
}
