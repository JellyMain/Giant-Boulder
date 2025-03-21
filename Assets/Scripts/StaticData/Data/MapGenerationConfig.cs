using Sirenix.OdinInspector;
using TerrainGenerator;
using TerrainGenerator.Enums;
using UnityEngine;


namespace StaticData.Data
{
    [CreateAssetMenu(menuName = "StaticData/MapGenerationConfig", fileName = "MapGenerationConfig")]
    public class MapGenerationConfig : ScriptableObject
    {
        public TerrainSeason terrainSeason;

        [Title("Terrain Color")] public ColorMode colorMode;

        [ShowIf("@colorMode ==TerrainGenerator.ColorMode.Gradient")]
        public Gradient heightGradient;

        [ShowIf("@colorMode ==TerrainGenerator.ColorMode.Regions")]
        public TerrainRegion[] terrainRegions;

        [Title("Terrain Settings")] public int mapSize = 10;

        public int seed;

        public Vector2 offset;

        [Title("Chunk Settings")] public AnimationCurve heightCurve;

        public Material chunkMaterial;

        [ValueDropdown("sizeVariants")] public int chunkSize = 100;

        [ValueDropdown("levelsOfDetail")] public int lod;

        [Range(0.001f, 1000)] public float noiseScale = 10;

        [Range(1, 500)] public float noiseMultiplier = 10;

        [Range(1, 20)] public int octaves = 5;

        [Range(0, 1)] public float persistance;

        [Range(1, 10)] public float lacunarity;



        private int[] levelsOfDetail = new[] { 1, 2, 5 };
        private int[] sizeVariants = new[] { 26, 51, 101 };
    }
}