using Factories;
using Infrastructure.GameStates;
using Infrastructure.Services;
using Zenject;


namespace Infrastructure.Installers.Global
{
    public class GameStateMachineInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            BindGameStateMachine();
            BindLoadStates();
            BindGameStatesFactory();
        }
        
        
        private void BindGameStatesFactory()
        {
            Container.Bind<GameStatesFactory>().AsSingle();
        }


        private void BindGameStateMachine()
        {
            Container.BindInterfacesAndSelfTo<GameStateMachine>().AsSingle().NonLazy();
        }


        private void BindLoadStates()
        {
            Container.Bind<BootstrapState>().AsSingle().NonLazy();
            Container.Bind<LoadProgressState>().AsSingle().NonLazy();
            Container.Bind<LoadMetaState>().AsSingle().NonLazy();
            Container.Bind<LoadLevelState>().AsSingle().NonLazy();
            Container.Bind<GameLoopState>().AsSingle().NonLazy();
        }
    }
}
