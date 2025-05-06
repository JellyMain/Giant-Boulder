using UnityEngine;


namespace Upgrades
{
    public class UpgradeData : ScriptableObject
    {
        public int upgradeId;
        public string upgradeTitle;
        public string upgradeDescription;
        
        
        private void OnEnable()
        {
            upgradeId = Mathf.Abs(name.GetHashCode());
        }
    }
}