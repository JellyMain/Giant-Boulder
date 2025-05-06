using System;
using System.Collections.Generic;
using Progress;


namespace Upgrades
{
    public class UpgradesService
    {
        private readonly PersistentPlayerProgress persistentPlayerProgress;
        public Dictionary<Type, UpgradeData> ActiveUpgrades { get; private set; } = new Dictionary<Type, UpgradeData>();


        public UpgradesService(PersistentPlayerProgress persistentPlayerProgress)
        {
            this.persistentPlayerProgress = persistentPlayerProgress;
        }


        public void SetActiveUpgrade<T>(UpgradeData upgradeData) where T : UpgradeData
        {
            UpgradesIdStatusDictionary upgradesIdStatusDictionary =
                persistentPlayerProgress.PlayerProgress.upgradesData.upgradesIdStatusDictionary;
            
            
            ActiveUpgrades[typeof(T)] = upgradeData;

            int upgradeId = upgradeData.upgradeId;
            
            // upgradesIdStatusDictionary[upgradeId] = 
            
        }
    }
}
