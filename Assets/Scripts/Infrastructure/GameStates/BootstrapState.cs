using Infrastructure.GameStates.Interfaces;
using Infrastructure.Services;
using UnityEngine;


namespace Infrastructure.GameStates
{
    public class BootstrapState : IGameState
    {
        private readonly GameStateMachine gameStateMachine;
    
    
        public BootstrapState(GameStateMachine gameStateMachine)
        {
            this.gameStateMachine = gameStateMachine;
        }


        public void Enter()
        {
            gameStateMachine.Enter<LoadProgressState>();    
        }
    }
}
