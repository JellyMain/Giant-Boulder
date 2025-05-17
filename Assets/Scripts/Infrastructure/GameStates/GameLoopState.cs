using DataTrackers;
using Factories;
using GameLoop;
using Infrastructure.GameStates.Interfaces;
using Infrastructure.Services;
using UnityEngine;


namespace Infrastructure.GameStates
{
    public class GameLoopState : IGameState
    {
        private readonly GameLoopStatesHandler gameLoopStatesHandler;
        private readonly GameCurrencyTracker gameCurrencyTracker;
        private readonly GameTimer gameTimer;
        private readonly DestroyedObjectsTracker destroyedObjectsTracker;


        public GameLoopState(GameLoopStatesHandler gameLoopStatesHandler, GameCurrencyTracker gameCurrencyTracker,
            GameTimer gameTimer, DestroyedObjectsTracker destroyedObjectsTracker)
        {
            this.gameLoopStatesHandler = gameLoopStatesHandler;
            this.gameCurrencyTracker = gameCurrencyTracker;
            this.gameTimer = gameTimer;
            this.destroyedObjectsTracker = destroyedObjectsTracker;
        }


        public void Enter()
        {
            InitGameLoopServices();
            gameLoopStatesHandler.StartGameSession();
            gameTimer.StartTimer();
        }


        private void InitGameLoopServices()
        {
            gameLoopStatesHandler.Init();
            gameCurrencyTracker.Init();
            destroyedObjectsTracker.Init();
        }
    }
}
