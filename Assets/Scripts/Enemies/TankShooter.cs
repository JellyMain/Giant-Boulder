using System;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using Sounds;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Zenject;


namespace Enemies
{
    public class TankShooter : MonoBehaviour
    {
        [SerializeField, Title("Components")] private ArcBullet bulletPrefab;
        [SerializeField] private Transform tankBarrel;
        [SerializeField] private ParticleSystem shotParticlesPrefab;
        [SerializeField] private DecalProjector targetPointPrefab;
        [SerializeField] private PlayerDetector playerDetector;

        [SerializeField, Title("Shooting Settings")]
        private float aimTime = 5;

        [SerializeField] private float reloadTime;
        [SerializeField] private float minRotationToAim = -35;
        [SerializeField] private float maxRotationToAim = 35;

        [SerializeField, Title("Bullet Settings")]
        private float arcHeightMultiplier = 5;

        [SerializeField] private float bulletSpeed;
        [SerializeField] private float targetPointOffset = 5;
        private SoundPlayer soundPlayer;
        private bool canShoot = true;
        private bool aimingStarted;
        

        [Inject]
        private void Construct(SoundPlayer soundPlayer)
        {
            this.soundPlayer = soundPlayer;
        }
        

        private void Update()
        {
            if (playerDetector.InMovementMinDistance && canShoot &&
                playerDetector.ShortestAngleToTargetY <= maxRotationToAim &&
                playerDetector.ShortestAngleToTargetY >= minRotationToAim)
            {
                AimForPlayer().Forget();
            }
        }


        private async UniTaskVoid UpdateShootCooldown()
        {
            float elapsedTime = 0;

            while (elapsedTime < reloadTime)
            {
                elapsedTime += Time.deltaTime;
                await UniTask.Yield();
            }

            canShoot = true;
        }


        private async UniTaskVoid AimForPlayer()
        {
            canShoot = false;

            Vector3 target = playerDetector.Player.position;
            DecalProjector spawnedDecalProjector = Instantiate(targetPointPrefab,
                new Vector3(target.x, target.y + targetPointOffset, target.z), Quaternion.Euler(90, 0, 0));

            float elapsedTime = 0;

            while (elapsedTime < aimTime)
            {
                target = playerDetector.Player.position;
                spawnedDecalProjector.transform.position =
                    new Vector3(target.x, target.y + targetPointOffset, target.z);
                elapsedTime += Time.deltaTime;

                if (!playerDetector.InAimingMinDistance || playerDetector.ShortestAngleToTargetY >= maxRotationToAim ||
                    playerDetector.ShortestAngleToTargetY <= minRotationToAim)
                {
                    CancelAiming(spawnedDecalProjector);
                    return;
                }

                await UniTask.Yield();
            }

            Shoot();
            Destroy(spawnedDecalProjector.gameObject);
        }



        private void CancelAiming(DecalProjector decalProjectorToDestroy)
        {
            canShoot = true;
            Destroy(decalProjectorToDestroy.gameObject);
        }


        private void Shoot()
        {
            Vector3 target = playerDetector.Player.position;
            Vector3 groundedTarget = target;

            if (Physics.Raycast(target, Vector3.down, out RaycastHit hit, 1000))
            {
                groundedTarget = hit.point;
            }

            Instantiate(shotParticlesPrefab, tankBarrel.position, tankBarrel.rotation);

            ArcBullet spawnedBullet = Instantiate(bulletPrefab, tankBarrel.position, Quaternion.identity);
            spawnedBullet.Construct(groundedTarget, bulletSpeed, arcHeightMultiplier, soundPlayer);

            UpdateShootCooldown().Forget();
        }
    }
}
