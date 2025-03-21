using Factories;
using GameLoop;
using Infrastructure.GameStates.Interfaces;
using Infrastructure.Services;


namespace Infrastructure.GameStates
{
    public class GameLoopState : IGameState
    {
        private readonly GameStateMachine gameStateMachine;


        public GameLoopState(GameStateMachine gameStateMachine)
        {
            this.gameStateMachine = gameStateMachine;
        }


        public void Enter() { }
    }
}
