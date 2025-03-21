using System;
using Sirenix.OdinInspector;
using UnityEngine;


namespace Enemies
{
    public class PlayerDetector : MonoBehaviour
    {
        [SerializeField,
         ValidateInput(nameof(ValidateMoveMinDistance), "MoveMinDistance must be smaller than AimMinDistance!")]
        private float moveMinDistance = 40;

        [SerializeField] private float aimMinDistance = 60;
        [SerializeField] private float playerDetectionRadius = 20;
        [SerializeField] private LayerMask playerLayer;
        public Transform Player { get; private set; }
        public bool InMovementMinDistance { get; private set; }
        public bool InAimingMinDistance { get; private set; }
        public Vector3 Direction { get; private set; }
        public Quaternion LookRotation { get; private set; }
        public float ShortestAngleToTargetY { get; private set; }
        private readonly Collider[] playerColliderBuffer = new Collider[1];


        private void FixedUpdate()
        {
            int player = Physics.OverlapSphereNonAlloc(transform.position, playerDetectionRadius, playerColliderBuffer,
                playerLayer);

            if (player != 0)
            {
                Player = playerColliderBuffer[0].transform;

                float distance = Vector3.Distance(transform.position, Player.position);

                Direction = Player.position - transform.position;

                LookRotation =
                    Quaternion.LookRotation(new Vector3(Direction.x, 0, Direction.z));

                ShortestAngleToTargetY =
                    Mathf.DeltaAngle(transform.eulerAngles.y, LookRotation.eulerAngles.y);

                InMovementMinDistance = distance < moveMinDistance;
                InAimingMinDistance = distance < aimMinDistance;
            }
            else
            {
                Player = null;
            }
        }


        private bool ValidateMoveMinDistance()
        {
            return moveMinDistance < aimMinDistance;
        }


        private void OnDrawGizmos()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, playerDetectionRadius);
        }
    }
}
