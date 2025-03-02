using System;
using System.Collections;
using Cysharp.Threading.Tasks;
using UnityEngine;


namespace Coins
{
    public class CoinAnimator : MonoBehaviour
    {
        [SerializeField] private Coin coin;
        [SerializeField] private MeshRenderer meshRenderer;
        [SerializeField] private float rotationSpeed;
        [SerializeField] private float disappearTime = 3;
        [SerializeField] private float initialBlinkInterval = 0.5f;


        private void OnEnable()
        {
            coin.OnDisappearStarted += StartDisappearing;
        }


        private void OnDisable()
        {
            coin.OnDisappearStarted -= StartDisappearing;
        }


        private void Start()
        {
            Rotate().Forget();
        }


        private void StartDisappearing()
        {
            Disappear().Forget();
        }


        private async UniTaskVoid Disappear()
        {
            float elapsedTime = 0;

            while (elapsedTime <= disappearTime)
            {
                float blinkInterval = Mathf.Lerp(initialBlinkInterval, 0.01f, elapsedTime / disappearTime);

                meshRenderer.enabled = !meshRenderer.enabled;

                await UniTask.Delay((int)(blinkInterval * 1000), cancellationToken: this.GetCancellationTokenOnDestroy());

                elapsedTime += blinkInterval;
            }

            meshRenderer.enabled = false;

            Destroy(gameObject);
        }


        


        private async UniTaskVoid Rotate()
        {
            while (true)
            {
                float rotationAmount = rotationSpeed * Time.deltaTime;
                transform.Rotate(0, rotationAmount, 0);

                await UniTask.Yield(this.GetCancellationTokenOnDestroy());
            }
        }
    }
}
