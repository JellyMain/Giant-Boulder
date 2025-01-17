using System;
using RayFire;
using Sirenix.OdinInspector;
using UnityEngine;


namespace Structures
{
    public abstract class StructureBase: MonoBehaviour
    {
        [SerializeField] private GameObject fragmentsRoot;
        private MeshRenderer meshRenderer;
        private RayfireBomb rayfireBomb;
        private Collider col;
        public event Action OnBuildingDestroyed;
        
        
        private void Awake()
        {
            rayfireBomb = GetComponent<RayfireBomb>();
            col = GetComponent<Collider>();
            meshRenderer = GetComponent<MeshRenderer>();
        }

      
        public void Destroy()
        {
            meshRenderer.enabled = false;
            col.enabled = false;

            fragmentsRoot.SetActive(true);
            rayfireBomb.Explode(0);
            
            OnBuildingDestroyed?.Invoke();
        }
    }
}