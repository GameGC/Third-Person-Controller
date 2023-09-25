using System;
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
            cam.Value.SetActive(type == cam.Key);
        }

        if(newLook == null || prevLook == null) return;
        newLook.m_XAxis.Value = prevLook.m_XAxis.Value;
        newLook.m_YAxis.Value = prevLook.m_YAxis.Value;
    }
}