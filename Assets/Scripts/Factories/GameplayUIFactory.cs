using System.Collections.Generic;
using Assets;
using Coins;
using Const;
using Cysharp.Threading.Tasks;
using Infrastructure.Services;
using UI;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Zenject;


namespace Factories
{
    public class GameplayUIFactory
    {
        private readonly DiContainer diContainer;
        private readonly AssetProvider assetProvider;
        private Transform uiParent;


        public GameplayUIFactory(DiContainer diContainer, AssetProvider assetProvider)
        {
            this.diContainer = diContainer;
            this.assetProvider = assetProvider;
        }


        private void CreateUIParent()
        {
            if (uiParent == null)
            {
                uiParent = new GameObject("UI").transform;
            }
        }


        public async UniTask<PlayerControlsUI> CreatePlayerControlsUI()
        {
            CreateUIParent();
            GameObject playerControlsUIPrefab =
                await assetProvider.LoadAsset<GameObject>(RuntimeConstants.PrefabAddresses.PLAYER_CONTROLS_UI);

            return diContainer.InstantiatePrefab(playerControlsUIPrefab, uiParent).GetComponent<PlayerControlsUI>();
        }



        public async UniTask<GameObject> CreateScoreAndCurrencyUI(Camera uiCamera)
        {
            CreateUIParent();

            GameObject scoreAndCurrencyUIPrefab =
                await assetProvider.LoadAsset<GameObject>(RuntimeConstants.PrefabAddresses.SCORE_AND_CURRENCY_UI);

            GameObject scoreAndCurrencyUI = diContainer.InstantiatePrefab(scoreAndCurrencyUIPrefab, uiParent);

            Canvas canvas = scoreAndCurrencyUI.GetComponent<Canvas>();
            canvas.worldCamera = uiCamera;

            return scoreAndCurrencyUI;
        }


        public async UniTaskVoid CreateGameTimerUI()
        {
            CreateUIParent();

            GameObject gameTimerUIPrefab =
                await assetProvider.LoadAsset<GameObject>(RuntimeConstants.PrefabAddresses.GAME_TIMER_UI);

            diContainer.InstantiatePrefab(gameTimerUIPrefab, uiParent);
        }


        public async UniTaskVoid CreateGameOverWindow()
        {
            CreateUIParent();

            GameObject gameOverWindowUIPrefab =
                await assetProvider.LoadAsset<GameObject>(RuntimeConstants.PrefabAddresses.GAME_OVER_WINDOW_UI);

            diContainer.InstantiatePrefab(gameOverWindowUIPrefab, uiParent);
        }


        public async UniTaskVoid CreateRageScaleUI()
        {
            CreateUIParent();

            GameObject rageScaleUIPrefab =
                await assetProvider.LoadAsset<GameObject>(RuntimeConstants.PrefabAddresses.RAGE_SCALE_UI);

            diContainer.InstantiatePrefab(rageScaleUIPrefab, uiParent);
        }
    }
}
