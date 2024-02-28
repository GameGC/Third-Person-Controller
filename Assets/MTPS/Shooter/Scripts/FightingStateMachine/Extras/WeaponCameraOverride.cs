using System;
using Cinemachine;
using GameGC.Collections;
using GameGC.CommonEditorUtils.Attributes;
using MTPS.Core;
using MTPS.Shooter.Cameras;
using UnityEngine;
using CameraType = MTPS.Shooter.Cameras.CameraType;

namespace MTPS.FightingStateMachine.Extras
{
   public class WeaponCameraOverride : MonoBehaviour
   {
      public CameraType CameraType = CameraType.Aiming;
      public GameObject cameraPrefab;
      private CameraManager _cameraManager;

      public SNullable<FollowAndLookArgs> followAndLooksArgs;

      [Serializable]
      public struct FollowAndLookArgs
      {
         [TransformToPath] public string followPath;
         [TransformToPath] public string LookPath;
      }

      private void Awake()
      {
         Transform look = null;
         Transform follow = null;

         if (followAndLooksArgs.HasValue)
         {
            look = transform.root.Find(followAndLooksArgs.Value.LookPath);
            follow = transform.root.Find(followAndLooksArgs.Value.followPath);
         }

         _cameraManager = GetComponentInParent<ReferenceResolver>().GetNamedComponent<CameraManager>("CameraManager");
         _cameraManager.ReplaceCamera(CameraType, cameraPrefab.GetComponent<CinemachineVirtualCameraBase>(), follow,
            look);
      }
   }
}