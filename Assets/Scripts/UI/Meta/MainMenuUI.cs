using System;
using Const;
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
        private GameStateMachine gameStateMachine;
        private SceneLoader sceneLoader;
        public QuestsWindow QuestsWindow { get; set; }


        [Inject]
        private void Construct(GameStateMachine gameStateMachine, SceneLoader sceneLoader, MetaUIFactory metaUIFactory)
        {
            this.sceneLoader = sceneLoader;
            this.gameStateMachine = gameStateMachine;
        }


        public void Construct(QuestsWindow questsWindow)
        {
            QuestsWindow = questsWindow;
        }

        
        private void Start()
        {
            playButton.onClick.AddListener(StartGame);
            questsButton.onClick.AddListener(OpenQuestsWindow);
            almanacButton.onClick.AddListener(OpenAlmanac);
        }


        public void StartGame()
        {
            sceneLoader.Load(RuntimeConstants.Scenes.GAME_SCENE, () => gameStateMachine.Enter<LoadLevelState>());
        }


        public void OpenAlmanac() { }


        
        public void OpenQuestsWindow()
        {
           QuestsWindow.WindowOverlay.SetActive(true);
        }
    }
}
