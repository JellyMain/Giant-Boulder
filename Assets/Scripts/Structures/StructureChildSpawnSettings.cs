using Sirenix.OdinInspector;
using UnityEngine;


namespace Structures
{
    public class StructureChildSpawnSettings : MonoBehaviour
    {
        [SerializeField, MinMaxSlider(-180, 180)]
        private Vector2 rotationModifier;
        [SerializeField, Range(0, 100)] private int spawnChance = 100;
        [SerializeField] private bool snapToGround;

        public Vector2 RotationModifier => rotationModifier;
        public int SpawnChance => spawnChance;
        public bool SnapToGround => snapToGround;
    }
}
