using System.Collections.Generic;
using Const;
using RayFire;
using Sirenix.OdinInspector;
using Structures;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
using RayFireEditor;
#endif


namespace Utils
{
    public class PrefabPreparer : SerializedMonoBehaviour
    {
        [SerializeField] private float maxBombPower = 20;
        [SerializeField] private float minBombPower = 0.5f;
        [SerializeField] private float maxObjectVolume = 900;
        [SerializeField] private float minObjectVolume = 5;
        [SerializeField] private int maxObjectFragments = 20;
        [SerializeField] private int minObjectFragments = 3;
        [SerializeField] private Material defaultInsideMaterial;
        [SerializeField] private Dictionary<GameObject, Material> uniqueObjects;
        private HashSet<GameObject> uniquePrefabs;

#if UNITY_EDITOR



        [Button]
        private void FindUniquePrefabs()
        {
            uniquePrefabs = new HashSet<GameObject>();
            uniqueObjects = new Dictionary<GameObject, Material>();

            FindUniquePrefabsRecursive(transform);
        }



        [Button]
        private void PrepareStructure()
        {
            foreach (GameObject child in uniqueObjects.Keys)
            {
                ShatterObject(child);
            }

            AddStructureComponents();
        }



        private void FindUniquePrefabsRecursive(Transform parent)
        {
            foreach (Transform child in parent)
            {
                if (PrefabUtility.IsPartOfAnyPrefab(child))
                {
                    if (!child.CompareTag(RuntimeConstants.Tags.CONTAINER) &&
                        !child.CompareTag(RuntimeConstants.Tags.INDESTRUCTIBLE))
                    {
                        string prefabPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(child.gameObject);
                        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

                        if (!uniquePrefabs.Contains(prefab) && !child.GetComponent<StructureObject>())
                        {
                            uniqueObjects.Add(child.gameObject, defaultInsideMaterial);
                        }

                        uniquePrefabs.Add(prefab);
                    }
                }

                FindUniquePrefabsRecursive(child);
            }
        }


        private void AddStructureComponents()
        {
            AddComponentsToUniqueObjects();
            AddComponentsToRestObjects();

            if (!gameObject.GetComponent<StructureRoot>())
            {
                gameObject.AddComponent<StructureRoot>();
            }
        }


        private void AddComponentsToRestObjects()
        {
            foreach (Transform child in transform)
            {
                if (child.CompareTag(RuntimeConstants.Tags.CONTAINER) &&
                    !child.GetComponent<StructureObject>() ||
                    child.CompareTag(RuntimeConstants.Tags.INDESTRUCTIBLE) &&
                    !child.GetComponent<StructureObject>())
                {
                    StructureObject structureObject = child.AddComponent<StructureObject>();
                    structureObject.snapToGround = true;
                }
            }
        }


        private void AddComponentsToUniqueObjects()
        {
            foreach (GameObject child in uniqueObjects.Keys)
            {
                if (!HasStructureComponents(child))
                {
                    LayerMask structureObjectLayer =
                        LayerMask.NameToLayer(RuntimeConstants.Layers.STRUCTURE_OBJECT_LAYER);
                    LayerMask structureFragmentedLayer =
                        LayerMask.NameToLayer(RuntimeConstants.Layers.STRUCTURE_FRAGMENTED_LAYER);

                    int structureFragmentedLayerMask = 1 << structureFragmentedLayer;

                    child.layer = structureObjectLayer;

                    DestructibleObject destructibleObject = child.AddComponent<DestructibleObject>();
                    destructibleObject.FindFragmentsRoot();

                    StructureObject structureObject = child.AddComponent<StructureObject>();
                    structureObject.snapToGround = true;

                    RayfireBomb rayfireBomb = child.AddComponent<RayfireBomb>();
                    rayfireBomb.range = 30;
                    rayfireBomb.affectInactive = true;

                    float objectVolume = CalculateObjectVolume(child);
                    float bombPower = CalculateBombPower(objectVolume);

                    rayfireBomb.strength = bombPower;
                    rayfireBomb.mask = structureFragmentedLayerMask;

                    if (!child.GetComponent<Collider>())
                    {
                        child.AddComponent<BoxCollider>();
                    }

                    child.AddComponent<CoinSpawner>();
                }
            }
        }


