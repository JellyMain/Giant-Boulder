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
        private float currentTime;
        private GameConfig gameConfig;
        private bool isStarted;


        [Inject]
        private void Construct(StaticDataService staticDataService)
        {
            gameConfig = staticDataService.GameConfig;
        }


        private void Start()
        {
            maxTime = gameConfig.maxTime;
        }


        private void Update()
        {
            if (isStarted)
            {
                currentTime -= Time.deltaTime;

                if (currentTime <= 0)
                {
                    currentTime = 0;
                    isStarted = false;
                }
            }
        }


        public void StartTimer()
        {
            isStarted = true;
            currentTime = maxTime;
        }
    }
}
