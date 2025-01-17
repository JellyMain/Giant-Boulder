using System;
using PlayerInput.Interfaces;
using UnityEngine;
using UnityEngine.InputSystem;


namespace PlayerInput.Services
{
    public class PcInput : IInput
    {
        private readonly InputActions inputActions;
        public event Action OnClickPerformed;
        public event Action OnClickCanceled;

        public PcInput()
        {
            inputActions = new InputActions();
            inputActions.PcInput.Enable();

            inputActions.PcInput.RightMouseClick.performed += ClickPerformed;
            inputActions.PcInput.RightMouseClick.canceled += ClickCanceled;
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
            return inputActions.PcInput.Move.ReadValue<Vector2>();
        }


        public Vector2 GetMouseDelta()
        {
            return inputActions.PcInput.MouseDelta.ReadValue<Vector2>();
        }
    }
}
