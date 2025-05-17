using System;
using System.Collections.Generic;
using System.Linq;
using Const;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Sirenix.OdinInspector;
using StaticData.Data;
using StaticData.Services;
using UnityEditor;
using UnityEngine;
using Zenject;


namespace Structures
{
    public class NatureStructureRoot : SerializedMonoBehaviour
    {
        [SerializeField] private List<GameObject[]> objectsBatches;
        [SerializeField] private List<DestructibleObjectBase> structureDestructibleObjects;
        public List<StructureObject> structureChildSettings;
        private Dictionary<string, List<GameObject>> identicalObjectsGroupPairs;
        private NatureObjectsAnimations natureObjectsAnimations;


        [Inject]
        private void Construct(StaticDataService staticDataService)
        {
            natureObjectsAnimations = staticDataService.AnimationsConfig.natureObjectsAnimations;
        }
        

        private async void Start()
        {
            await AnimateObjects();
            BatchObjects();
        }


        private async UniTask AnimateObjects()
        {
            foreach (DestructibleObjectBase destructibleObject in structureDestructibleObjects)
            {
                destructibleObject.transform.localScale = Vector3.zero;
            }
            
            foreach (DestructibleObjectBase destructibleObject in structureDestructibleObjects)
            {
                destructibleObject.transform.DOScale(Vector3.one, natureObjectsAnimations.growAnimationTime);
                await UniTask.Yield();
            }

            await UniTask.WaitForSeconds(natureObjectsAnimations.growAnimationTime);
        }


        private void BatchObjects()
        {
            foreach (GameObject[] group in objectsBatches)
            {
                StaticBatchingUtility.Combine(group, gameObject);
            }
        }



#if UNITY_EDITOR


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


        [Button]
        public void FindStructureChildObjects()
        {
            structureChildSettings = new List<StructureObject>();

            StructureObject[] structureChildren = GetComponentsInChildren<StructureObject>();

            DestructibleObjectBase[] destructibleObjects = GetComponentsInChildren<DestructibleObjectBase>();

            structureChildSettings = structureChildren.ToList();
            structureDestructibleObjects = destructibleObjects.ToList();
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
