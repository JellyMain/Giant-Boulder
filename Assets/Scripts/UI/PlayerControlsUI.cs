using System;
using PlayerInput.Interfaces;
using UnityEngine;
using Zenject;


namespace UI
{
    public class PlayerControlsUI : MonoBehaviour
    {
        [SerializeField] private RectTransform lookArea;
        private IInput inputService;
        private bool clicked;
        public bool IsAreaAndClicked { get; private set; }


        [Inject]
        private void Construct(IInput inputService)
        {
            this.inputService = inputService;
        }


        private void OnEnable()
        {
            inputService.OnClickPerformed += ToggleClicked;
            inputService.OnClickCanceled += ToggleClicked;
        }


        private void OnDisable()
        {
            inputService.OnClickPerformed -= ToggleClicked;
            inputService.OnClickCanceled -= ToggleClicked;
        }


        private void ToggleClicked()
        {
            clicked = !clicked;
        }


        private bool IsInRotationArea()
        {
            Vector2 touchPosition = inputService.GetTouchPosition();
            return RectTransformUtility.RectangleContainsScreenPoint(lookArea, touchPosition);
        }


        private void Update()
        {
            IsAreaAndClicked = clicked && IsInRotationArea();
        }
    }
}
