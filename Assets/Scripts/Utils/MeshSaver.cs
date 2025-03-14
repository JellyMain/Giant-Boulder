using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;


namespace Utils
{
    public class MeshSaver : MonoBehaviour
    {
        [SerializeField] private List<MeshFilter> meshFilters;
        private int counter;

#if UNITY_EDITOR


        [Button]
        private void SaveMeshes()
        {
            foreach (MeshFilter meshFilter in meshFilters)
            {
                AssetDatabase.CreateAsset(meshFilter.sharedMesh,
                    @"Assets\Visuals\SavedMeshes\" + $"MountainMesh{counter}" + ".asset");
                counter++;
            }
        }


#endif
    }
}