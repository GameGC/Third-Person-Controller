using System;
using System.Linq;
using Cinemachine;
using GameGC.Collections;
using ThirdPersonController.Core.DI;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

public class CameraManager : MonoBehaviour
{
    [SerializeField] private SKeyValuePair<CameraType, CinemachineVirtualCameraBase>[] cameras;

    public ReferenceResolver resolver;
    
    public void SetActiveCamera(CameraType type)
    {
        CinemachineVirtualCameraBase prevLook = null;
        CinemachineVirtualCameraBase newLook = null;
        
        foreach (var cam in cameras)
        {
            if (cam.Value.isActiveAndEnabled && type != cam.Key)
            {
                prevLook = cam.Value;
            }
            else if(!cam.Value.isActiveAndEnabled && type == cam.Key)
            {
                newLook = cam.Value;
            }
        }

        if(newLook == null || prevLook == null) return;

        CopyXAndY(newLook, prevLook);


        prevLook.gameObject.SetActive(false);
        newLook.gameObject.SetActive(true);
    }

    private void CopyXAndY(CinemachineVirtualCameraBase prevLook,CinemachineVirtualCameraBase newLook)
    {
        
        float xAxisValue = 0;
        float yAxisValue = 0;
        {
            if (prevLook is CinemachineFreeLook freeLook)
            {
                xAxisValue = freeLook.m_XAxis.Value ;
                yAxisValue = freeLook.m_YAxis.Value ;
            }
            else if (newLook is CinemachineVirtualCamera virtualC)
            {
                var pov = virtualC.GetComponentPipeline().First(c => c is CinemachinePOV) as CinemachinePOV;
                xAxisValue = pov.m_HorizontalAxis.Value;
                yAxisValue = pov.m_VerticalAxis.Value;
            }
        }
        {
            if (newLook is CinemachineFreeLook freeLook)
            {
                freeLook.m_XAxis.Value = xAxisValue;
                freeLook.m_YAxis.Value = yAxisValue;
            }
            else if (newLook is CinemachineVirtualCamera virtualC)
            {
                var pov = virtualC.GetComponentPipeline().First(c => c is CinemachinePOV) as CinemachinePOV;
                pov.m_HorizontalAxis.Value = xAxisValue;
                pov.m_VerticalAxis.Value = yAxisValue;
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
        prefab.gameObject.SetActive(false);
        
        int previousIndex = Array.FindIndex(cameras, el => el.Key == type);
        if (previousIndex > -1)
        {
            var prevLook = cameras[previousIndex].Value;

            bool wasActive = prevLook.gameObject.activeSelf;
            prevLook.gameObject.SetActive(false);

            var newLook = Instantiate(prefab, transform);

            PasteCustomTargets(newLook, follow, lookAt);

            CopyXAndY(newLook, prevLook);


            Destroy(prevLook.gameObject);
            cameras[previousIndex].Value = newLook;
            newLook.gameObject.SetActive(wasActive);
        }
        else
        {
            Array.Resize(ref cameras,cameras.Length+1);
            
            var newLook = Instantiate(prefab, transform);
            PasteCustomTargets(newLook, follow, lookAt);
            CopyXAndY(newLook, cameras.First(c=>c.Value.isActiveAndEnabled).Value);
            newLook.gameObject.SetActive(false);

            
            cameras[^1].Key = type;
            cameras[^1].Value = newLook;
        }

    }
}