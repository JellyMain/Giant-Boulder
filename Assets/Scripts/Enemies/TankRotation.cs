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
            Vector3 direction = player.transform.position - transform.position;

            Quaternion lookDirection = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));

            Quaternion nextFrameTurretRotation = Quaternion.Slerp(turret.transform.rotation, lookDirection,
                Time.deltaTime * turretRotationSpeed);


            Quaternion nextFrameBodyRotation = Quaternion.Slerp(transform.rotation, lookDirection,
                Time.deltaTime * bodyRotationSpeed);


            float nextFrameYNormalized =
                Mathf.DeltaAngle(transform.eulerAngles.y, lookDirection.eulerAngles.y);


            if (nextFrameYNormalized <= maxTurretRotation && nextFrameYNormalized >= minTurretRotation)
            {
                turret.transform.rotation = nextFrameTurretRotation;
                transform.rotation = nextFrameBodyRotation;
            }
            else
            {
                transform.rotation = nextFrameBodyRotation;
            }
        }





        
    }
}
