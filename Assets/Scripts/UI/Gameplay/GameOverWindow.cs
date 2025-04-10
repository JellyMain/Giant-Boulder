using System;
using Const;
using Infrastructure.GameStates;
using Infrastructure.Services;
using Progress;
using Scenes;
using UI.Meta;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Zenject;


namespace UI
{
    public class GameOverWindow : MonoBehaviour
    {
        [SerializeField] private Button restartButton;
        private GameStateMachine gameStateMachine;
        private SaveLoadService saveLoadService;
        private SceneLoader sceneLoader;


        [Inject]
        private void Construct(GameStateMachine gameStateMachine, SaveLoadService saveLoadService,
            SceneLoader sceneLoader)
        {
            this.gameStateMachine = gameStateMachine;
            this.saveLoadService = saveLoadService;
            this.sceneLoader = sceneLoader;
        }


        private void Start()
        {
            restartButton.onClick.AddListener(SaveAndReturnToMenu);
        }


        private void SaveAndReturnToMenu()
        {
            saveLoadService.SaveProgress();
            sceneLoader.Load(RuntimeConstants.Scenes.MAIN_MENU_SCENE, () => gameStateMachine.Enter<LoadMetaState>());
        }
    }
}
