using Sirenix.OdinInspector;
using UnityEngine;


namespace Structures
{
    public class StructureSpawnSettings : MonoBehaviour
    {
        [SerializeField, MinMaxSlider(-180, 180)]
        private Vector2 rotationModifier;
        [SerializeField, Range(0, 100)] private int spawnChance = 100;
        public bool snapToGround;

        public Vector2 RotationModifier => rotationModifier;
        public int SpawnChance => spawnChance;
    }
}
