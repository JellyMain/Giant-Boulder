using Assets;
using Const;
using Cysharp.Threading.Tasks;
using Progress;
using Quests;
using UI.Meta;
using UI.Meta.Quests;
using UnityEngine;
using Zenject;


namespace Factories
{
    public class MetaUIFactory
    {
        private readonly AssetProvider assetProvider;
        private readonly QuestService questService;
        private readonly SaveLoadService saveLoadService;
        private readonly DiContainer diContainer;
        private Transform uiRoot;


        public MetaUIFactory(AssetProvider assetProvider, QuestService questService, SaveLoadService saveLoadService,
            DiContainer diContainer)
        {
            this.assetProvider = assetProvider;
            this.questService = questService;
            this.saveLoadService = saveLoadService;
            this.diContainer = diContainer;
        }


        public void CreateUIRoot()
        {
            uiRoot = new GameObject("UIRoot").transform;
        }


        public async UniTask CreateMainMenuUI(QuestsWindow questsWindow)
        {
            GameObject mainMenuPrefab =
                await assetProvider.LoadAsset<GameObject>(RuntimeConstants.PrefabAddresses.MAIN_MENU_UI);

            MainMenuUI createdMainMenu =
                diContainer.InstantiatePrefab(mainMenuPrefab, uiRoot).GetComponent<MainMenuUI>();
            createdMainMenu.Construct(questsWindow);
        }


        public async UniTask<QuestsWindow> CreateQuestsWindow()
        {
            GameObject questsWindowPrefab =
                await assetProvider.LoadAsset<GameObject>(RuntimeConstants.PrefabAddresses.QUESTS_WINDOW_UI);

            QuestsWindow createdQuestsWindow =
                diContainer.InstantiatePrefab(questsWindowPrefab, uiRoot).GetComponent<QuestsWindow>();
            Transform questsParent = createdQuestsWindow.QuestsContainer;

            await CreateQuestsUI(questsParent);

            return createdQuestsWindow;
        }


        private async UniTask CreateQuestsUI(Transform parent)
        {
            GameObject questUIPrefab =
                await assetProvider.LoadAsset<GameObject>(RuntimeConstants.PrefabAddresses.QUEST_UI);

            QuestUI createdQuest = diContainer.InstantiatePrefab(questUIPrefab, parent).GetComponent<QuestUI>();
            createdQuest.SetQuest(questService.CurrentQuest);
            saveLoadService.RegisterSceneObject(createdQuest);
        }
    }
}
