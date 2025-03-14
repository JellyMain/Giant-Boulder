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
        [SerializeField] private float minDistanceToPlayer = 10;


        private void Update()
        {
            if (playerDetector.Player != null)
            {
                FollowPlayer(playerDetector.Player);
            }
        }


        private void FollowPlayer(Transform player)
        {
            float distance = Vector3.Distance(transform.position, player.position);
            Vector3 direction = player.transform.position - transform.position;

            Quaternion lookDirection = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));

            float nextFrameYNormalized =
                Mathf.DeltaAngle(transform.eulerAngles.y, lookDirection.eulerAngles.y);

            if (nextFrameYNormalized <= maxRotationToMove && nextFrameYNormalized >= minRotationToMove &&
                distance >= minDistanceToPlayer)
            {
                navMeshAgent.Move(transform.forward * (Time.deltaTime * moveSpeed));
            }
        }
    }
}