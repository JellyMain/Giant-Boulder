using System;
using Structures;
using UnityEngine;


namespace StaticData.Data
{
    [Serializable]
    public class StructuresPercentagePair
    {
        public PollutionStructureRoot pollutionStructurePrefab;
        public NatureStructureRoot natureStructurePrefab;
        [Range(0, 100)] public int spawnRate;
    }
}