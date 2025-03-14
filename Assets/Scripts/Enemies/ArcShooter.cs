using Sirenix.OdinInspector;
using UnityEngine;


namespace Enemies
{
    public class ArcShooter : MonoBehaviour
    {
        [SerializeField] private ArcBullet bulletPrefab;
        [SerializeField] private float bulletSpeed;
        [SerializeField] private float arcHeightMultiplier = 5;
        [SerializeField] private PlayerDetector playerDetector;
        [SerializeField] private LayerMask groundLayer;


        [Button]
        public void Shoot()
        {
            Vector3 target = playerDetector.Player.position;
            Vector3 groundedTarget = target;

            if (Physics.Raycast(target, Vector3.down, out RaycastHit hit, 1000))
            {
                groundedTarget = hit.point;
            }

            ArcBullet spawnedBullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
            spawnedBullet.Construct(groundedTarget, bulletSpeed, arcHeightMultiplier);
        }
    }
}
