using PlayerInput.Interfaces;
using UnityEngine;
using Zenject;


namespace PlayerCamera
{
    public class ThirdPersonCameraController : MonoBehaviour
    {
        [SerializeField] private float rotationSpeed = 5;
        [SerializeField] private float maxXRotation;
        [SerializeField] private Transform cameraPivot;
        public RectTransform lookArea;
        private Vector3 lastCameraRotation = Vector3.zero;
        private IInput inputService;
        private bool canRotate;


        [Inject]
        private void Construct(IInput inputService)
        {
            this.inputService = inputService;
        }


        private void OnEnable()
        {
            inputService.OnClickPerformed += ToggleCanRotate;
            inputService.OnClickCanceled += ToggleCanRotate;
        }


        private void OnDisable()
        {
            inputService.OnClickPerformed -= ToggleCanRotate;
            inputService.OnClickCanceled -= ToggleCanRotate;
        }


        private void ToggleCanRotate()
        {
            canRotate = !canRotate;
        }


        private bool IsInRotationArea()
        {
            Vector2 touchPosition = inputService.GetTouchPosition();
            return RectTransformUtility.RectangleContainsScreenPoint(lookArea, touchPosition);
        }


        private void Update()
        {
            if (canRotate)
            {
                if (IsInRotationArea())
                {
                    lastCameraRotation.z = 0;

                    Vector2 mouseDelta = inputService.GetMouseDelta();

                    lastCameraRotation.x -= mouseDelta.y;
                    lastCameraRotation.y += mouseDelta.x;

                    if (lastCameraRotation.x > maxXRotation)
                    {
                        lastCameraRotation.x = maxXRotation;
                    }
                }
            }

            cameraPivot.transform.rotation = Quaternion.Euler(lastCameraRotation);
        }
    }
}
