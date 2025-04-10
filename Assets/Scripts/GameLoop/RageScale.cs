using System;
using DG.Tweening;
using Factories;
using Player;
using UnityEngine;
using Zenject;


namespace GameLoop
{
    public class RageScale : MonoBehaviour
    {
        [SerializeField] private int maxScaleScore = 2000;
        [SerializeField] private float superBoulderTime = 5;
        public int ScaleScore { get; private set; }
        public bool IsActivated { get; private set; }
        private int currentMultiplier = 1;
        private PlayerFactory playerFactory;
        private ObjectsDestroyer objectsDestroyer;
        public event ScoreCollectedHandler OnScoreCollected;
        public event Action OnScoreChanged;
        public event Action OnSuperBoulderActivated;
        public event Action OnSuperBoulderDiactivated;

        public delegate void ScoreCollectedHandler(int score, int multiplier, Vector3 objectWorldPosition);


        [Inject]
        private void Construct(PlayerFactory playerFactory)
        {
            this.playerFactory = playerFactory;
        }


        private void OnEnable()
        {
            playerFactory.OnPlayerCreated += SetAndSubscribeOnObjectDestroyer;
        }


        private void OnDisable()
        {
            playerFactory.OnPlayerCreated -= SetAndSubscribeOnObjectDestroyer;
            objectsDestroyer.OnObjectScoreCollected -= AddObjectScore;
        }


        private void SetAndSubscribeOnObjectDestroyer(GameObject player)
        {
            objectsDestroyer = player.GetComponent<ObjectsDestroyer>();
            objectsDestroyer.OnObjectScoreCollected += AddObjectScore;
        }


        private void AddObjectScore(int score, Vector3 objectWorldPosition)
        {
            ScaleScore += score;
            OnScoreCollected?.Invoke(score, currentMultiplier, objectWorldPosition);

            if (!IsActivated)
            {
                OnScoreChanged?.Invoke();

                if (ScaleScore >= maxScaleScore)
                {
                    ScaleScore = maxScaleScore;
                    ActivateSuperBoulder();
                }
            }
        }


        private void ActivateSuperBoulder()
        {
            IsActivated = true;
            OnSuperBoulderActivated?.Invoke();

            DOTween.To(() => ScaleScore, x => ScaleScore = x, 0, superBoulderTime)
                .OnComplete(() =>
                {
                    IsActivated = false;
                    OnSuperBoulderDiactivated?.Invoke();
                });
        }


        public float GetNormalizedScaleScore()
        {
            return (float)ScaleScore / maxScaleScore;
        }
    }
}
