using Infrastructure.GameStates.Interfaces;
using Infrastructure.Services;


namespace Infrastructure.GameStates
{
    public class LoadProgressState : IGameState
    {
        private readonly GameStateMachine gameStateMachine;


        public LoadProgressState(GameStateMachine gameStateMachine)
        {
            this.gameStateMachine = gameStateMachine;
        }


        public void Enter()
        {
            LoadSavesOrCreateNew();
            gameStateMachine.Enter<LoadMetaState>();
        }


        private void LoadSavesOrCreateNew()
        {
            
        }
    }
}
