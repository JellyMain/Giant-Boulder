using Sirenix.OdinInspector;
using UnityEngine;


namespace TerrainGenerator
{
    public class ChunkGenerator : MonoBehaviour
    {
        [SerializeField] private AnimationCurve heightCurve;

        [SerializeField] private Material chunkMaterial;

        [SerializeField] private Gradient heightGradient;

        [SerializeField, ValueDropdown("sizeVariants"), OnValueChanged("CreateChunkEditor")]
        private int size = 100;

        [SerializeField, ValueDropdown("levelsOfDetail"), OnValueChanged("CreateChunkEditor")]
        private int lod;

        [SerializeField, Range(0.001f, 100), OnValueChanged("CreateChunkEditor")]
        private float noiseScale = 10;

        [SerializeField, Range(1, 100), OnValueChanged("CreateChunkEditor")]
        private float noiseMultiplier = 10;

        [SerializeField, Range(1, 20), OnValueChanged("CreateChunkEditor")]
        private int octaves = 5;

        [SerializeField, Range(0, 1), OnValueChanged("CreateChunkEditor")]
        private float persistance;

        [SerializeField, Range(1, 10), OnValueChanged("CreateChunkEditor")]
        private float lacunarity;

        [SerializeField] private int seed;

        [SerializeField, OnValueChanged("CreateChunkEditor")]
        private Vector2 offset;

        private readonly NoiseGenerator noiseGenerator = new NoiseGenerator();
        private readonly TextureGenerator textureGenerator = new TextureGenerator();
        private readonly MeshGenerator meshGenerator = new MeshGenerator();

        private int[] levelsOfDetail = new[] { 1, 2, 4, 5, 10, 20, 25, 50 };
        private int[] sizeVariants = new[] { 100, 200, 400 };

        private TerrainChunk editorTerrainChunk;
        public int Size => size;



        public TerrainChunk CreateChunk(Vector3 position)
        {
            editorTerrainChunk = new TerrainChunk(noiseGenerator, textureGenerator, meshGenerator, size, noiseScale,
                persistance,
                lacunarity, octaves, seed, offset, noiseMultiplier, heightCurve, lod, chunkMaterial,
                heightGradient, position);

            return editorTerrainChunk;
        }


        [Button]
        private void CreateChunkEditor()
        {
            // if (editorTerrainChunk != null)
            // {
            //     DestroyImmediate(editorTerrainChunk.chunkGameObject);
            //     editorTerrainChunk = null;
            // }

            CreateChunk(Vector3.zero);
        }
    }
}
