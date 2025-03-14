using Cinemachine;
using Const;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal;


namespace Factories
{
    public class CameraCreator
    {
        public CinemachineVirtualCamera CreateVirtualCamera()
        {
            CinemachineVirtualCamera virtualCameraPrefab =
                Resources.Load<CinemachineVirtualCamera>(RuntimeConstants.PrefabPaths.VIRTUAL_CAMERA);
            
            return Object.Instantiate(virtualCameraPrefab);
        }


        public void SetUpVirtualCamera(CinemachineVirtualCamera virtualCamera, Transform cameraPivot)
        {
            virtualCamera.Follow = cameraPivot;
            virtualCamera.LookAt = cameraPivot;

            Cinemachine3rdPersonFollow thirdPersonFollow =
                virtualCamera.GetCinemachineComponent<Cinemachine3rdPersonFollow>();

            virtualCamera.m_Lens.FieldOfView = 60;

            thirdPersonFollow.ShoulderOffset = Vector3.zero;
            thirdPersonFollow.CameraDistance = 8;
        }


        public Camera CreateUICamera()
        {
            Camera uiCameraPrefab = Resources.Load<Camera>(RuntimeConstants.PrefabPaths.UI_CAMERA);
            return Object.Instantiate(uiCameraPrefab);
        }


        public void StackCamera(Camera cameraToStack)
        {
            UniversalAdditionalCameraData cameraData = Camera.main.GetUniversalAdditionalCameraData();
            cameraData.cameraStack.Add(cameraToStack);
        }
    }
}
