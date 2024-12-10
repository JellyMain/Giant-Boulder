using Infrastructure.GameStates.Interfaces;
using Zenject;


namespace Factories
{
    public class GameStatesFactory
    {
        private readonly DiContainer container;
    
        public GameStatesFactory(DiContainer container)
        {
            this.container = container;
        }


        public TState CreateState<TState>() where TState : IGameState
        {
            return container.Resolve<TState>();
        }
    }
}
