using System;
using RayFire;
using Structures;
using UnityEngine;


namespace Player
{
    public class PlayerDestroyer : MonoBehaviour
    {
        [SerializeField] private GameObject fragmentsRoot;
        private RayfireBomb rayfireBomb;
        private Rigidbody rb;
        private MeshRenderer meshRenderer;
        private Collider col;
        private bool isDestroyed;


        private void Awake()
        {
            rayfireBomb = GetComponent<RayfireBomb>();
            rb = GetComponent<Rigidbody>();
            meshRenderer = GetComponent<MeshRenderer>();
            col = GetComponent<Collider>();
        }

        
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Destroy();
            }
        }


        private void OnCollisionEnter(Collision other)
        {
            if (other.gameObject.TryGetComponent(out StructureBase structure))
            {
                structure.Destroy();
            }
        }


        private void Destroy()
        {
            if (!isDestroyed)
            {
                isDestroyed = true;

                rb.isKinematic = true;
                meshRenderer.enabled = false;
                col.enabled = false;
                
                fragmentsRoot.SetActive(true);
                rayfireBomb.Explode(0);
            }
        }
    }
}