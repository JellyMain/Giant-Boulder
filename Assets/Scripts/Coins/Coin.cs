using System;
using Cysharp.Threading.Tasks;
using Sounds;
using UnityEngine;


namespace Coins
{
    public class Coin : MonoBehaviour
    {
        [SerializeField] private Rigidbody rb;
        [SerializeField] private ParticleSystem destroyParticlesPrefab;
        [SerializeField] private float disappearStartTime = 2;
        private SoundPlayer soundPlayer;
        public Rigidbody Rb => rb;
        public event Action OnDisappearStarted;

        
        public void Construct(SoundPlayer soundPlayer)
        {
            this.soundPlayer = soundPlayer;
        }
        

        private void Start()
        {
            StartTimer().Forget();
        }


        private async UniTaskVoid StartTimer()
        {
            float elapsedTime = 0;

            while (elapsedTime <= disappearStartTime)
            {
                elapsedTime += Time.deltaTime;
                await UniTask.Yield(this.GetCancellationTokenOnDestroy());
            }

            OnDisappearStarted?.Invoke();
        }


        private void OnCollisionEnter(Collision other)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("Ground"))
            {
                rb.isKinematic = true;
            }
        }
        
        
        public void Destroy()
        {
            soundPlayer.PlayCoinCollectedSound();
            Instantiate(destroyParticlesPrefab, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }

        
    }
}
