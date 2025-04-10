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
        }
        
        
        private void BindGameStateMachine()
        {
            Container.BindInterfacesAndSelfTo<GameStateMachine>().AsSingle().NonLazy();
        }


        private void BindLoadStates()
        {
            Container.Bind<BootstrapState>().AsSingle().NonLazy();
            Container.Bind<LoadProgressState>().AsSingle().NonLazy();
        }
    }
}
