using UnityEngine;


namespace Upgrades
{
    [CreateAssetMenu(menuName = "Upgrades/PlayerSpeedUpgrade", fileName = "PlayerSpeedUpgrade")]
    public class PlayerSpeedUpgradeData : UpgradeData
    {
        public float percentUpgrade;
    }
}
