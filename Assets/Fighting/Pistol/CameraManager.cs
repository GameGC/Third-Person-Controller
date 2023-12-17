using System;
using System.Linq;
using Cinemachine;
using ThirdPersonController.Core.DI;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    /// <summary>
    /// constant size depended on CameraType enums count
    /// </summary>
    [SerializeField, EnumArray(typeof(CameraType))]
    private CinemachineVirtualCameraBase[] cameras;
    private CameraType _activeLook = CameraType.Follow;
    
    public ReferenceResolver resolver;
    
    public void SetActiveCamera(CameraType type)
    {
        CinemachineVirtualCameraBase prevLook = cameras[(int) _activeLook];
        CinemachineVirtualCameraBase newLook = cameras[(int) type];
        
        
        if(newLook == prevLook) return;

        CopyXAndY(newLook, prevLook);

        

        prevLook.gameObject.SetActive(false);
        newLook.gameObject.SetActive(true);
        _activeLook = type;
    }

    private void CopyXAndY(CinemachineVirtualCameraBase prevLook,CinemachineVirtualCameraBase newLook)
    {
        
        float xAxisValue = 0;
        float yAxisValue = 0;
        Pose state = new Pose();
        {
            if (prevLook is CinemachineFreeLook freeLook)
            {
                xAxisValue = freeLook.m_XAxis.Value ;
                yAxisValue = freeLook.m_YAxis.Value ;
                freeLook.transform.GetLocalPositionAndRotation(out state.position,out state.rotation);
            }
            else if (prevLook is CinemachineVirtualCamera virtualC)
            {
                var pov = virtualC.GetComponentPipeline().First(c => c is CinemachinePOV) as CinemachinePOV;
                xAxisValue = pov.m_HorizontalAxis.Value;
                yAxisValue = pov.m_VerticalAxis.Value;
                virtualC.transform.GetLocalPositionAndRotation(out state.position,out state.rotation);
            }
        }
        {
            if (newLook is CinemachineFreeLook freeLook)
            {
                freeLook.m_XAxis.Value = xAxisValue;
                freeLook.m_YAxis.Value = yAxisValue;
                freeLook.transform.SetLocalPositionAndRotation(state.position,state.rotation);
            }
            else if (newLook is CinemachineVirtualCamera virtualC)
            {
                var pov = virtualC.GetComponentPipeline().First(c => c is CinemachinePOV) as CinemachinePOV;
                pov.m_HorizontalAxis.Value = xAxisValue;
                pov.m_VerticalAxis.Value = yAxisValue;
                virtualC.transform.SetLocalPositionAndRotation(state.position,state.rotation);
                virtualC.transform.SetLocalPositionAndRotation(state.position,state.rotation);
            }
        }
    }

    private void PasteTargets(ICinemachineCamera newLook)
    {
        newLook.Follow = resolver.GetComponent<Transform>();
        newLook.LookAt = resolver.GetNamedComponent<Transform>("HeadLookAt");
    }
    
    private void PasteCustomTargets(ICinemachineCamera newLook,Transform follow,Transform lookAt)
    {
        newLook.Follow = follow ? follow : resolver.GetComponent<Transform>();
        newLook.LookAt = lookAt ? lookAt : resolver.GetNamedComponent<Transform>("HeadLookAt");
    }
    
    
    public void ReplaceCamera(CameraType type,CinemachineVirtualCameraBase prefab,Transform follow=null,Transform lookAt=null)
    {
        prefab.gameObject.SetActive(true);
        
        int previousIndex = (byte) type;
        if (cameras[previousIndex] != null)
        {
            var prevLook = cameras[previousIndex];

            bool wasActive = prevLook.gameObject.activeSelf;
            prevLook.gameObject.SetActive(false);

            var newLook = Instantiate(prefab, transform);
            newLook.name = prefab.name;

            PasteCustomTargets(newLook, follow, lookAt);

            Destroy(prevLook.gameObject);
            cameras[previousIndex] = newLook;

            newLook.gameObject.SetActive(wasActive);
        }
        else
        {
            var newLook = Instantiate(prefab, transform);
            newLook.name = prefab.name;
            
            PasteCustomTargets(newLook, follow, lookAt);
            newLook.gameObject.SetActive(false);
            cameras[(int) type] = newLook;
        }

    }


#if UNITY_EDITOR
    private static readonly int enumNamesSize = Enum.GetValues(typeof(CameraType)).Length;
    
    private void OnValidate()
    {
        if(cameras.Length!=enumNamesSize)
            Array.Resize(ref cameras,enumNamesSize);
    }

    private void Reset() => cameras = new CinemachineVirtualCameraBase[enumNamesSize];
#endif
}