using System;
using System.Collections.Generic;
using Progress;
using StaticData.Data;
using StaticData.Services;
using UnityEngine;


namespace Upgrades
{
    public class UpgradesService
    {
        private readonly PersistentPlayerProgress persistentPlayerProgress;
        private readonly StaticDataService staticDataService;

        public Dictionary<UpgradeType, UpgradeData> ActiveUpgrades { get; private set; } =
            new Dictionary<UpgradeType, UpgradeData>();


        public UpgradesService(PersistentPlayerProgress persistentPlayerProgress, StaticDataService staticDataService)
        {
            this.persistentPlayerProgress = persistentPlayerProgress;
            this.staticDataService = staticDataService;
        }


        public void SetSavedUpgrades()
        {
            UpgradesTypeLevelDictionary upgradesTypeLevelDictionary =
                persistentPlayerProgress.PlayerProgress.upgradesData.upgradesTypeLevelDictionary;

            Dictionary<UpgradeType, List<UpgradeData>> allUpgrades = staticDataService.UpgradesConfig;


            foreach (KeyValuePair<UpgradeType, int> upgradesDataPair in upgradesTypeLevelDictionary)
            {
                foreach (UpgradeData upgradeData in allUpgrades[upgradesDataPair.Key])
                {
                    if (upgradeData.upgradeLevel == upgradesDataPair.Value)
                    {
                        ActiveUpgrades[upgradesDataPair.Key] = upgradeData;
                    }
                }
            }
        }


        public void SetActiveUpgrade(UpgradeData upgradeData)
        {
            UpgradesTypeLevelDictionary upgradesTypeLevelDictionary =
                persistentPlayerProgress.PlayerProgress.upgradesData.upgradesTypeLevelDictionary;

            UpgradeType upgradeType = upgradeData.upgradeType;

            ActiveUpgrades[upgradeType] = upgradeData;

            upgradesTypeLevelDictionary[upgradeType] = upgradeData.upgradeLevel;
            
            Debug.Log($"Set upgrade with type {upgradeType} and level {upgradeData.upgradeLevel}");
        }
    }
}
