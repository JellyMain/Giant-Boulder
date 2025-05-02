using System;
using System.Collections.Generic;
using System.Linq;
using Const;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;


namespace Structures
{
    public class StructureRoot : SerializedMonoBehaviour
    {
        [SerializeField] private List<GameObject[]> objectsBatches;
        [SerializeField] private List<DestructibleObjectBase> structureKeyObjects;
        [SerializeField] private List<DestructibleObjectBase> structureDestructibleObjects;
        public List<StructureObject> structureChildSettings;
        public Dictionary<MeshRenderer, DestructibleObjectBase> structureMeshRendererObjectMap;
        private Dictionary<string, List<GameObject>> identicalObjectsGroupPairs;



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
    }
}
