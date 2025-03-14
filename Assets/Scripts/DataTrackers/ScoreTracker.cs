namespace DataTrackers
{
    public class ScoreTracker
    {
        public int CurrentScore { get; private set; }
        
        
    
        public void AddScore(int scoreValue)
        {
            CurrentScore += scoreValue;
        }
    }
}
