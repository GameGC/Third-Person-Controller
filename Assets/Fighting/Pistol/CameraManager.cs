using System;
using GameGC.Collections;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public enum CameraType
    {
        Follow,
        Aiming,
    }

    [SerializeField] private SKeyValuePair<CameraType, GameObject>[] cameras;

    public void SetActiveCamera(CameraType type)
    {
        foreach (var cam in cameras) 
            cam.Value.SetActive(type == cam.Key);
    }
}