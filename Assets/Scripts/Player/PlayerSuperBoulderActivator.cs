using System;
using GameLoop;
using UnityEngine;
using Zenject;


namespace Player
{
    public class PlayerSuperBoulderActivator : MonoBehaviour
    {
        private RageScale rageScale;


        [Inject]
        private void Construct(RageScale rageScale)
        {
            this.rageScale = rageScale;
        }


        private void OnEnable()
        {
            rageScale.OnSuperBoulderActivated += ChangeToSuperBoulder;
            rageScale.OnSuperBoulderDiactivated += ChangeToStandardBoulder;
        }


        private void OnDisable()
        {
            rageScale.OnSuperBoulderActivated -= ChangeToSuperBoulder;
            rageScale.OnSuperBoulderDiactivated -= ChangeToStandardBoulder;
        }


        private void ChangeToSuperBoulder()
        {
            transform.localScale = Vector3.one * 3;
        }


        private void ChangeToStandardBoulder()
        {
            transform.localScale = Vector3.one;
        }
    }
}
