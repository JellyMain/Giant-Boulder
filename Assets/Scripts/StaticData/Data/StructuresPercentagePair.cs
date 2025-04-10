using System;
using Structures;
using UnityEngine;


namespace StaticData.Data
{
    [Serializable]
    public class StructuresPercentagePair
    {
        public StructureRoot structurePrefab;
        [Range(0, 100)] public int spawnRate;
    }
}