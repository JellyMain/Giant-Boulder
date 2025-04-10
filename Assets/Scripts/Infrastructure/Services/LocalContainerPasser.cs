using Zenject;


namespace Infrastructure.Services
{
    public class LocalContainerPasser
    {
        public LocalContainerPasser(DiContainer diContainer, ContainerService containerService)
        {
            containerService.SetLocalContainer(diContainer);
        }
    }
}
