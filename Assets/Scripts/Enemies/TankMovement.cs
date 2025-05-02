using System;
using UnityEngine;
using UnityEngine.AI;


namespace Enemies
{
    public class TankMovement : MonoBehaviour
    {
        [SerializeField] private NavMeshAgent navMeshAgent;
        [SerializeField] private PlayerDetector playerDetector;
        [SerializeField] private float moveSpeed = 4;
        [SerializeField] private float minRotationToMove = -35f;
        [SerializeField] private float maxRotationToMove = 35f;



        private void Update()
        {
            if (playerDetector.Player != null)
            {
                FollowPlayer();
            }
        }


        private void FollowPlayer()
        {
            if (playerDetector.ShortestAngleToTargetY <= maxRotationToMove &&
                playerDetector.ShortestAngleToTargetY >= minRotationToMove &&
                !playerDetector.InMovementMinDistance)
            {
                navMeshAgent.Move(transform.forward * (Time.deltaTime * moveSpeed));
            }
        }
    }
}
