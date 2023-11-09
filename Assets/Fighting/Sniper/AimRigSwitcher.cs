using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using static UnityEngine.Animations.Rigging.MultiAimConstraintData;

public class AimRigSwitcher : MonoBehaviour
{
   public BlendConstraint Constraint;
   public MultiAimConstraint AimConstraint;
   private CinemachineFreeLook _look;
   
   public Transform source;

   private float weight;
   private float angleBetween;

   private void Awake()
   {

   }
   
   static Vector3 Convert(Axis axis)
   {
      switch (axis)
      {
         case Axis.X:
            return Vector3.right;
         case Axis.X_NEG:
            return Vector3.left;
         case Axis.Y:
            return Vector3.up;
         case Axis.Y_NEG:
            return Vector3.down;
         case Axis.Z:
            return Vector3.forward;
         case Axis.Z_NEG:
            return Vector3.back;
         default:
            return Vector3.up;
      }
   }

   private void Update()
   {
      _look = FindObjectOfType<CinemachineFreeLook>(false);
      float weaight = 1 - _look.m_YAxis.Value - 0.5f;
     // angleBetween=  Mathf.DeltaAngle(_look.m_YAxis.m_MaxValue ,_look.m_YAxis.m_MinValue) /;

      //float value = angleBetween / _look.m_YAxis.Value;
      Constraint.data.positionWeight =weaight *2 ;
      Constraint.data.rotationWeight =weaight * 2;
   }
}
