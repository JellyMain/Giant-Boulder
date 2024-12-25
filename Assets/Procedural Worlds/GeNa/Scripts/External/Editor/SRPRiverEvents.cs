using UnityEngine;
using UnityEditor;
#if GeNa_HDRP
using UnityEngine.Rendering.HighDefinition;
#endif
namespace GeNa.Core
{
    [InitializeOnLoad]
    public class SRPRiverEvents
    {
        static SRPRiverEvents()
        {
            GeNaEvents.SetHDRPRiverCam = SetHDRPRiverCam;
        }
        public static bool SetHDRPRiverCam(GameObject captureCameraObj)
        {
#if GeNa_HDRP
            HDAdditionalCameraData camData = captureCameraObj.AddComponent<HDAdditionalCameraData>();
            camData.backgroundColorHDR = new Color(0f, 0f, 0f);
            camData.volumeLayerMask = 0;
#endif
            return true;
        }
    }
}