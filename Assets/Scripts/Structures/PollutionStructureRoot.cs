using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Const;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;


namespace Structures
{
    public class PollutionStructureRoot : SerializedMonoBehaviour
    {
        [SerializeField] private List<GameObject[]> objectsBatches;
        [SerializeField] private List<DestructibleObjectBase> structureDestructibleObjects;
        public List<StructureObject> structureChildSettings;
        public Dictionary<MeshRenderer, DestructibleObjectBase> structureMeshRendererObjectMap;
        private Dictionary<string, List<GameObject>> identicalObjectsGroupPairs;
        public event Action OnPollutionCleared;


        private IEnumerator Start()
        {
            ObserveAllObjectsDestruction();
            yield return null;
            RemoveNotSpawnedObjects();
        }

        
        private void ObserveAllObjectsDestruction()
        {
            foreach (DestructibleObjectBase destructibleObject in structureDestructibleObjects)
            {
                destructibleObject.OnDestroyed += OnObjectDestroyed;
            }
        }


        public void RemoveNotSpawnedObjects()
        {
            structureDestructibleObjects.RemoveAll(item => item == null);
        }


        private void OnObjectDestroyed(DestructibleObjectBase destructibleObject)
        {
            destructibleObject.OnDestroyed -= OnObjectDestroyed;
            structureDestructibleObjects.Remove(destructibleObject);

            if (structureDestructibleObjects.Count == 0)
            {
                Debug.Log("Cleared");
                OnPollutionCleared?.Invoke();
            }
        }



        public void DisableAllMeshRenderers()
        {
            foreach (KeyValuePair<MeshRenderer, DestructibleObjectBase> pair in structureMeshRendererObjectMap)
            {
                if (pair.Key != null)
                {
                    pair.Key.enabled = false;
                }
            }
        }


        public void EnableAllMeshRenderers()
        {
            foreach (KeyValuePair<MeshRenderer, DestructibleObjectBase> pair in structureMeshRendererObjectMap)
            {
                if (pair.Key != null)
                {
                    if (pair.Value != null)
                    {
                        if (!pair.Value.IsDestroyed)
                        {
                            pair.Key.enabled = true;
                        }
                    }
                    else
                    {
                        pair.Key.enabled = true;
                    }
                }
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
            Gizmos.DrawWireCube(transform.position, new Vector3(100, 1, 100));
        }



#if UNITY_EDITOR


        [Button]
        private void Test()
        {
            structureDestructibleObjects.RemoveAll(item => item == null);
        }


        [Button]
        private void DestroyWholeStructure()
        {
            foreach (DestructibleObjectBase destructibleObject in structureDestructibleObjects)
            {
                Destroy(destructibleObject.gameObject);
            }

            OnPollutionCleared?.Invoke();
        }


        [Button]
        private void FindMeshRenderers()
        {
            structureMeshRendererObjectMap = new Dictionary<MeshRenderer, DestructibleObjectBase>();

            MeshRenderer[] meshRenderers = GetComponentsInChildren<MeshRenderer>();

            foreach (MeshRenderer meshRenderer in meshRenderers)
            {
                DestructibleObjectBase objectBase =
                    meshRenderer.gameObject.GetComponentInParent<DestructibleObjectBase>();

                structureMeshRendererObjectMap[meshRenderer] = objectBase;
            }
        }



        [Button]
        private void FindStructureChildObjects()
        {
            structureChildSettings = new List<StructureObject>();

            StructureObject[] structureChildren = GetComponentsInChildren<StructureObject>();

            DestructibleObjectBase[] destructibleObjects = GetComponentsInChildren<DestructibleObjectBase>();

            structureChildSettings = structureChildren.ToList();
            structureDestructibleObjects = destructibleObjects.ToList();
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
    }
}
