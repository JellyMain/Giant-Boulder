using Cinemachine;
using Const;
using Unity.VisualScripting;
using UnityEngine;


namespace Factories
{
    public class CameraCreator
    {
        public CinemachineVirtualCamera CreateVirtualCamera()
        {
            CinemachineVirtualCamera virtualCameraPrefab =
                Resources.Load<CinemachineVirtualCamera>(RuntimeConstants.PrefabPaths.CAMERA);

            CinemachineVirtualCamera virtualCamera = Object.Instantiate(virtualCameraPrefab);

            return virtualCamera;
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
    }
}