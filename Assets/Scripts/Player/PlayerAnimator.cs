using System;
using UnityEngine;


namespace Player
{
    public class PlayerAnimator : MonoBehaviour
    {
        [SerializeField] private GameObject fragmentsRoot;
        [SerializeField] private PlayerMovement playerMovement;
        [SerializeField] private MeshRenderer meshRenderer;
        [SerializeField] private Rigidbody rb;
        [SerializeField] private Collider collider;


        private void Start()
        {
            PlaySpawnAnimation();
        }


        private void PlaySpawnAnimation()
        {
            playerMovement.canMove = false;
            meshRenderer.enabled = false;
            rb.useGravity = false;
            collider.enabled = false;
            fragmentsRoot.SetActive(true);
        }


        public void RevertPlayableState()
        {
            playerMovement.canMove = true;
            meshRenderer.enabled = true;
            rb.useGravity = true;
            collider.enabled = true;
            fragmentsRoot.SetActive(false);
        }
    } 
}
