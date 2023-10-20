using System;
using System.Linq;
using Cinemachine;
using GameGC.Collections;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [SerializeField] private SKeyValuePair<CameraType, GameObject>[] cameras;

    public void SetActiveCamera(CameraType type)
    {
        CinemachineFreeLook prevLook = null;
        CinemachineFreeLook newLook = null;
        
        foreach (var cam in cameras)
        {
            if (cam.Value.activeSelf && type != cam.Key)
            {
                prevLook = cam.Value.GetComponent<CinemachineFreeLook>();
            }
            else if(!cam.Value.activeSelf && type == cam.Key)
            {
                newLook = cam.Value.GetComponent<CinemachineFreeLook>();
            }
        }

        if(newLook == null || prevLook == null) return;
        newLook.m_XAxis.Value = prevLook.m_XAxis.Value;
        newLook.m_YAxis.Value = prevLook.m_YAxis.Value;
        
        prevLook.gameObject.SetActive(false);
        newLook.gameObject.SetActive(true);
    }

    public void ReplaceCamera(CameraType type,GameObject prefab)
    {
        int previousIndex = Array.FindIndex(cameras, el => el.Key == type);
        CinemachineFreeLook prevLook = cameras[previousIndex].Value.GetComponent<CinemachineFreeLook>();
        
        bool wasActive = prevLook.gameObject.activeSelf;
        prevLook.gameObject.SetActive(false);
        
        CinemachineFreeLook newLook = Instantiate(prefab,transform).GetComponent<CinemachineFreeLook>();
        newLook.m_Follow = prevLook.m_Follow;
        newLook.m_LookAt = prevLook.m_LookAt;
        
        newLook.m_XAxis.Value = prevLook.m_XAxis.Value;
        newLook.m_YAxis.Value = prevLook.m_YAxis.Value;
        
        Destroy(prevLook.gameObject);
        cameras[previousIndex].Value = newLook.gameObject;
        newLook.gameObject.SetActive(wasActive);
    }
}