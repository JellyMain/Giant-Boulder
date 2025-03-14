using Progress;


namespace DataTrackers
{
    public class CurrencyTracker: IProgressSaver, IProgressUpdater
    {
        public int Coins { get; private set; }
        

        public void AddCoin()
        {
            Coins++;
        }


        public void SaveProgress(PlayerProgress playerProgress)
        {
            playerProgress.currencyData.coinsAmount = Coins;
        }


        public void UpdateProgress(PlayerProgress playerProgress)
        {
            Coins = playerProgress.currencyData.coinsAmount;
        }
    }
}