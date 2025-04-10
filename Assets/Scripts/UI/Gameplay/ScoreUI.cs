using System;
using DataTrackers;
using DG.Tweening;
using GameLoop;
using Player;
using TMPro;
using UnityEngine;
using Zenject;
using Random = UnityEngine.Random;


namespace UI
{
    public class ScoreUI : MonoBehaviour
    {
        [SerializeField] private float maxScoreTextRotation = 45;
        [SerializeField] private float minScoreTextRotation = -45;
        [SerializeField] private float spawnedTextAppearTime = 0.3f;
        [SerializeField] private float spawnedTextDisappearTime = 0.3f;
        [SerializeField] private float scoreTextScalePunchScaleTime = 0.3f;
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private Canvas canvas;
        [SerializeField] private Transform animatedScoreTextParent;
        [SerializeField] private TMP_Text animatedScoreTextPrefab;
        private ScoreTracker scoreTracker;
        private Camera uiCamera;
        private RageScale rageScale;
        private DOTweenTMPAnimator scoreTextAnimator;


        [Inject]
        private void Construct(ScoreTracker scoreTracker)
        {
            this.scoreTracker = scoreTracker;
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

            float randomRotation = Random.Range(minScoreTextRotation, maxScoreTextRotation);

            TMP_Text spawnedScoreText = Instantiate(animatedScoreTextPrefab, uiCameraWorldPos,
                Quaternion.Euler(0, 0, randomRotation), animatedScoreTextParent);

            spawnedScoreText.text = scoreValue.ToString();

            Sequence sequence = DOTween.Sequence();


            sequence.Insert(0, spawnedScoreText.DOScale(2, spawnedTextAppearTime).From(0));
            sequence.Append(spawnedScoreText.DOScale(0, spawnedTextDisappearTime));
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

                    scoreTextAnimator.DOPunchCharScale(i, 0.3f, scoreTextScalePunchScaleTime);
                }
            }
        }
    }
}
