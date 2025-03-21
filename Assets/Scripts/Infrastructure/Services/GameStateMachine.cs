using System;
using System.Collections.Generic;
using Factories;
using Infrastructure.GameStates;
using Infrastructure.GameStates.Interfaces;
using Zenject;


namespace Infrastructure.Services
{
    public class GameStateMachine : IInitializable
    {
        private readonly GameStatesFactory gameStatesFactory;
        private Dictionary<Type, IGameState> states;


        public GameStateMachine(GameStatesFactory gameStatesFactory)
        {
            this.gameStatesFactory = gameStatesFactory;
        }


        public void Initialize()
        {
            states = new Dictionary<Type, IGameState>()
            {
                [typeof(BootstrapState)] = gameStatesFactory.CreateState<BootstrapState>(),
                [typeof(LoadProgressState)] = gameStatesFactory.CreateState<LoadProgressState>(),
                [typeof(LoadMetaState)] = gameStatesFactory.CreateState<LoadMetaState>(),
                [typeof(LoadLevelState)] = gameStatesFactory.CreateState<LoadLevelState>(),
                [typeof(GameLoopState)] = gameStatesFactory.CreateState<GameLoopState>()
            };
        }


        public void Enter<TState>() where TState : class, IGameState
        {
            TState newState = GetState<TState>();
            newState.Enter();
        }


        private TState GetState<TState>() where TState : class, IGameState
        {
            return states[typeof(TState)] as TState;
        }
    }
}
