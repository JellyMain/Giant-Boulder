using System;
using Player;
using Sounds;
using UnityEngine;


namespace Enemies
{
    public class ArcBullet : MonoBehaviour
    {
        [SerializeField] private AnimationCurve animationCurve;
        [SerializeField] private ParticleSystem destroyParticlesPrefab;
        [SerializeField] private float explosionRadius = 5;
        [SerializeField] private LayerMask playerLayer;
        [SerializeField] private int damage = 50;
        private readonly Collider[] playerColliderBuffer = new Collider[1];
        private SoundPlayer soundPlayer;
        private Vector3 target;
        private float moveSpeed;
        private float arcHeightMultiplier;
        private Vector3 startingPoint;
        private Vector3 previousPosition;
        private float t;


        public void Construct(Vector3 target, float moveSpeed, float arcHeightMultiplier, SoundPlayer soundPlayer)
        {
            this.target = target;
            this.moveSpeed = moveSpeed;
            this.arcHeightMultiplier = arcHeightMultiplier;
            this.soundPlayer = soundPlayer;
        }


        private void Start()
        {
            startingPoint = transform.position;
            previousPosition = transform.position;
        }


        private void Update()
        {
            Move();
            Rotate();
        }


        private void Move()
        {
            if (t <= 1)
            {
                float bulletHeight = animationCurve.Evaluate(t) * arcHeightMultiplier;
                Vector3 bulletHorizontalPosition = Vector3.Lerp(startingPoint, target, t);
                Vector3 bulletPosition = new Vector3(bulletHorizontalPosition.x,
                    bulletHorizontalPosition.y + bulletHeight,
                    bulletHorizontalPosition.z);
                transform.position = bulletPosition;

                t += Time.deltaTime * moveSpeed;
            }
            else
            {
                Explode();
            }
        }


        private void Rotate()
        {
            Vector3 directionNormalized = (transform.position - previousPosition).normalized;

            if (directionNormalized != Vector3.zero)
            {
                transform.up = directionNormalized;
            }

            previousPosition = transform.position;
        }


        private void Explode()
        {
            Instantiate(destroyParticlesPrefab, transform.position, Quaternion.identity);
            soundPlayer.PlayMissileExplosionSound(transform.position);

            if (Physics.OverlapSphereNonAlloc(transform.position, explosionRadius, playerColliderBuffer, playerLayer) !=
                0)
            {
                PlayerHealth playerHealth = playerColliderBuffer[0].GetComponent<PlayerHealth>();
                playerHealth.TakeDamage(damage);
            }

            Destroy(gameObject);
        }


        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, explosionRadius);
        }
    }
}
