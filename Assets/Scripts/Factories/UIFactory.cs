using Const;
using UI;
using UnityEngine;
using Zenject;


namespace Factories
{
    public class UIFactory
    {
        private readonly DiContainer diContainer;
        private Transform uiParent;
        

        public UIFactory(DiContainer diContainer)
        {
            this.diContainer = diContainer;
        }


        public PlayerControlsUI CreatePlayerControlsUI()
        {
            PlayerControlsUI playerControlsUIPrefab =
                Resources.Load<PlayerControlsUI>(RuntimeConstants.PrefabPaths.PLAYER_CONTROLS_UI);

            if (uiParent == null)
            {
                uiParent = new GameObject("UI").transform;
            }
            
            return diContainer.InstantiatePrefab(playerControlsUIPrefab, uiParent).GetComponent<PlayerControlsUI>();
        }


        public GameObject CreateScoreAndCurrencyUI(Camera uiCamera)
        {
            GameObject scoreAndCurrencyUIPrefab =
                Resources.Load<GameObject>(RuntimeConstants.PrefabPaths.SCORE_AND_CURRENCY_UI);

            if (uiParent == null)
            {
                uiParent = new GameObject("UI").transform;
            }

            GameObject scoreAndCurrencyUI = diContainer.InstantiatePrefab(scoreAndCurrencyUIPrefab, uiParent);

            Canvas canvas = scoreAndCurrencyUI.GetComponent<Canvas>();
            canvas.worldCamera = uiCamera;

            return scoreAndCurrencyUI;
        }
    }
}
