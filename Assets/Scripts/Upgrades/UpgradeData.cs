using UnityEngine;


namespace Upgrades
{
    public class UpgradeData : ScriptableObject
    {
        public int upgradeId;
        public string upgradeTitle;
        public string upgradeDescription;
        public UpgradeType upgradeType;
        public int upgradeLevel;
        public int upgradePrice;
        
        
        private void OnEnable()
        {
            upgradeId = Mathf.Abs(name.GetHashCode());
        }
    }
}