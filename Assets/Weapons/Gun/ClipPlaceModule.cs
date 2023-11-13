using System;
using GameGC.Collections;
using UnityEngine;
using Weapons;

public class ClipPlaceModule : MonoBehaviour
{
    public Transform clipElement;
    public Vector3 localPositionInLeftHand;
    public Quaternion localRotationInLeftHand;

    private Pose initialPosition;


    private void Awake()
    {
        clipElement.GetLocalPositionAndRotation(out initialPosition.position,out initialPosition.rotation);
    }

    public void AttachToLeftHand()
    {
        clipElement.SetParent(transform.root.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.LeftHand));
        clipElement.SetLocalPositionAndRotation(localPositionInLeftHand,localRotationInLeftHand);
    }
    
    public void AttachToRightHand()
    {
        clipElement.SetParent(transform);
        clipElement.SetLocalPositionAndRotation(initialPosition.position,initialPosition.rotation);
    }

#if UNITY_EDITOR
    [ContextMenu("CopyVariables")]
    public void CopyVariables() => clipElement.GetLocalPositionAndRotation(out localPositionInLeftHand,out localRotationInLeftHand);

    [ContextMenu("PasteVariables")]
    public void PasteVariables() => clipElement.SetLocalPositionAndRotation(localPositionInLeftHand,localRotationInLeftHand);
#endif

}