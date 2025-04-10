using System;
using DG.Tweening;
using GameLoop;
using UnityEngine;
using UnityEngine.UI;
using Zenject;


namespace UI
{
    public class RageScaleUI : MonoBehaviour
    {
        [SerializeField] private Image scaleFiller;
        [SerializeField] private float scaleAnimationTime = 1;
        private RageScale rageScale;


        [Inject]
        private void Construct(RageScale rageScale)
        {
            this.rageScale = rageScale;
        }
        
        private void OnEnable()
        {
            rageScale.OnScoreChanged += UpdateScale;
        }


        private void OnDisable()
        {
            rageScale.OnScoreChanged -= UpdateScale;
        }


        private void Start()
        {
            scaleFiller.fillAmount = 0;
        }



        private void Update()
        {
            if (rageScale.IsActivated)
            {
                float normalizedScaleScore = rageScale.GetNormalizedScaleScore();
                scaleFiller.fillAmount = normalizedScaleScore;
            }
        }



        private void UpdateScale()
        {
            float normalizedScaleScore = rageScale.GetNormalizedScaleScore();
            DOTween.To(() => scaleFiller.fillAmount, x => scaleFiller.fillAmount = x, normalizedScaleScore,
                scaleAnimationTime).SetEase(Ease.OutElastic);
        }
        
        
    }
}
