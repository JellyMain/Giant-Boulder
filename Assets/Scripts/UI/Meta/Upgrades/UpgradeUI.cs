using System;
using Coins;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Upgrades;
using Zenject;


namespace UI.Meta.Upgrades
{
    public class UpgradeUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text upgradeTitle;
        [SerializeField] private TMP_Text upgradeDescription;
        [SerializeField] private TMP_Text upgradePrice;
        [SerializeField] private Button buyButton;
        private UpgradesService upgradesService;
        private UpgradeData upgradeData;
        private CurrencyService currencyService;
        public event Action<UpgradeUI, UpgradeData> OnUpgradeBought;



        [Inject]
        private void Construct(UpgradesService upgradesService, CurrencyService currencyService)
        {
            this.upgradesService = upgradesService;
            this.currencyService = currencyService;
        }


        private void OnEnable()
        {
            buyButton.onClick.AddListener(TryBuyUpgrade);
        }


        private void OnDisable()
        {
            buyButton.onClick.RemoveListener(TryBuyUpgrade);
        }


        public void SetUpgradeData(UpgradeData upgradeData)
        {
            this.upgradeData = upgradeData;

            upgradeTitle.text = upgradeData.upgradeTitle;
            upgradeDescription.text = upgradeData.upgradeDescription;
            upgradePrice.text = upgradeData.upgradePrice.ToString();
        }


        private void TryBuyUpgrade()
        {
            if (currencyService.Coins >= upgradeData.upgradePrice)
            {
                BuyUpgrade();
            }
            else
            {
                Debug.Log("Don't have enough money");
            }
        }


        private void BuyUpgrade()
        {
            upgradesService.SetActiveUpgrade(upgradeData);
            currencyService.SubtractCoins(upgradeData.upgradePrice);
            OnUpgradeBought?.Invoke(this, upgradeData);

            Debug.Log($"Coins after purchase {currencyService.Coins}");
        }
    }
}
