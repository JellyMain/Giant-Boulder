using UI;
using UnityEngine;
using Zenject;


namespace Factories
{
    public class UIFactory
    {
        private readonly DiContainer diContainer;


        public UIFactory(DiContainer diContainer)
        {
            this.diContainer = diContainer;
        }


        
    }
}