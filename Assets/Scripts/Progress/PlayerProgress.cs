using System;


namespace Progress
{
    [Serializable]
    public class PlayerProgress
    {
        public CurrencyData currencyData;
        public ScoreData scoreData;
        public QuestsData questsData;
        public StatsData statsData;


        public PlayerProgress()
        {
            currencyData = new CurrencyData();
            scoreData = new ScoreData();
            questsData = new QuestsData();
            statsData = new StatsData();
        }
        
    }
}
