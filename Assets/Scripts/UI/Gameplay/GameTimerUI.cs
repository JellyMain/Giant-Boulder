using System;
using GameLoop;
using TMPro;
using UnityEngine;
using Zenject;


namespace UI
{
    public class GameTimerUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text timerText;
        private GameTimer gameTimer;


        [Inject]
        private void Construct(GameTimer gameTimer)
        {
            this.gameTimer = gameTimer;
        }


        private void Update()
        {
            timerText.text = gameTimer.CurrentTime.ToString("F2");
        }
    }
}
