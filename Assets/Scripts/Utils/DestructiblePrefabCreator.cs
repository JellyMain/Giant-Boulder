using RayFire;
using RayFireEditor;
using Sirenix.OdinInspector;
using UnityEngine;


namespace Utils
{
    public class DestructiblePrefabCreator : MonoBehaviour
    {
        [SerializeField] private Material insideMaterial;
        private GameObject[] childrenObjects;     
        


        
        [Button]
        private void FindChildrenObjects()
        {
            
        }
        
        

        [Button]
        private void ShatterStructure()
        {
            foreach (GameObject child in childrenObjects)
            {
                ShatterObject(child);
            }
            
        }


        private void ShatterObject(GameObject obj)
        {
            RayfireShatter rayfireShatter = obj.AddComponent<RayfireShatter>();

            rayfireShatter.voronoi.amount = 20;

            rayfireShatter.material.iMat = insideMaterial;
            rayfireShatter.advanced.decompose = false;

            GameObject fragmentsParent = new GameObject($"{obj.name}_Fragmented");

            rayfireShatter.Fragment(fragmentsParent.transform);
            
            fragmentsParent.transform.SetParent(obj.transform);
            
            fragmentsParent.transform.localPosition = Vector3.zero;
            
            string saveName = rayfireShatter.gameObject.name + rayfireShatter.export.suffix;

            SaveFragmentMeshes(rayfireShatter, saveName);
            
            AddColliders(rayfireShatter);
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
    }
}
