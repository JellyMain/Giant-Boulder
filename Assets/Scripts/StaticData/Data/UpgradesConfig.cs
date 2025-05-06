using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using Upgrades;


namespace StaticData.Data
{
    [CreateAssetMenu(menuName = "StaticData/UpgradesConfig", fileName = "UpgradesConfig")]
    public class UpgradesConfig : SerializedScriptableObject
    {
        public Dictionary<Type, List<UpgradeData>> upgradesConfig;
    }
}
