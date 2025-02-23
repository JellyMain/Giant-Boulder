using System.Collections.Generic;
using Sirenix.OdinInspector;
using Structures;
using TerrainGenerator.Enums;
using UnityEngine;


namespace StaticData.Data
{
    [CreateAssetMenu(menuName = "StaticData/StructureSpawnerConfig", fileName = "StructureSpawnerConfig")]
    public class SpawnerConfig : SerializedScriptableObject
    {
        public Dictionary<StructureRoot, int> structurePrefabCountPair;
        public float raycastHeight = 1000f;
        public LayerMask groundLayer;
        public LayerMask structuresLayer;
        public ChunkLandscapeType chunkLandscapeType;
        public int spawnOrder;
        public bool isEnabled;
    }
}
