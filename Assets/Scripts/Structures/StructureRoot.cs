using System;
using System.Collections.Generic;
using System.Linq;
using Const;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;


namespace Structures
{
    public class StructureRoot : SerializedMonoBehaviour
    {
        [SerializeField] private float structureRadius = 10;
        [SerializeField] private float maxSlopeAngle = 30;
        [SerializeField] private List<GameObject[]> objectsBatches;
        public List<StructureSpawnSettings> structureChildSettings;
        private Dictionary<string, List<GameObject>> identicalObjectsGroupPairs;

        public float MaxSlopeAngle => maxSlopeAngle;
        public float StructureRadius => structureRadius;


        [Button]
        public void FindStructureChildObjects()
        {
            structureChildSettings = new List<StructureSpawnSettings>();

            StructureSpawnSettings[] structureChildren = GetComponentsInChildren<StructureSpawnSettings>();

            structureChildSettings = structureChildren.ToList();
        }


        [Button]
        private void GroupIdenticalPrefabs()
        {
            identicalObjectsGroupPairs = new Dictionary<string, List<GameObject>>();
            objectsBatches = new List<GameObject[]>();
            GroupIdenticalPrefabsRecursive(transform);

            foreach (List<GameObject> group in identicalObjectsGroupPairs.Values)
            {
                objectsBatches.Add(group.ToArray());
            }
        }


        private void GroupIdenticalPrefabsRecursive(Transform parent)
        {
            if (parent.CompareTag(RuntimeConstants.Tags.FRAGMENTS_ROOT))
            {
                return;
            }

            foreach (Transform child in parent)
            {
                if (PrefabUtility.IsPartOfAnyPrefab(child) && !child.CompareTag(RuntimeConstants.Tags.FRAGMENTS_ROOT))
                {
                    string prefabPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(child.gameObject);

                    if (!identicalObjectsGroupPairs.ContainsKey(prefabPath))
                    {
                        identicalObjectsGroupPairs.Add(prefabPath, new List<GameObject>());
                    }

                    identicalObjectsGroupPairs[prefabPath].Add(child.gameObject);
                }

                GroupIdenticalPrefabsRecursive(child);
            }
        }


        public void BatchObjects()
        {
            foreach (GameObject[] group in objectsBatches)
            {
                StaticBatchingUtility.Combine(group, gameObject);
            }
        }


        private void OnDrawGizmos()
        {
            Gizmos.DrawWireSphere(transform.position, StructureRadius);
            Gizmos.DrawWireCube(transform.position, new Vector3(100, 1, 100));
        }
    }
}
