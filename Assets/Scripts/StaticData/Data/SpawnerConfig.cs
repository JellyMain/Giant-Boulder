using System.Collections.Generic;
using Sirenix.OdinInspector;
using Structures;
using UnityEngine;


namespace StaticData.Data
{
    [CreateAssetMenu(menuName = "StaticData/StructureSpawnerConfig", fileName = "StructureSpawnerConfig")]
    public class SpawnerConfig : SerializedScriptableObject
    {
        public float spawnAttempts = 1000;
        public Dictionary<StructureRoot, int> structurePrefabCountPair;
        public float raycastHeight = 1000f;
        public LayerMask groundLayer;
        public LayerMask structuresLayer;
        public int slopeCheckRaysAmount = 20;
        public int spawnOrder;
    }
}
