using System;
using UnityEngine;


namespace PlayerInput.Interfaces
{
    public interface IInput
    {
        public event Action OnClickPerformed;
        public event Action OnClickCanceled;
        public Vector2 GetNormalizedMoveInput();
        public Vector2 GetMouseDelta();
        public Vector2 GetTouchPosition();

    }
}