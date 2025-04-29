using System;
using StaticData.Data;
using StaticData.Services;
using UnityEngine;
using Zenject;


namespace GameLoop
{
    public class GameTimer : MonoBehaviour
    {
        private float maxTime;
        private GameConfig gameConfig;
        private StaticDataService staticDataService;
        public float CurrentTime { get; private set; }
        public bool IsStarted { get; private set; }
        public event Action OnTimerEnded;


        [Inject]
        private void Construct(StaticDataService staticDataService)
        {
            this.staticDataService = staticDataService;
        }

        
        private void Start()
        {
            gameConfig = staticDataService.GameConfig;
            maxTime = gameConfig.maxTimerTime;
        }


        private void Update()
        {
            if (IsStarted)
            {
                CurrentTime -= Time.deltaTime;

                if (CurrentTime <= 0)
                {
                    CurrentTime = 0;
                    IsStarted = false;
                    OnTimerEnded?.Invoke();
                }
            }
        }


        public void StartTimer()
        {
            IsStarted = true;
            CurrentTime = maxTime;
        }
    }
}