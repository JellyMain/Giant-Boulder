using Factories;
using Infrastructure.GameStates;
using Zenject;


namespace Infrastructure.Installers.Scene
{
    public class MenuSceneInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            BindLoadMetaState();
            BindMetaUIFactory();
        }


        private void BindLoadMetaState()
        {
            Container.Bind<LoadMetaState>().AsSingle().NonLazy();
        }
        
        
        private void BindMetaUIFactory()
        {
            Container.Bind<MetaUIFactory>().AsSingle();
        }
    }
}
