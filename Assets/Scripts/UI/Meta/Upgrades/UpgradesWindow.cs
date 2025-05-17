using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Factories;
using Progress;
using StaticData.Data;
using StaticData.Services;
using UnityEngine;
using Upgrades;
using Zenject;


namespace UI.Meta.Upgrades
{
    public class UpgradesWindow : WindowBase
    {
        [SerializeField] private Transform upgradesContainer;
        private PersistentPlayerProgress persistentPlayerProgress;
        private StaticDataService staticDataService;
        private MetaUIFactory metaUIFactory;
        private List<UpgradeUI> initializedUpgradesUI;


        [Inject]
        private void Construct(PersistentPlayerProgress persistentPlayerProgress, StaticDataService staticDataService,
            MetaUIFactory metaUIFactory)
        {
            this.persistentPlayerProgress = persistentPlayerProgress;
            this.staticDataService = staticDataService;
            this.metaUIFactory = metaUIFactory;
        }


        private void OnDisable()
        {
            UnsubscribeFromUpgradesUI();
        }


        protected override void OnStart()
        {
            base.OnStart();
            InitUpgrades().Forget();
        }


        private async UniTaskVoid InitUpgrades()
        {
            initializedUpgradesUI = new List<UpgradeUI>();

            UpgradeType[] allUpgradeTypes = (UpgradeType[])Enum.GetValues(typeof(UpgradeType));

            UpgradesTypeLevelDictionary upgradesLevelDictionary =
                persistentPlayerProgress.PlayerProgress.upgradesData.upgradesTypeLevelDictionary;

            foreach (UpgradeType upgradeType in allUpgradeTypes)
            {
                int upgradeLevel = 0;

                if (upgradesLevelDictionary.TryGetValue(upgradeType, out int level))
                {
                    upgradeLevel = level + 1;
                }

                UpgradeData upgradeData = staticDataService.UpgradeDataForLevelAndType(upgradeType, upgradeLevel);
                UpgradeUI upgradeUI = await InitUpgradeUI(upgradeData);

                initializedUpgradesUI.Add(upgradeUI);
            }
        }


        private async UniTask<UpgradeUI> InitUpgradeUI(UpgradeData upgradeData)
        {
            UpgradeUI spawnedUpgradeUI = await metaUIFactory.CreateUpgradeUI(upgradesContainer);
            spawnedUpgradeUI.SetUpgradeData(upgradeData);

            spawnedUpgradeUI.OnUpgradeBought += IterateUpgrade;

            return spawnedUpgradeUI;
        }


        private void UnsubscribeFromUpgradesUI()
        {
            foreach (UpgradeUI upgradeUI in initializedUpgradesUI)
            {
                upgradeUI.OnUpgradeBought -= IterateUpgrade;
            }
        }


        private void IterateUpgrade(UpgradeUI upgradeUI, UpgradeData oldUpgradeData)
        {
            UpgradeData newUpgradeData =
                staticDataService.UpgradeDataForLevelAndType(oldUpgradeData.upgradeType,
                    oldUpgradeData.upgradeLevel + 1);

            upgradeUI.SetUpgradeData(newUpgradeData);
        }
    }
}
