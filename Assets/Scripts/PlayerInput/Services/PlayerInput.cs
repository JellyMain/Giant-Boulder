using System;
using PlayerInput.Interfaces;
using UnityEngine;
using UnityEngine.InputSystem;


namespace PlayerInput.Services
{
    public class PlayerInput : IInput
    {
        private readonly InputActions inputActions;
        public event Action OnClickPerformed;
        public event Action OnClickCanceled;


        public PlayerInput()
        {
            inputActions = new InputActions();
            inputActions.Player.Enable();

            inputActions.Player.LeftMouseClick.performed += ClickPerformed;
            inputActions.Player.LeftMouseClick.canceled += ClickCanceled;
        }


        private void ClickPerformed(InputAction.CallbackContext callbackContext)
        {
            OnClickPerformed?.Invoke();
        }


        private void ClickCanceled(InputAction.CallbackContext callbackContext)
        {
            OnClickCanceled?.Invoke();
        }


        public Vector2 GetNormalizedMoveInput()
        {
            return inputActions.Player.Move.ReadValue<Vector2>();
        }


        public Vector2 GetMouseDelta()
        {
            return inputActions.Player.MouseDelta.ReadValue<Vector2>();
        }


        public Vector2 GetTouchPosition()
        {
            return inputActions.Player.TouchPosition.ReadValue<Vector2>();
        }
    }
}
