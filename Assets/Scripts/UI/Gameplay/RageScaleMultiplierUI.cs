using System;
using DG.Tweening;
using StaticData.Data;
using StaticData.Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;


namespace UI.Gameplay
{
    public class RageScaleMultiplierUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text multiplierText;
        [SerializeField] private Image multiplierImage;
        private RageScaleAnimations rageScaleAnimations;



        [Inject]
        private void Construct(StaticDataService staticDataService)
        {
            rageScaleAnimations = staticDataService.AnimationsConfig.rageScaleAnimations;
        }


        public void ActivateAnimation()
        {
            Sequence sequence = DOTween.Sequence();

            sequence.Append(multiplierText.DOScale(rageScaleAnimations.mlTextScale,
                rageScaleAnimations.mlTextScaleTime).SetEase(Ease.OutElastic));

            sequence.Insert(0, multiplierText.DOColor(Color.red, rageScaleAnimations.mlColorTime));

            sequence.Append(multiplierText.DOFade(0, rageScaleAnimations.mlTextFadeTime));
            sequence.Append(transform.DOScale(0, rageScaleAnimations.mlObjectDisappearTime));


        }
    }
}
