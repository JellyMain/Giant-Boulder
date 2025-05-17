using Assets;
using Const;
using Cysharp.Threading.Tasks;
using Progress;
using Quests;
using UI.Meta;
using UI.Meta.Quests;
using UI.Meta.Upgrades;
using UnityEngine;
using Zenject;


namespace Factories
{
    public class MetaUIFactory
    {
        private readonly AssetProvider assetProvider;
        private readonly QuestsService questsService;
        private readonly SaveLoadService saveLoadService;
        private readonly DiContainer diContainer;
        private Transform uiRoot;


        public MetaUIFactory(AssetProvider assetProvider, QuestsService questsService, SaveLoadService saveLoadService,
            DiContainer diContainer)
        {
            this.assetProvider = assetProvider;
            this.questsService = questsService;
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


        public async UniTask<UpgradeUI> CreateUpgradeUI(Transform parent)
        {
            GameObject upgradeUIPrefab =
                await assetProvider.LoadAsset<GameObject>(RuntimeConstants.PrefabAddresses.UPGRADE_UI);

            return diContainer.InstantiatePrefab(upgradeUIPrefab, parent).GetComponent<UpgradeUI>();
        }


        public async UniTaskVoid CreateUpgradesWindow()
        {
            GameObject upgradesWindowPrefab =
                await assetProvider.LoadAsset<GameObject>(RuntimeConstants.PrefabAddresses.UPGRADES_WINDOW_UI);

            diContainer.InstantiatePrefab(upgradesWindowPrefab, uiRoot);
        }
    }
}
