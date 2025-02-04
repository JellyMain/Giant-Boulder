using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;


namespace Structures
{
    public class StructureRoot : MonoBehaviour
    {
        [SerializeField] private float structureRadius = 10;
        [SerializeField] private float maxSlopeAngle = 30;
        public List<StructureSpawnSettings> structureChildSettings;

        public float MaxSlopeAngle => maxSlopeAngle;
        public float StructureRadius => structureRadius;



        [Button]
        public void FindStructureChildObjects()
        {
            structureChildSettings = new List<StructureSpawnSettings>();

            StructureSpawnSettings[] structureChildren = GetComponentsInChildren<StructureSpawnSettings>();

            structureChildSettings = structureChildren.ToList();
        }



        private void OnDrawGizmosSelected()
        {
            Gizmos.DrawWireSphere(transform.position, StructureRadius);
        }
    }
}
