using System;
using UnityEngine;


namespace Enemies
{
    public class ArcBullet : MonoBehaviour
    {
        [SerializeField] private AnimationCurve animationCurve;
        private Vector3 target;
        private float moveSpeed;
        private float arcHeightMultiplier;
        private Vector3 startingPoint;
        private Vector3 previousPosition;
        private float t;


        public void Construct(Vector3 target, float moveSpeed, float arcHeightMultiplier)
        {
            this.target = target;
            this.moveSpeed = moveSpeed;
            this.arcHeightMultiplier = arcHeightMultiplier;
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
    }
}
