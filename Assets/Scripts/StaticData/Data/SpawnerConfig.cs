using System.Collections.Generic;
using Sirenix.OdinInspector;
using TerrainGenerator.Enums;
using UnityEngine;


namespace StaticData.Data
{
    [CreateAssetMenu(menuName = "StaticData/SpawnerConfig", fileName = "SpawnerConfig")]
    public class SpawnerConfig : SerializedScriptableObject
    {
        [ValidateInput(nameof(ValidatePercentages), "The sum of percentages must equal 100.")]
        public Dictionary<ChunkBiome, List<StructuresPercentagePair>> allSpawners;

        public float raycastHeight = 1000f;
        public LayerMask groundLayer;
        public LayerMask structuresLayer;

        private bool ValidatePercentages(Dictionary<ChunkBiome, List<StructuresPercentagePair>> spawners)
        {
            if (spawners == null)
            {
                Debug.LogError("Spawners dictionary is null.");
                return false;
            }

            foreach (KeyValuePair<ChunkBiome, List<StructuresPercentagePair>> pair in spawners)
            {
                ChunkBiome biome = pair.Key;
                
                int biomePercentageSum = 0;

                foreach (StructuresPercentagePair structuresPercentagePair in pair.Value)
                {
                    biomePercentageSum += structuresPercentagePair.spawnRate;
                }

                if (biomePercentageSum != 100)
                {
                    Debug.LogError($"Biome {biome} has an invalid percentage sum: {biomePercentageSum}. It must equal 100.");
                    return false;
                }
            }

            return true;
        }
    }
}
