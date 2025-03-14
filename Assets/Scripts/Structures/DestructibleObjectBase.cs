using System;
using Const;
using RayFire;
using Sirenix.OdinInspector;
using UnityEngine;
using Utils;


namespace Structures
{
    public abstract class DestructibleObjectBase : MonoBehaviour
    {
        [SerializeField] private ParticleSystem destroyParticlesPrefab;
        [SerializeField] private GameObject fragmentsRoot;
        [SerializeField] private int scoreValue = 100;
        private MeshRenderer meshRenderer;
        private RayfireBomb rayfireBomb;
        private Collider col;
        private Vector3 objectCenter;
        public int ScoreValue => scoreValue;
        public event Action OnBuildingDestroyed;



        private void Awake()
        {
            rayfireBomb = GetComponent<RayfireBomb>();
            col = GetComponent<Collider>();
            meshRenderer = GetComponent<MeshRenderer>();
        }


        private void Start()
        {
            objectCenter = RuntimeMeshUtility.GetMeshCenter(transform, col);
        }


        public void FindFragmentsRoot()
        {
            foreach (Transform child in transform)
            {
                if (child.CompareTag("FragmentsRoot"))
                {
                    fragmentsRoot = child.gameObject;
                }
            }
        }


        public void Destroy()
        {
            meshRenderer.enabled = false;
            col.enabled = false;

            fragmentsRoot.SetActive(true);
            rayfireBomb.Explode(0);

            Instantiate(destroyParticlesPrefab, objectCenter, Quaternion.identity);

            OnBuildingDestroyed?.Invoke();
        }
    }
}
