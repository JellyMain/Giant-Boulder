using System;
using Const;
using Cysharp.Threading.Tasks;
using Factories;
using Infrastructure.GameStates;
using Infrastructure.Services;
using Scenes;
using UnityEngine;
using UnityEngine.UI;
using Zenject;


namespace UI.Meta
{
    public class MainMenuUI : MonoBehaviour
    {
        [SerializeField] private Button playButton;
        [SerializeField] private Button questsButton;
        [SerializeField] private Button almanacButton;
        [SerializeField] private Button statisticsWindow;
        [SerializeField] Canvas canvas;
        private GameStateMachine gameStateMachine;
        private SceneLoader sceneLoader;
        private MetaUIFactory metaUIFactory;
        public QuestsWindow QuestsWindow { get; set; }


        [Inject]
        private void Construct(GameStateMachine gameStateMachine, SceneLoader sceneLoader, MetaUIFactory metaUIFactory)
        {
            this.sceneLoader = sceneLoader;
            this.gameStateMachine = gameStateMachine;
            this.metaUIFactory = metaUIFactory;
        }
        
        
        private void Start()
        {
            SetCamera();
            playButton.onClick.AddListener(StartGame);
            questsButton.onClick.AddListener(OpenQuestsWindow);
            almanacButton.onClick.AddListener(OpenAlmanac);
            statisticsWindow.onClick.AddListener(OpenStatisticsWindow);
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
    }
}
