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
        private CoinCollector coinCollector;
        private CurrencyTracker currencyTracker;


        [Inject]
        private void Construct(CurrencyTracker currencyTracker)
        {
            this.currencyTracker = currencyTracker;
        }


        public void Construct(CoinCollector coinCollector, Camera uiCamera)
        {
            this.uiCamera = uiCamera;
            this.coinCollector = coinCollector;
        }


        private void OnDisable()
        {
            coinCollector.OnCoinCollected -= AnimateCollectedCoin;
        }


        private void Start()
        {
            coinCollector.OnCoinCollected += AnimateCollectedCoin;
            UpdateCoinsAmount(currencyTracker.Coins);
        }


        private void AnimateCollectedCoin(Vector3 coinWorldPosition)
        {
            Vector3 mainCameraScreenPos = Camera.main.WorldToScreenPoint(coinWorldPosition);

            float canvasPlaneDistance = canvas.planeDistance;
            Vector3 uiCameraWorldPos = uiCamera.ScreenToWorldPoint(
                new Vector3(mainCameraScreenPos.x, mainCameraScreenPos.y, canvasPlaneDistance)
            );

            GameObject spawnedUICoin =
                Instantiate(uiCoinPrefab, uiCameraWorldPos, Quaternion.identity, animationCoinsParent);

            Sequence sequence = CreateCoinAnimationSequence(spawnedUICoin);

            sequence.Play();
        }


        private Sequence CreateCoinAnimationSequence(GameObject spawnedUICoin)
        {
            Tween moveTween = spawnedUICoin.transform.DOMove(targetUICoin.transform.position, 1).SetEase(Ease.InBack);
            Tween scaleTween = spawnedUICoin.transform.DOScale(200 / 1.25f, 1).SetEase(Ease.InBack);

            Sequence sequence = DOTween.Sequence();
            sequence.Append(moveTween);
            sequence.Insert(0, scaleTween);
            sequence.OnComplete(() =>
            {
                Destroy(spawnedUICoin);
                currencyTracker.AddCoin();
                UpdateCoinsAmount(currencyTracker.Coins);
            });

            return sequence;
        }



        private void UpdateCoinsAmount(int coinsAmount)
        {
            coinsText.text = coinsAmount.ToString();
        }
    }
}
