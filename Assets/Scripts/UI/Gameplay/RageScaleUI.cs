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
        [SerializeField] private RectTransform rageScaleEndPosition;
        [SerializeField] private RectTransform rageScaleUI;
        private StageMultipliersUI currentStageMultipliersUI;
        private RageScale rageScale;
        private RageScaleAnimations rageScaleAnimations;


        [Inject]
        private void Construct(RageScale rageScale, StaticDataService staticDataService)
        {
            this.rageScale = rageScale;
            rageScaleAnimations = staticDataService.AnimationsConfig.rageScaleAnimations;
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
            AnimateAppear();
        }


        private void AnimateAppear()
        {
            rageScaleUI.DOMove(rageScaleEndPosition.position, rageScaleAnimations.scaleAppearTime)
                .SetDelay(rageScaleAnimations.scaleAppearDelay).SetEase(Ease.OutQuart);
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
                rageScaleAnimations.scaleAnimationTime).SetEase(Ease.OutElastic);
        }
    }
}
