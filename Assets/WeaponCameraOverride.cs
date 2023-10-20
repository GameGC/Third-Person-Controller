using System;
using System.Collections;
using System.Collections.Generic;
using ThirdPersonController.Core.DI;
using UnityEngine;

public class WeaponCameraOverride : MonoBehaviour
{
   public GameObject cameraPrefab;
   
   private CameraManager _cameraManager;
   private void Awake()
   {
      _cameraManager = GetComponentInParent<ReferenceResolver>().GetNamedComponent<CameraManager>("CameraManager");
      _cameraManager.ReplaceCamera(CameraType.Aiming, cameraPrefab);
      //_cameraManager.SetActiveCamera();
   }
}
