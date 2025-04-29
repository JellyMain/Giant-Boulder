using System;
using DG.Tweening;
using Factories;
using Player;
using StaticData.Data;
using StaticData.Services;
using UnityEngine;
using Zenject;


namespace GameLoop
{
    public class RageScale : MonoBehaviour
    {
        public int ScaleScore { get; private set; }
        private bool isLowerMultiplierActivated;
        private bool isHigherMultiplierActivated;
        public bool IsAbilityActivated { get; private set; }
        public int CurrentStage { get; private set; } = 1;
        private int currentMultiplier = 1;
        private RageScaleMultiplier lowerMultiplier;
        private RageScaleMultiplier higherMultiplier;
        private PlayerFactory playerFactory;
        private GameConfig gameConfig;
        private ObjectsDestroyer objectsDestroyer;
        public event ScoreCollectedHandler OnScoreCollected;
        public event Action OnScoreChanged;
        public event Action OnSuperBoulderActivated;
        public event Action OnSuperBoulderDeactivated;
        public event Action OnLowerMultiplierActivated;
        public event Action OnHigherMultiplierActivated;

        public delegate void ScoreCollectedHandler(int score, int multiplier, Vector3 objectWorldPosition);


        [Inject]
        private void Construct(PlayerFactory playerFactory, StaticDataService staticDataService)
        {
            this.playerFactory = playerFactory;
            gameConfig = staticDataService.GameConfig;
        }


        private void OnEnable()
        {
            playerFactory.OnPlayerCreated += SetAndSubscribeOnObjectDestroyer;
        }


        private void Start()
        {
            SetMultipliers();
        }


        private void SetMultipliers()
        {
            if (gameConfig.rageScaleStageMultiplierPointsMap.TryGetValue(CurrentStage,
                    out RageScaleStageMultipliersPair map))
            {
                lowerMultiplier = map.lowerMultiplier;
                higherMultiplier = map.higherMultiplier;

                isLowerMultiplierActivated = false;
                isHigherMultiplierActivated = false;
            }
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

            if (!IsAbilityActivated)
            {
                OnScoreChanged?.Invoke();

                if (!isLowerMultiplierActivated)
                {
                    if (GetNormalizedScaleScore() >= lowerMultiplier.normalizedPointOnScale)
                    {
                        isLowerMultiplierActivated = true;
                        OnLowerMultiplierActivated?.Invoke();
                        currentMultiplier = lowerMultiplier.multiplier;
                    }
                }

                if (!isHigherMultiplierActivated)
                {
                    if (GetNormalizedScaleScore() >= higherMultiplier.normalizedPointOnScale)
                    {
                        isHigherMultiplierActivated = true;
                        OnHigherMultiplierActivated?.Invoke();
                        currentMultiplier = higherMultiplier.multiplier;
                    }
                }

                if (ScaleScore >= gameConfig.maxScaleScore)
                {
                    ScaleScore = gameConfig.maxScaleScore;
                    CurrentStage++;
                    SetMultipliers();
                    ActivateSuperBoulder();
                }
            }
        }


        private void ActivateSuperBoulder()
        {
            IsAbilityActivated = true;
            OnSuperBoulderActivated?.Invoke();

            DOTween.To(() => ScaleScore, x => ScaleScore = x, 0, gameConfig.superBoulderTime)
                .OnComplete(() =>
                {
                    IsAbilityActivated = false;
                    OnSuperBoulderDeactivated?.Invoke();
                    SetMultipliers();
                });
        }


        public float GetNormalizedScaleScore()
        {
            return (float)ScaleScore / gameConfig.maxScaleScore;
        }
    }
}
