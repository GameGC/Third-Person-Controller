using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Weapons;

public class AltRigHandPlacer : MonoBehaviour
{
    public Vector3 leftHandLocalPosition;
    [QuaternionAsEuler] public Quaternion leftLocalRotation;

    public Vector3 rightHandLocalPosition;
    [QuaternionAsEuler] public Quaternion rightLocalRotation;
    
    private void Awake()
    {
        var controller = GetComponentInParent<AltRigContoller>();
        var leftHandTarget = controller.leftHand.data.target;
        var rightHandTarget = controller.rightHand.data.target;

        var thisTransform = transform;
        
        leftHandTarget.SetParent(thisTransform,false);
        leftHandTarget.SetLocalPositionAndRotation(leftHandLocalPosition,leftLocalRotation);
        
        rightHandTarget.SetParent(thisTransform,false);
        rightHandTarget.SetLocalPositionAndRotation(rightHandLocalPosition,rightLocalRotation);
    }
}
