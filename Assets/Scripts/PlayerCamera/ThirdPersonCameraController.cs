using PlayerInput.Interfaces;
using UI;
using UnityEngine;
using Zenject;


namespace PlayerCamera
{
    public class ThirdPersonCameraController : MonoBehaviour
    {
        [SerializeField, Range(0, 1)] private float sensitivity = 0.4f;
        [SerializeField] private float rotationSpeed = 5;
        [SerializeField] private float maxXRotation;
        [SerializeField] private Transform cameraPivot;
        public PlayerControlsUI PlayerControlsUI { get; set; }
        private Vector3 targetCameraRotation = Vector3.zero;
        private Quaternion lastRotation = Quaternion.identity;
        private IInput inputService;
        private bool canRotate;


        [Inject]
        private void Construct(IInput inputService)
        {
            this.inputService = inputService;
        }


        private void Update()
        {
            if (PlayerControlsUI.IsAreaAndClicked)
            {
                targetCameraRotation.z = 0;

                Vector2 mouseDelta = inputService.GetMouseDelta() * sensitivity;

                targetCameraRotation.x -= mouseDelta.y;
                targetCameraRotation.y += mouseDelta.x;

                if (Mathf.Abs(targetCameraRotation.x) > maxXRotation)
                {
                    targetCameraRotation.x = Mathf.Sign(targetCameraRotation.x) * maxXRotation;
                }

                cameraPivot.transform.rotation = Quaternion.Lerp(lastRotation,
                    Quaternion.Euler(targetCameraRotation), Time.deltaTime * rotationSpeed);

                lastRotation = cameraPivot.transform.rotation;
            }
            else
            {
                cameraPivot.transform.rotation = lastRotation;
                targetCameraRotation = lastRotation.eulerAngles;
            }
        }
    }
}
