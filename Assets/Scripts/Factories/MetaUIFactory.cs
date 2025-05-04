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


        public async UniTask CreateMainMenuUI()
        {
            GameObject mainMenuPrefab =
                await assetProvider.LoadAsset<GameObject>(RuntimeConstants.PrefabAddresses.MAIN_MENU_UI);

            diContainer.InstantiatePrefab(mainMenuPrefab, uiRoot).GetComponent<MainMenuUI>();
        }


        public async UniTaskVoid CreateQuestsWindow()
        {
            GameObject questsWindowPrefab =
                await assetProvider.LoadAsset<GameObject>(RuntimeConstants.PrefabAddresses.QUESTS_WINDOW_UI);

            diContainer.InstantiatePrefab(questsWindowPrefab, uiRoot).GetComponent<QuestsWindow>();
        }


        public async UniTask<QuestUI> CreateQuestUI(Transform parent)
        {
            GameObject questUIPrefab =
                await assetProvider.LoadAsset<GameObject>(RuntimeConstants.PrefabAddresses.QUEST_UI);

            return diContainer.InstantiatePrefab(questUIPrefab, parent).GetComponent<QuestUI>();
        }


        public async UniTaskVoid CreateStatisticsWindow()
        {
            GameObject statisticsWindowPrefab =
                await assetProvider.LoadAsset<GameObject>(RuntimeConstants.PrefabAddresses.STATISTICS_WINDOW_UI);

            diContainer.InstantiatePrefab(statisticsWindowPrefab, uiRoot);
        }
    }
}
