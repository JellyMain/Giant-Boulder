using Infrastructure.GameStates;
using Infrastructure.Services;
using UnityEngine;
using Zenject;


namespace UI
{
    public class MainMenuUI : MonoBehaviour
    {
        private GameStateMachine gameStateMachine;


        [Inject]
        private void Construct(GameStateMachine gameStateMachine)
        {
            this.gameStateMachine = gameStateMachine;
        }
        
        
        public void StartGame()
        {
            gameStateMachine.Enter<LoadLevelState>();
        }
    }
}