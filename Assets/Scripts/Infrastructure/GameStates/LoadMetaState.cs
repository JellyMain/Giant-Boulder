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


        public LoadMetaState(SaveLoadService saveLoadService, MetaUIFactory metaUIFactory)
        {
            this.saveLoadService = saveLoadService;
            this.metaUIFactory = metaUIFactory;
        }


        public async void Enter()
        {
            await CreateMeta();
            saveLoadService.UpdateProgress();
        }


        private async UniTask CreateMeta()
        {
            metaUIFactory.CreateUIRoot();
            QuestsWindow questsWindow = await metaUIFactory.CreateQuestsWindow();
            await metaUIFactory.CreateMainMenuUI(questsWindow);
        }
    }
}
