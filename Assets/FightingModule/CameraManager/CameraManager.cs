using System;
using System.Collections;
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
    

    private CinemachineVirtualCameraBase prevLook;
    public void SetActiveCamera(CameraType type)
    {
        CinemachineVirtualCameraBase prevLook = cameras[(int) _activeLook];
        CinemachineVirtualCameraBase newLook = cameras[(int) type];
        
        
        if(newLook == prevLook) return;

        
        CopyXAndY(prevLook, newLook);



        this.prevLook = prevLook;
        
        newLook.gameObject.SetActive(true);
        prevLook.gameObject.SetActive(false);

        _activeLook = type;
    }

    private void CopyXAndY(CinemachineVirtualCameraBase prevLook,CinemachineVirtualCameraBase newLook)
    {
        
        float xAxisValue = 0;
        float yAxisValue = 0;
        
        float xAxisInput = 0;
        float yAxisInput = 0;

        AxisState xAxis = default;
        AxisState yAxis = default;

        Transform prevLookAt = null;
        
        Pose mainPose = new Pose();
        {
            if (prevLook is CinemachineFreeLook freeLook)
            {
                xAxisValue = freeLook.m_XAxis.Value ;
                yAxisValue = freeLook.m_YAxis.Value ;
                
                xAxisInput = freeLook.m_XAxis.m_InputAxisValue;
                yAxisInput = freeLook.m_YAxis.m_InputAxisValue;

                prevLookAt = freeLook.m_LookAt;
                freeLook.transform.GetLocalPositionAndRotation(out mainPose.position,out mainPose.rotation);
            }
            else if (prevLook is CinemachineVirtualCamera virtualC)
            {
                var pov = virtualC.GetComponentPipeline().First(c => c is CinemachinePOV) as CinemachinePOV;
                xAxisValue = pov.m_HorizontalAxis.Value;
                yAxisValue = pov.m_VerticalAxis.Value;
                
                xAxisInput = pov.m_HorizontalAxis.m_InputAxisValue;
                yAxisInput = pov.m_VerticalAxis.m_InputAxisValue;
                
                prevLookAt = virtualC.m_LookAt;
                virtualC.transform.GetLocalPositionAndRotation(out mainPose.position,out mainPose.rotation);
            }
        }
        {
            if (newLook is CinemachineFreeLook freeLook)
            {
                freeLook.m_XAxis.Value = xAxisValue;
                freeLook.m_YAxis.Value = yAxisValue;

                freeLook.m_XAxis.m_InputAxisValue = xAxisInput;
                //freeLook.m_YAxis.m_InputAxisValue = yAxisInput;


                //if (_activeLook == CameraType.Aiming)
                //{
                //    freeLook.m_XAxis.Reset();
                //    freeLook.m_YAxis.Reset();
                //    StopAllCoroutines();
                //    StartCoroutine(UpdateCameraFrameLater(freeLook, FindObjectOfType<CameraTargetPlacer>().Target));
                //}

               // StartCoroutine(RecenterFreeLookCamera(freeLook));
                //StartCoroutine(UpdateCameraFrameLater(CinemachineCore.Instance.GetActiveBrain(0),mainPose));
                freeLook.transform.SetLocalPositionAndRotation(mainPose.position,mainPose.rotation);
            }
            else if (newLook is CinemachineVirtualCamera virtualC)
            {
                var pov = virtualC.GetComponentPipeline().First(c => c is CinemachinePOV) as CinemachinePOV;
                pov.m_HorizontalAxis.Value = xAxisValue;
                pov.m_VerticalAxis.Value = yAxisValue;
                
                pov.m_HorizontalAxis.m_InputAxisValue = xAxisInput;
                pov.m_VerticalAxis  .m_InputAxisValue = yAxisInput;

                StartCoroutine(UpdateCameraFrameLater(CinemachineCore.Instance.GetActiveBrain(0),mainPose));
                virtualC.transform.SetLocalPositionAndRotation(mainPose.position,mainPose.rotation);
            }
        }
        
       
    }

    private IEnumerator UpdateCameraFrameLater(CinemachineFreeLook look,Transform lookAt)
    {
        var prevValue = look.m_LookAt;
        var target = new GameObject("target").transform;
        target.position = lookAt.position - lookAt.forward * 3;
        target.rotation = lookAt.rotation;

        look.LookAt = target;

        look.PreviousStateIsValid = false;
        var brain = CinemachineCore.Instance.GetActiveBrain(0);
        yield return null;
        while (brain.IsBlending)
        {
            yield return null;
        }

        look.LookAt = prevValue;
    }
    
    /// <summary>
    /// Repositions the camera behind it's target, facing the target's forward direction
    /// </summary>
    /// <returns></returns>
    private IEnumerator RecenterFreeLookCamera(CinemachineFreeLook _thirdPersonCamera)
    {
        var previousBindingMode = _thirdPersonCamera.m_BindingMode;
        _thirdPersonCamera.m_BindingMode = CinemachineTransposer.BindingMode.LockToTargetWithWorldUp;
        _thirdPersonCamera.m_RecenterToTargetHeading.m_enabled = true;
        yield return new WaitForSeconds(_thirdPersonCamera.m_RecenterToTargetHeading.m_RecenteringTime + _thirdPersonCamera.m_RecenterToTargetHeading.m_WaitTime);
        _thirdPersonCamera.m_RecenterToTargetHeading.m_enabled = false;
        _thirdPersonCamera.m_BindingMode = previousBindingMode;
    }
    
    private IEnumerator UpdateCameraFrameLater(CinemachineBrain brain,Pose p)
    {
        yield return null;

        brain.transform.SetPositionAndRotation(p.position,p.rotation);
        
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