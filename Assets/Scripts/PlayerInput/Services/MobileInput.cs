using System;
using PlayerInput.Interfaces;
using UnityEngine;


namespace PlayerInput.Services
{
    public class MobileInput : IInput
    {
        public event Action OnClickPerformed;
        public event Action OnClickCanceled;
        
        public MobileInput()
        {
           
        }


       


        public Vector2 GetNormalizedMoveInput()
        {
            throw new NotImplementedException();
        }


        public Vector2 GetMouseDelta()
        {
            throw new NotImplementedException();
        }
    }
}