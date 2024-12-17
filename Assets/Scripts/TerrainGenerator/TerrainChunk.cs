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
            MeshGenerator meshGenerator, int size, float scale,
            float persistance, float lacunarity, int octaves, int seed, Vector2 offset, float noiseMultiplier,
            AnimationCurve heightCurve, int lod, Material material, Gradient gradient, Vector3 position)
        {
            this.noiseGenerator = noiseGenerator;
            this.meshGenerator = meshGenerator;
            chunkGameObject = new GameObject("Terrain Chunk");
            chunkGameObject.transform.position = position;

            meshRenderer = chunkGameObject.AddComponent<MeshRenderer>();
            meshFilter = chunkGameObject.AddComponent<MeshFilter>();


            float[,] heightMap =
                this.noiseGenerator.GenerateHeightMap(size, scale, persistance, lacunarity, octaves, seed,
                    new Vector2(position.x, position.z) + offset);

            MeshData meshData = meshGenerator.CreateMeshData(heightMap, noiseMultiplier, heightCurve, lod);

            meshFilter.mesh = meshData.CreateMesh();
            meshRenderer.material = material;
            meshRenderer.material.mainTexture =
                textureGenerator.CreateTextureFromGradient(heightMap, gradient, size);
        }
    }
}
