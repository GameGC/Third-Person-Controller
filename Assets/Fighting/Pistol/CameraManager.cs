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

    public SKeyValuePair<CameraType, GameObject>[] cameras;
}