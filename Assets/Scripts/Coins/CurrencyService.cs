using Progress;
using UnityEngine;
using Zenject;


namespace Coins
{
    public class CurrencyService : IInitializable, IProgressUpdater, IProgressSaver
    {
        public int Coins { get; private set; }
        private readonly SaveLoadService saveLoadService;


        public CurrencyService(SaveLoadService saveLoadService)
        {
            this.saveLoadService = saveLoadService;
        }


        public void Initialize()
        {
            saveLoadService.RegisterGlobalObject(this);
        }


        public void AddCoins(int coinsToAdd)
        {
            Coins += coinsToAdd;
        }


        public void SubtractCoins(int coinsToSubtract)
        {
            Coins -= coinsToSubtract;
        }


        public void UpdateProgress(PlayerProgress playerProgress)
        {
            Coins = playerProgress.currencyData.coins;
            Debug.Log($"Coins: {Coins}");
        }


        public void SaveProgress(PlayerProgress playerProgress)
        {
            playerProgress.currencyData.coins = Coins;
            Debug.Log($"Coins: {Coins}");
        }
    }
}
