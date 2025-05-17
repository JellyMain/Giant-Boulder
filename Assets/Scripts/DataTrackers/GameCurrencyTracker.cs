using System;
using Coins;
using Factories;
using GameLoop;
using Player;
using Progress;
using Stats;
using UnityEngine;
using Zenject;


namespace DataTrackers
{
    public class GameCurrencyTracker : IDisposable
    {
        private readonly PlayerFactory playerFactory;
        private readonly StatsService statsService;
        private readonly GameLoopStatesHandler gameLoopStatesHandler;
        private readonly CurrencyService currencyService;
        private CoinCollector coinCollector;
        public int SessionCoins { get; private set; }

        public delegate void OnCoinAddedHandler(int currentAmount, Vector3 coinPosition);

        public event OnCoinAddedHandler OnCoinAdded;


        public GameCurrencyTracker(PlayerFactory playerFactory, StatsService statsService,
            GameLoopStatesHandler gameLoopStatesHandler, CurrencyService currencyService)
        {
            this.playerFactory = playerFactory;
            this.statsService = statsService;
            this.gameLoopStatesHandler = gameLoopStatesHandler;
            this.currencyService = currencyService;
        }


        public void Init()
        {
            gameLoopStatesHandler.OnGameSessionOver += PassCoinsToServices;
            SetPlayer();
        }


        private void SetPlayer()
        {
            if (playerFactory.Player != null)
            {
                SetAndSubscribeCoinCollector(playerFactory.Player);
            }
            else
            {
                playerFactory.OnPlayerCreated += SetAndSubscribeCoinCollector;
            }
        }


        private void SetAndSubscribeCoinCollector(GameObject player)
        {
            coinCollector = player.GetComponent<CoinCollector>();
            coinCollector.OnCoinCollected += AddCoin;
        }


        private void PassCoinsToServices()
        {
            currencyService.AddCoins(SessionCoins);
            statsService.AddCoins(SessionCoins);
        }


        private void AddCoin(Vector3 coinPosition)
        {
            SessionCoins++;
            OnCoinAdded?.Invoke(SessionCoins, coinPosition);
        }


        public void Dispose()
        {
            playerFactory.OnPlayerCreated -= SetAndSubscribeCoinCollector;
            gameLoopStatesHandler.OnGameSessionOver -= PassCoinsToServices;
            coinCollector.OnCoinCollected -= AddCoin;
        }
    }
}
