using Assets;
using Cinemachine;
using Const;
using Cysharp.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Rendering.Universal;
using UnityEngine.ResourceManagement.AsyncOperations;


namespace Factories
{
    public class CameraCreator
    {
        private readonly AssetProvider assetProvider;


        public CameraCreator(AssetProvider assetProvider)
        {
            this.assetProvider = assetProvider;
        }


        public async UniTask<CinemachineVirtualCamera> CreateVirtualCamera()
        {
            GameObject virtualCameraPrefab =
                await assetProvider.LoadAsset<GameObject>(RuntimeConstants.PrefabAddresses.VIRTUAL_CAMERA);

            return Object.Instantiate(virtualCameraPrefab).GetComponent<CinemachineVirtualCamera>();
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


        public async UniTask<Camera> CreateUICamera()
        {
            GameObject uiCameraPrefab =
                await assetProvider.LoadAsset<GameObject>(RuntimeConstants.PrefabAddresses.UI_CAMERA);
            
            return Object.Instantiate(uiCameraPrefab).GetComponent<Camera>();
        }


        public void StackCamera(Camera cameraToStack)
        {
            UniversalAdditionalCameraData cameraData = Camera.main.GetUniversalAdditionalCameraData();
            cameraData.cameraStack.Add(cameraToStack);
        }
    }
}