        private void ShatterObject(GameObject obj)
        {
            RayfireShatter rayfireShatter = obj.AddComponent<RayfireShatter>();

            float objectVolume = CalculateObjectVolume(obj);
            int fragmentsAmount = CalculateFragmentsAmount(objectVolume);

            rayfireShatter.voronoi.amount = fragmentsAmount;

            rayfireShatter.material.iMat = uniqueObjects[obj] != null ? uniqueObjects[obj] : defaultInsideMaterial;

            rayfireShatter.advanced.decompose = false;
            rayfireShatter.advanced.combineChildren = true;

            GameObject fragmentsParent = new GameObject($"{obj.name}_Fragmented");
            fragmentsParent.tag = RuntimeConstants.Tags.FRAGMENTS_ROOT;

            LayerMask fragmentsLayer = LayerMask.NameToLayer(RuntimeConstants.Layers.STRUCTURE_FRAGMENTED_LAYER);

            rayfireShatter.Fragment(fragmentsParent.transform, fragmentsLayer);

            fragmentsParent.transform.SetParent(obj.transform);

            fragmentsParent.transform.localPosition = Vector3.zero;
            fragmentsParent.layer = fragmentsLayer;

            fragmentsParent.SetActive(false);

            string saveName = rayfireShatter.gameObject.name + rayfireShatter.export.suffix;

            SaveFragmentMeshes(rayfireShatter, saveName);

            AddColliders(rayfireShatter);

            AddFragmentRigid(fragmentsParent.transform);

            DestroyImmediate(rayfireShatter);
        }


        private void AddFragmentRigid(Transform fragmentsParent)
        {
            foreach (Transform fragment in fragmentsParent)
            {
                RayfireRigid rayfireRigid = fragment.AddComponent<RayfireRigid>();
                rayfireRigid.initialization = RayfireRigid.InitType.AtStart;
                rayfireRigid.simulationType = SimType.Inactive;
                rayfireRigid.fading.onDemolition = false;
                rayfireRigid.fading.onActivation = true;
                rayfireRigid.fading.fadeType = FadeType.ScaleDown;
                rayfireRigid.fading.fadeTime = 1;
                rayfireRigid.fading.lifeTime = 2;
                rayfireRigid.fading.lifeVariation = 0.5f;
                rayfireRigid.fading.sizeFilter = 0;
                rayfireRigid.fading.shardAmount = 0;
            }
        }


        private static void SaveFragmentMeshes(RayfireShatter rayfireShatter, string saveName)
        {
            RFMeshAsset.SaveFragments(rayfireShatter, @"Assets\Visuals\FragmentMeshes\" + saveName + ".asset");
        }


        private void AddColliders(RayfireShatter rayfireShatter)
        {
            if (rayfireShatter.fragmentsLast.Count > 0)
            {
                foreach (var frag in rayfireShatter.fragmentsLast)
                {
                    if (!frag.TryGetComponent(out MeshCollider meshCollider))
                    {
                        meshCollider = frag.AddComponent<MeshCollider>();
                        meshCollider.convex = true;
                    }
                }
            }
        }


        private static bool HasStructureComponents(GameObject child)
        {
            return child.GetComponent<DestructibleObject>() &&
                   child.GetComponent<StructureObject>();
        }


        private float CalculateObjectVolume(GameObject obj)
        {
            Renderer[] objectRenderers = obj.GetComponentsInChildren<Renderer>();
            float objectVolume = 0;

            foreach (Renderer objectRenderer in objectRenderers)
            {
                Bounds bounds = objectRenderer.bounds;
                Vector3 size = bounds.size;

                objectVolume += size.x * size.y * size.z;
            }

            return objectVolume;
        }


        private int CalculateFragmentsAmount(float objectVolume)
        {
            if (objectVolume > maxObjectVolume)
            {
                return maxObjectFragments;
            }

            if (objectVolume < minObjectVolume)
            {
                return minObjectFragments;
            }

            float t = Mathf.InverseLerp(minObjectVolume, maxObjectVolume, objectVolume);

            int fragmentsAmount = (int)Mathf.Lerp(minObjectFragments, maxObjectFragments, t);

            return fragmentsAmount;
        }


        private float CalculateBombPower(float objectVolume)
        {
            if (objectVolume > maxObjectVolume)
            {
                return maxBombPower;
            }

            if (objectVolume < minObjectVolume)
            {
                return minBombPower;
            }

            float t = Mathf.InverseLerp(minObjectVolume, maxObjectVolume, objectVolume);

            float bombPower = Mathf.Lerp(minObjectFragments, maxObjectFragments, t);

            return bombPower;
        }


#endif
    }
}
