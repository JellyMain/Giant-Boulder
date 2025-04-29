using DataTrackers;
using DG.Tweening;
using GameLoop;
using StaticData.Data;
using StaticData.Services;
using TMPro;
using UnityEngine;
using Zenject;
using Random = UnityEngine.Random;


namespace UI.Gameplay
{
    public class ScoreUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private Canvas canvas;
        [SerializeField] private Transform animatedScoreTextParent;
        [SerializeField] private TMP_Text animatedScoreTextPrefab;
        private ScoreTracker scoreTracker;
        private GameConfig gameConfig;
        private AnimationsConfig animationsConfig;
        private Camera uiCamera;
        private RageScale rageScale;
        private DOTweenTMPAnimator scoreTextAnimator;


        [Inject]
        private void Construct(ScoreTracker scoreTracker, StaticDataService staticDataService)
        {
            this.scoreTracker = scoreTracker;
            gameConfig = staticDataService.GameConfig;
            animationsConfig = staticDataService.AnimationsConfig;
        }


        public void Construct(Camera uiCamera)
        {
            this.uiCamera = uiCamera;
        }


        private void OnDisable()
        {
            scoreTracker.OnScoreAdded -= AnimateSpawnedObjectScoreText;
        }


        private void Start()
        {
            scoreTracker.OnScoreAdded += AnimateSpawnedObjectScoreText;

            scoreTextAnimator = new DOTweenTMPAnimator(scoreText);
            UpdateScoreText();
        }


        private void AnimateSpawnedObjectScoreText(int scoreValue, Vector3 structureWorldPosition)
        {
            Vector3 mainCameraScreenPos = Camera.main.WorldToScreenPoint(structureWorldPosition);

            float canvasPlaneDistance = canvas.planeDistance;
            Vector3 uiCameraWorldPos = uiCamera.ScreenToWorldPoint(
                new Vector3(mainCameraScreenPos.x, mainCameraScreenPos.y, canvasPlaneDistance)
            );

            float randomRotation = Random.Range(animationsConfig.scoreAnimations.minScoreTextRotation,
                animationsConfig.scoreAnimations.maxScoreTextRotation);

            TMP_Text spawnedScoreText = Instantiate(animatedScoreTextPrefab, uiCameraWorldPos,
                Quaternion.Euler(0, 0, randomRotation), animatedScoreTextParent);

            spawnedScoreText.text = scoreValue.ToString();
            float normalizedScoreValue = (float)scoreValue / gameConfig.maxScoreForObject;
            spawnedScoreText.color = animationsConfig.scoreAnimations.scoreGradient.Evaluate(normalizedScoreValue);

            Sequence sequence = DOTween.Sequence();

            float scaleValue = Mathf.Lerp(animationsConfig.scoreAnimations.scoreMinScale,
                animationsConfig.scoreAnimations.scoreMaxScale, normalizedScoreValue);
            float disappearTime = Mathf.Lerp(animationsConfig.scoreAnimations.scoreMinDisappearTime,
                animationsConfig.scoreAnimations.scoreMaxDisappearTime,
                normalizedScoreValue);

            sequence.Insert(0,
                spawnedScoreText.DOScale(scaleValue, animationsConfig.scoreAnimations.spawnedTextAppearTime).From(0));
            sequence.Append(spawnedScoreText.DOScale(0, disappearTime));
            sequence.OnComplete(() => { Destroy(spawnedScoreText.gameObject); });

            UpdateScoreText();
        }


        private void UpdateScoreText()
        {
            scoreText.text = scoreTracker.CurrentScore.ToString();

            AnimateScoreText();
        }


        private void AnimateScoreText()
        {
            if (!DOTween.IsTweening(scoreText))
            {
                for (int i = 0; i < scoreTextAnimator.textInfo.characterCount; i++)
                {
                    if (!scoreTextAnimator.textInfo.characterInfo[i].isVisible)
                    {
                        continue;
                    }

                    scoreTextAnimator.DOPunchCharScale(i, 0.3f,
                        animationsConfig.scoreAnimations.scoreTextScalePunchScaleTime);
                }
            }
        }
    }
}
