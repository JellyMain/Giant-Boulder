using System.Collections.Generic;
using DG.Tweening;
using GameLoop;
using Sirenix.OdinInspector;
using StaticData.Data;
using StaticData.Services;
using UnityEngine;
using UnityEngine.UI;
using Zenject;


namespace UI.Gameplay
{
    public class RageScaleUI : SerializedMonoBehaviour
    {
        [SerializeField] private Image scaleFiller;
        [SerializeField] private Dictionary<int, StageMultipliersUI> stageMultipliersMap;
        private StageMultipliersUI currentStageMultipliersUI;
        private RageScale rageScale;
        private AnimationsConfig animationsConfig;


        [Inject]
        private void Construct(RageScale rageScale, StaticDataService staticDataService)
        {
            this.rageScale = rageScale;
            animationsConfig = staticDataService.AnimationsConfig;
        }


        private void OnEnable()
        {
            rageScale.OnScoreChanged += UpdateScale;
            rageScale.OnLowerMultiplierActivated += ActivateLowerMultiplier;
            rageScale.OnHigherMultiplierActivated += ActivateHigherMultiplier;
            rageScale.OnSuperBoulderDeactivated += SetStageMultipliers;
        }


        private void OnDisable()
        {
            rageScale.OnScoreChanged -= UpdateScale;
            rageScale.OnLowerMultiplierActivated -= ActivateLowerMultiplier;
            rageScale.OnHigherMultiplierActivated -= ActivateHigherMultiplier;
            rageScale.OnSuperBoulderDeactivated -= SetStageMultipliers;
        }


        private void Start()
        {
            SetStageMultipliers();
            scaleFiller.fillAmount = 0;
        }


        private void SetStageMultipliers()
        {
            if (stageMultipliersMap.TryGetValue(rageScale.CurrentStage, out StageMultipliersUI stageMultipliersUI))
            {
                currentStageMultipliersUI = stageMultipliersUI;
                currentStageMultipliersUI.gameObject.SetActive(true);
            }
        }


        private void ActivateLowerMultiplier()
        {
            currentStageMultipliersUI.lowerMultiplierUI.ActivateAnimation();
        }


        private void ActivateHigherMultiplier()
        {
            currentStageMultipliersUI.higherMultiplierUI.ActivateAnimation();
        }


        private void Update()
        {
            if (rageScale.IsAbilityActivated)
            {
                float normalizedScaleScore = rageScale.GetNormalizedScaleScore();
                scaleFiller.fillAmount = normalizedScaleScore;
            }
        }


        private void UpdateScale()
        {
            float normalizedScaleScore = rageScale.GetNormalizedScaleScore();
            DOTween.To(() => scaleFiller.fillAmount, x => scaleFiller.fillAmount = x, normalizedScaleScore,
                animationsConfig.rageScaleAnimations.scaleAnimationTime).SetEase(Ease.OutElastic);
        }
    }
}
