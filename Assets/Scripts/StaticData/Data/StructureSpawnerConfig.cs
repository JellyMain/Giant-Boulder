using System.Collections.Generic;
using Structures;
using UnityEngine;


namespace StaticData.Data
{
    [CreateAssetMenu(menuName = "StaticData/StructureSpawnerConfig", fileName = "StructureSpawnerConfig")]
    public class StructureSpawnerConfig : ScriptableObject
    {
        public int structuresCount = 10;
        public float spawnAttempts = 1000;
        public List<StructureRoot> structurePrefabs;
        public float raycastHeight = 10f;
        public LayerMask groundLayer;
        public LayerMask structuresLayer;
    }
}
