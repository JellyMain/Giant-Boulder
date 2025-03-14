using System;
using UnityEngine;


namespace Enemies
{
    public class PlayerDetector : MonoBehaviour
    {
        [SerializeField] private float playerDetectionRadius = 20;
        [SerializeField] private LayerMask playerLayer;
        public Transform Player { get; private set; }
        private readonly Collider[] playerColliderBuffer = new Collider[1];


        private void FixedUpdate()
        {
            int player = Physics.OverlapSphereNonAlloc(transform.position, playerDetectionRadius, playerColliderBuffer,
                playerLayer);

            if (player != 0)
            {
                Player = playerColliderBuffer[0].transform;
            }
            else
            {
                Player = null;
            }
        }
        
        
        
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, playerDetectionRadius);
        }
    }
}
