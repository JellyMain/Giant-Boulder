using System;
using UnityEngine;
using UnityEngine.AI;


namespace Enemies
{
    public class TankRotation : MonoBehaviour
    {
        [SerializeField] private GameObject turret;
        [SerializeField] private PlayerDetector playerDetector;
        [SerializeField] private float turretRotationSpeed = 1;
        [SerializeField] private float bodyRotationSpeed = 0.2f;
        [SerializeField] private float maxTurretRotation = 35f;
        [SerializeField] private float minTurretRotation = -35f;



        private void Update()
        {
            if (playerDetector.Player != null)
            {
                Rotate(playerDetector.Player);
            }
        }



        private void Rotate(Transform player)
        {
            Quaternion nextFrameTurretRotation = Quaternion.Slerp(turret.transform.rotation,
                playerDetector.LookRotation, Time.deltaTime * turretRotationSpeed);


            Quaternion nextFrameBodyRotation = Quaternion.Slerp(transform.rotation, playerDetector.LookRotation,
                Time.deltaTime * bodyRotationSpeed);


            if (playerDetector.ShortestAngleToTargetY <= maxTurretRotation &&
                playerDetector.ShortestAngleToTargetY >= minTurretRotation)
            {
                turret.transform.rotation = nextFrameTurretRotation;
            }

            transform.rotation = nextFrameBodyRotation;
        }
    }
}
