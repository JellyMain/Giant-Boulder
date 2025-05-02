using Const;
using Cysharp.Threading.Tasks;
using Factories;
using Infrastructure.GameStates.Interfaces;
using Infrastructure.Services;
using Progress;
using UI;
using UI.Meta;
using UnityEngine;


namespace Infrastructure.GameStates
{
    public class LoadMetaState : IGameState
    {
        private readonly SaveLoadService saveLoadService;
        private readonly MetaUIFactory metaUIFactory;
        private readonly CameraCreator cameraCreator;


        public LoadMetaState(SaveLoadService saveLoadService, MetaUIFactory metaUIFactory, CameraCreator cameraCreator)
        {
            this.saveLoadService = saveLoadService;
            this.metaUIFactory = metaUIFactory;
            this.cameraCreator = cameraCreator;
        }


        public async void Enter()
        {
            CreateCameras().Forget();
            await CreateMeta();
            saveLoadService.UpdateProgress();
        }


        private async UniTaskVoid CreateCameras()
        {
            Camera uiCamera = await cameraCreator.CreateUICamera();
            cameraCreator.StackCamera(uiCamera);
        }


        private async UniTask CreateMeta()
        {
            metaUIFactory.CreateUIRoot();
            await metaUIFactory.CreateMainMenuUI();
        }
    }
}
