using UnityEngine;


namespace TerrainGenerator
{
    public class TerrainChunk
    {
        private MeshRenderer meshRenderer;
        private MeshFilter meshFilter;
        private readonly GameObject chunkGameObject;


        public TerrainChunk(Material material, Vector3 position, MeshData meshData)
        {
            chunkGameObject = new GameObject("Terrain Chunk");
            chunkGameObject.transform.position = position;

            meshRenderer = chunkGameObject.AddComponent<MeshRenderer>();
            meshFilter = chunkGameObject.AddComponent<MeshFilter>();

            meshFilter.mesh = meshData.CreateMesh();

            meshRenderer.material = material;
        }
    }
}