using System;
using GameLoop;
using Unity.VisualScripting;
using UnityEngine;


namespace DataTrackers
{
    public class ScoreTracker : IInitializable, IDisposable
    {
        public int CurrentScore { get; private set; }
        private readonly RageScale rageScale;

        public delegate void ScoreAddedHandler(int score, Vector3 scoreWorldPosition);

        public event ScoreAddedHandler OnScoreAdded;


        public ScoreTracker(RageScale rageScale)
        {
            this.rageScale = rageScale;
            rageScale.OnScoreCollected += AddScore;
        }


        private void AddScore(int scoreValue, int multiplier, Vector3 scoreWorldPosition)
        {
            int scoreSum = scoreValue * multiplier;
            CurrentScore += scoreSum;
            OnScoreAdded?.Invoke(scoreSum, scoreWorldPosition);
        }


        public void Initialize()
        {
           
        }


        public void Dispose()
        {
            rageScale.OnScoreCollected -= AddScore;
        }


        
    }
}