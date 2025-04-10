using Zenject;


namespace Infrastructure.Services
{
    public class ContainerService 
    {
        
        public DiContainer LocalContainer { get; private set; }
        public DiContainer GlobalContainer { get; private set; }
        

        public ContainerService(DiContainer globalDiContainer, [Inject(Source = InjectSources.Local)] DiContainer localContainer)
        {
            GlobalContainer = globalDiContainer;
            LocalContainer = localContainer;
        }


        public void SetLocalContainer(DiContainer container)
        {
            LocalContainer = container;
        }
    }
}