using System;
using Factories;
using UnityEngine;
using Zenject;


namespace GameLoop
{
    public class GameLoopStatesHandler:  IDisposable
    {
        private readonly GameTimer gameTimer;
        private readonly GameplayUIFactory gameplayUIFactory;
        public event Action OnGameSessionOver;
        public event Action OnGameSessionStarted;


        public GameLoopStatesHandler(GameTimer gameTimer, GameplayUIFactory gameplayUIFactory)
        {
            this.gameTimer = gameTimer;
            this.gameplayUIFactory = gameplayUIFactory;
        }
        
        
        public void Init()
        {
            gameTimer.OnTimerEnded += ShowGameOver;
        }
        
        
        private void ShowGameOver()
        {
            gameplayUIFactory.CreateGameOverWindow().Forget();
            OnGameSessionOver?.Invoke();
        }

        
        public void StartGameSession()
        {
            OnGameSessionStarted?.Invoke();
        }


        public void Dispose()
        {
            gameTimer.OnTimerEnded -= ShowGameOver;
        }
    }
}