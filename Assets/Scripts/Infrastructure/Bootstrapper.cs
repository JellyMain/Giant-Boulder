using System;
using Infrastructure.GameStates;
using Infrastructure.Services;
using UnityEngine;
using Zenject;


namespace Infrastructure
{
    public class Bootstrapper : MonoBehaviour
    {
        private GameStateMachine gameStateMachine;


        [Inject]
        private void Construct(GameStateMachine gameStateMachine)
        {
            this.gameStateMachine = gameStateMachine;
        }


        private void Start()
        {
            Application.targetFrameRate = 120;
            gameStateMachine.Enter<BootstrapState>();
        }
    }
}
