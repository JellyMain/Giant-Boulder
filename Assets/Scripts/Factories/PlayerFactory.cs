using Constants;
using StaticData.Services;
using UnityEngine;
using Zenject;


namespace Factories
{
    public class PlayerFactory
    {
        private readonly StaticDataService staticDataService;
        private readonly DiContainer diContainer;


        public PlayerFactory(StaticDataService staticDataService, DiContainer diContainer)
        {
            this.staticDataService = staticDataService;
            this.diContainer = diContainer;
        }

        
    }
}