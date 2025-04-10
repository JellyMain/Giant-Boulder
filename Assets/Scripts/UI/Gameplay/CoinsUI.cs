using System;
using DataTrackers;
using DG.Tweening;
using Player;
using TMPro;
using UnityEngine;
using Zenject;


namespace UI
{
    public class CoinsUI : MonoBehaviour
    {
        [SerializeField] private GameObject uiCoinPrefab;
        [SerializeField] private Canvas canvas;
        [SerializeField] private TMP_Text coinsText;
        [SerializeField] private GameObject targetUICoin;
        [SerializeField] private Transform animationCoinsParent;
        private Camera uiCamera;
        private GameCurrencyTracker gameCurrencyTracker;


        [Inject]
        private void Construct(GameCurrencyTracker gameCurrencyTracker)
        {
            this.gameCurrencyTracker = gameCurrencyTracker;
        }


        public void Construct( Camera uiCamera)
        {
            this.uiCamera = uiCamera;
        }


        private void OnDisable()
        {
            gameCurrencyTracker.OnCoinAdded -= AnimateCollectedCoin;
        }


        private void Start()
        {
            gameCurrencyTracker.OnCoinAdded += AnimateCollectedCoin;
            UpdateCoinsAmount(gameCurrencyTracker.Coins);
        }


        private void AnimateCollectedCoin(int currentAmount ,Vector3 coinWorldPosition)
        {
            Vector3 mainCameraScreenPos = Camera.main.WorldToScreenPoint(coinWorldPosition);

            float canvasPlaneDistance = canvas.planeDistance;
            Vector3 uiCameraWorldPos = uiCamera.ScreenToWorldPoint(
                new Vector3(mainCameraScreenPos.x, mainCameraScreenPos.y, canvasPlaneDistance)
            );

            GameObject spawnedUICoin =
                Instantiate(uiCoinPrefab, uiCameraWorldPos, Quaternion.identity, animationCoinsParent);
            
            
            Tween moveTween = spawnedUICoin.transform.DOMove(targetUICoin.transform.position, 1).SetEase(Ease.InBack);
            Tween scaleTween = spawnedUICoin.transform.DOScale(200 / 1.25f, 1).SetEase(Ease.InBack);

            Sequence sequence = DOTween.Sequence();
            sequence.Append(moveTween);
            sequence.Insert(0, scaleTween);
            sequence.OnComplete(() =>
            {
                Destroy(spawnedUICoin);
                UpdateCoinsAmount(currentAmount);
            });

            sequence.Play();
        }
        


        private void UpdateCoinsAmount(int coinsAmount)
        {
            coinsText.text = coinsAmount.ToString();
        }
    }
}