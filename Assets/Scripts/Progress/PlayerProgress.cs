using System;


namespace Progress
{
    [Serializable]
    public class PlayerProgress
    {
        public ScoreData scoreData;
        public CurrencyData currencyData;
        

        public PlayerProgress()
        {
            scoreData = new ScoreData();
            currencyData = new CurrencyData();
        }
    }
}
