using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.Serialization;

public class CollectRigController : MonoBehaviour
{
    public ChainIKConstraint leftConstraint;
    public Transform leftTarget;
    
    public ChainIKConstraint rightConstraint;
    public Transform rightTarget;

    public void SetTargets(Transform leftHand, Transform rightHand)
    {
        if (leftHand)
        {
            leftHand.GetPositionAndRotation(out var position, out var rotation);
            leftTarget.SetPositionAndRotation(position,rotation);
            leftConstraint.weight = 1;
        }
        else
            leftConstraint.weight = 0;

        if (rightHand)
        {
            rightHand.GetPositionAndRotation(out var position, out var rotation);
            rightTarget.SetPositionAndRotation(position,rotation);
            rightConstraint.weight = 1;
        }
        else
            rightConstraint.weight = 0;
            
    }
}
