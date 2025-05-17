using System;
using System.Collections.Generic;
using Const;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Factories;
using Infrastructure.GameStates;
using Infrastructure.Services;
using Scenes;
using Sirenix.OdinInspector;
using StaticData.Data;
using StaticData.Services;
using UI.Meta.Quests;
using UnityEngine;
using UnityEngine.UI;
using Zenject;


namespace UI.Meta
{
    public class MainMenuUI : SerializedMonoBehaviour
    {
        [SerializeField] private Dictionary<Button, RectTransform> buttonsEndPoints;
        [SerializeField] private Button playButton;
        [SerializeField] private Button questsButton;
        [SerializeField] private Button almanacButton;
        [SerializeField] private Button statisticsButton;
        [SerializeField] private Button upgradesButton;
        [SerializeField] Canvas canvas;
        private GameStateMachine gameStateMachine;
        private SceneLoader sceneLoader;
        private MetaUIFactory metaUIFactory;
        private MainMenuAnimations mainMenuAnimations;


        [Inject]
        private void Construct(GameStateMachine gameStateMachine, SceneLoader sceneLoader, MetaUIFactory metaUIFactory,
            StaticDataService staticDataService)
        {
            this.sceneLoader = sceneLoader;
            this.gameStateMachine = gameStateMachine;
            this.metaUIFactory = metaUIFactory;
            mainMenuAnimations = staticDataService.AnimationsConfig.mainMenuAnimations;
        }


        private async void Start()
        {
            SetCamera();
            await AnimateButtons();
            playButton.onClick.AddListener(StartGame);
            questsButton.onClick.AddListener(OpenQuestsWindow);
            almanacButton.onClick.AddListener(OpenAlmanac);
            statisticsButton.onClick.AddListener(OpenStatisticsWindow);
            upgradesButton.onClick.AddListener(OpenUpgradesWindow);
        }


        private async UniTask AnimateButtons()
        {
            foreach (KeyValuePair<Button, RectTransform> buttonsEndPointPair in buttonsEndPoints)
            {
                buttonsEndPointPair.Key.transform.DOMove(buttonsEndPointPair.Value.position,
                        mainMenuAnimations.buttonsAppearTime)
                    .SetEase(Ease.OutElastic);
                await UniTask.WaitForSeconds(mainMenuAnimations.delayBeforeNextButtonAppear);
            }
        }


        private void SetCamera()
        {
            Camera uiCamera = GameObject.FindGameObjectWithTag(RuntimeConstants.Tags.UI_CAMERA).GetComponent<Camera>();
            canvas.worldCamera = uiCamera;
        }


        private void StartGame()
        {
            sceneLoader.Load(RuntimeConstants.Scenes.GAME_SCENE, () => gameStateMachine.Enter<LoadLevelState>());
        }


        private void OpenAlmanac() { }



        private void OpenQuestsWindow()
        {
            metaUIFactory.CreateQuestsWindow().Forget();
        }


        private void OpenStatisticsWindow()
        {
            metaUIFactory.CreateStatisticsWindow().Forget();
        }


        private void OpenUpgradesWindow()
        {
            metaUIFactory.CreateUpgradesWindow().Forget();
        }
    }
}
