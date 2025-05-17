using System;
using System.Collections.Generic;
using RayFire;
using Sirenix.OdinInspector;
using UnityEngine;


namespace Player
{
    public class PlayerHealth : MonoBehaviour
    {
        [SerializeField] private GameObject fragmentsRoot;
        [SerializeField] private int playerHealth = 100;
        private bool isDestroyed;
        private Rigidbody rb;
        private MeshRenderer meshRenderer;
        private Collider col;
        private RayfireBomb rayfireBomb;


        private void Awake()
        {
            rayfireBomb = GetComponent<RayfireBomb>();
            rb = GetComponent<Rigidbody>();
            meshRenderer = GetComponent<MeshRenderer>();
            col = GetComponent<Collider>();
        }
        

        public void TakeDamage(int damage)
        {
            playerHealth -= damage;

            if (playerHealth <= 0)
            {
                playerHealth = 0;
                Die();
            }
        }


        private void Die()
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