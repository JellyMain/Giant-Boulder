using Factories;
using GameLoop;
using Infrastructure.GameStates.Interfaces;
using Infrastructure.Services;
using UnityEngine;


namespace Infrastructure.GameStates
{
    public class GameLoopState : IGameState
    {
        private readonly GameStateMachine gameStateMachine;
        private readonly GameTimer gameTimer;


        public GameLoopState(GameStateMachine gameStateMachine, GameTimer gameTimer)
        {
            this.gameStateMachine = gameStateMachine;
            this.gameTimer = gameTimer;
        }


        public void Enter()
        {
            gameTimer.StartTimer();
        }
    }
}
