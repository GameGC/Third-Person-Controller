using System;
using Cinemachine;
using GameGC.Collections;
using ThirdPersonController.Core.DI;
using UnityEditor;
using UnityEngine;

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
      _cameraManager.ReplaceCamera(CameraType, cameraPrefab.GetComponent<CinemachineVirtualCameraBase>(),follow,look);
   }
}

[AttributeUsage(AttributeTargets.Property|AttributeTargets.Field)]
public class TransformToPathAttribute : PropertyAttribute
{
   public Type rootType = typeof(Transform);
   public TransformToPathAttribute()
   {
      
   }
   
   public TransformToPathAttribute(Type rootType)
   {
      this.rootType = rootType;
   }
}

[CustomPropertyDrawer(typeof(TransformToPathAttribute))]
public class TransformToPathDrawer : PropertyDrawerWithCustomData<TransformToPathDrawer.Data>
{
   public class Data
   {
      public bool inInited;
      public Transform root;
      public Transform target;
   }

   protected override void OnGUI(Rect position, SerializedProperty property, GUIContent label, Data customData)
   {
      var target = property.serializedObject.targetObject as MonoBehaviour;

      if (PrefabUtility.IsPartOfPrefabInstance(target.gameObject))
      {
         if (!customData.inInited)
         {
            var attribute = this.attribute as TransformToPathAttribute;

            if (attribute.rootType == typeof(Transform))
            {
               customData.root = target.transform.root;
            }
            else
            {
               customData.root = target.GetComponentInParent(attribute.rootType).transform;
            }

            customData.target = customData.root.Find(property.stringValue);
            customData.inInited = true;
         }

         using (var changeCheckScope = new EditorGUI.ChangeCheckScope())
         {
            var newTrnasform =
               EditorGUI.ObjectField(position, label, customData.target, typeof(Transform)) as Transform;
            if (changeCheckScope.changed)
            {
               property.stringValue = AnimationUtility.CalculateTransformPath(newTrnasform, customData.root);
               customData.target = newTrnasform;
               property.serializedObject.ApplyModifiedProperties();
            }
         }
      }
      else
      {
         EditorGUI.PropertyField(position, property, label);
         property.serializedObject.ApplyModifiedProperties();
      }
   }
}

