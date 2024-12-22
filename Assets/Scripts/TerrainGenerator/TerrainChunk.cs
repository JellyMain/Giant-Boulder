using UnityEngine;


namespace TerrainGenerator
{
    public class TerrainChunk
    {
        private readonly NoiseGenerator noiseGenerator;
        private readonly MeshGenerator meshGenerator;
        private MeshRenderer meshRenderer;
        private MeshFilter meshFilter;
        public readonly GameObject chunkGameObject;


        public TerrainChunk(NoiseGenerator noiseGenerator, TextureGenerator textureGenerator,
            MeshGenerator meshGenerator, int size, float noiseMultiplier, AnimationCurve heightCurve, int lod,
            Material material, Gradient gradient, Vector3 position, float[,] heightMap, TerrainRegion[] terrainRegions)
        {
            this.noiseGenerator = noiseGenerator;
            this.meshGenerator = meshGenerator;

            chunkGameObject = new GameObject("Terrain Chunk");
            chunkGameObject.transform.position = position;

            meshRenderer = chunkGameObject.AddComponent<MeshRenderer>();
            meshFilter = chunkGameObject.AddComponent<MeshFilter>();


            MeshData meshData = meshGenerator.CreateMeshData(heightMap, noiseMultiplier, heightCurve, lod, gradient);

            meshFilter.mesh = meshData.CreateMesh();
            
            meshRenderer.material = material;
            
            // meshRenderer.material.mainTexture =
            //     textureGenerator.CreateTextureFromGradient(heightMap, gradient, size);

            // meshRenderer.material.mainTexture =
            //     textureGenerator.CreateTextureFromRegions(heightMap, terrainRegions, size);
        }
    }
}