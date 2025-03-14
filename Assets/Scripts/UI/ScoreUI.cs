using System;
using DataTrackers;
using DG.Tweening;
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
        private ObjectsDestroyer objectsDestroyer;
        private Camera uiCamera;
        private DOTweenTMPAnimator scoreTextAnimator;


        [Inject]
        private void Construct(ScoreTracker scoreTracker)
        {
            this.scoreTracker = scoreTracker;
        }


        public void Construct(ObjectsDestroyer objectsDestroyer, Camera uiCamera)
        {
            this.objectsDestroyer = objectsDestroyer;
            this.uiCamera = uiCamera;
        }


        private void OnDisable()
        {
            objectsDestroyer.OnScoreCollected -= UpdateScoreText;
            objectsDestroyer.OnScoreCollected -= AnimateSpawnedScoreText;
        }


        private void Start()
        {
            objectsDestroyer.OnScoreCollected += UpdateScoreText;
            objectsDestroyer.OnScoreCollected += AnimateSpawnedScoreText;

            scoreTextAnimator = new DOTweenTMPAnimator(scoreText);
            UpdateScoreText(0, Vector3.zero);
        }


        private void AnimateSpawnedScoreText(int scoreValue, Vector3 structureWorldPosition)
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
        }


        private void UpdateScoreText(int _, Vector3 __)
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