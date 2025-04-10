using System;
using Factories;
using UnityEngine;
using Zenject;


namespace GameLoop
{
    public class GameOverHandler: IInitializable, IDisposable
    {
        private readonly GameTimer gameTimer;
        private readonly GameplayUIFactory gameplayUIFactory;


        public GameOverHandler(GameTimer gameTimer, GameplayUIFactory gameplayUIFactory)
        {
            this.gameTimer = gameTimer;
            this.gameplayUIFactory = gameplayUIFactory;
        }
        
        
        private void ShowGameOver()
        {
            gameplayUIFactory.CreateGameOverWindow().Forget();
        }


        public void Initialize()
        {
            gameTimer.OnTimerEnded += ShowGameOver;
        }


        public void Dispose()
        {
            gameTimer.OnTimerEnded -= ShowGameOver;
        }
    }
}
