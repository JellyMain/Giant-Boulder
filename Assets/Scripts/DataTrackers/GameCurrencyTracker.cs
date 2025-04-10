using System;
using Factories;
using Player;
using Progress;
using UnityEngine;
using Zenject;


namespace DataTrackers
{
    public class GameCurrencyTracker : IInitializable, IDisposable, IProgressSaver, IProgressUpdater
    {
        private readonly SaveLoadService saveLoadService;
        private readonly PlayerFactory playerFactory;
        private readonly StatsTracker statsTracker;
        private CoinCollector coinCollector;
        public int Coins { get; private set; }
        public int SessionCoins { get; private set; }

        public delegate void OnCoinAddedHandler(int currentAmount, Vector3 coinPosition);

        public event OnCoinAddedHandler OnCoinAdded;


        public GameCurrencyTracker(SaveLoadService saveLoadService, PlayerFactory playerFactory,
            StatsTracker statsTracker)
        {
            this.saveLoadService = saveLoadService;
            this.playerFactory = playerFactory;
            this.statsTracker = statsTracker;
        }


        public void Initialize()
        {
            saveLoadService.RegisterSceneObject(this);
            playerFactory.OnPlayerCreated += SetAndSubscribeCoinCollector;
        }


        private void SetAndSubscribeCoinCollector(GameObject player)
        {
            coinCollector = player.GetComponent<CoinCollector>();
            coinCollector.OnCoinCollected += AddCoin;
        }


        public void AddCoin(Vector3 coinPosition)
        {
            SessionCoins++;
            Coins++;
            statsTracker.AddCoin();
            OnCoinAdded?.Invoke(Coins, coinPosition);
        }


        public void SaveProgress(PlayerProgress playerProgress)
        {
            playerProgress.currencyData.coinsAmount = Coins;
        }


        public void UpdateProgress(PlayerProgress playerProgress)
        {
            Coins = playerProgress.currencyData.coinsAmount;
        }


        public void Dispose()
        {
            playerFactory.OnPlayerCreated -= SetAndSubscribeCoinCollector;
            coinCollector.OnCoinCollected -= AddCoin;
        }
    }
}
