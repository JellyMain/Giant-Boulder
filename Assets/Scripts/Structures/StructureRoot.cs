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
        [SerializeField] private List<DestructibleObjectBase> structureKeyObjects;
        [SerializeField] private List<DestructibleObjectBase> structureDestructibleObjects;
        public List<StructureObject> structureChildSettings;
        private Dictionary<string, List<GameObject>> identicalObjectsGroupPairs;

        public float MaxSlopeAngle => maxSlopeAngle;
        public float StructureRadius => structureRadius;


#if UNITY_EDITOR


        [Button]
        public void FindStructureChildObjects()
        {
            structureChildSettings = new List<StructureObject>();

            StructureObject[] structureChildren = GetComponentsInChildren<StructureObject>();

            DestructibleObjectBase[] destructibleObjects = GetComponentsInChildren<DestructibleObjectBase>();

            structureChildSettings = structureChildren.ToList();
            structureDestructibleObjects = destructibleObjects.ToList();

            structureKeyObjects = new List<DestructibleObjectBase>();

            foreach (StructureObject structureObject in structureChildSettings)
            {
                if (structureObject.IsKeyObject)
                {
                    DestructibleObjectBase destructibleObject = structureObject.GetComponent<DestructibleObjectBase>();
                    structureKeyObjects.Add(destructibleObject);
                }
            }
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


#endif


        private void Start()
        {
            ObserveKeyObjectsDestruction();
            ObserveAllObjectsDestruction();
        }



        private void ObserveKeyObjectsDestruction()
        {
            foreach (DestructibleObjectBase keyObject in structureKeyObjects)
            {
                keyObject.OnDestroyed += OnKeyObjectDestroyed;
            }
        }


        private void ObserveAllObjectsDestruction()
        {
            foreach (DestructibleObjectBase destructibleObject in structureDestructibleObjects)
            {
                destructibleObject.OnDestroyed += OnObjectDestroyed;
            }
        }


        private void OnObjectDestroyed(DestructibleObjectBase destructibleObject)
        {
            destructibleObject.OnDestroyed -= OnObjectDestroyed;
            structureDestructibleObjects.Remove(destructibleObject);
        }


        private void OnKeyObjectDestroyed(DestructibleObjectBase keyObject)
        {
            keyObject.OnDestroyed -= OnKeyObjectDestroyed;
            structureKeyObjects.Remove(keyObject);

            if (structureKeyObjects.Count == 0)
            {
                DestroyStructure();
            }
        }


        private void DestroyStructure()
        {
            foreach (StructureObject structureObject in structureChildSettings)
            {
                if (structureObject != null)
                {
                    Destroy(structureObject.gameObject);
                }
            }
            
            structureChildSettings.Clear();

            // foreach (DestructibleObjectBase destructibleObject in structureDestructibleObjects)
            // {
            //     destructibleObject.OnDestroyed -= OnObjectDestroyed;
            //     destructibleObject.DestroyLight();
            // }
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