using Infrastructure.GameStates.Interfaces;
using UnityEngine;


namespace Infrastructure.Services
{
    public class GameStateMachine
    {
        private readonly ContainerService containerService;


        public GameStateMachine(ContainerService containerService)
        {
            this.containerService = containerService;
        }

        
        public void Enter<TState>() where TState : class, IGameState
        {
            TState newState = CreateOrGetState<TState>();
            newState.Enter();
        }


        private TState CreateOrGetState<TState>() where TState : class, IGameState
        {
            TState state = containerService.GlobalContainer.TryResolve<TState>() ??
                           containerService.LocalContainer.Resolve<TState>();

            if (state == null)
            {
                Debug.LogError("Game state is not bind or resolved correctly");
            }

            return state;
        }
    }
}
