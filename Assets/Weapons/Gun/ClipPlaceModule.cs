using GameGC.Collections;
using UnityEngine;
using Weapons;

public class ClipPlaceModule : MonoBehaviour
{
    public Transform clipElement;
    public Vector3 localPositionInLeftHand;
    public Quaternion localRotationInLeftHand;

    public SNullable<Pose> initialPosition;

    public void AttachToLeftHand()
    {
        clipElement.SetParent(transform.root.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.LeftHand));
        clipElement.SetLocalPositionAndRotation(localPositionInLeftHand,localRotationInLeftHand);
    }
    
    public void AttachToRightHand()
    {
        clipElement.SetParent(transform.root.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.RightHand));
        if (initialPosition.HasValue)
        {
            clipElement.SetLocalPositionAndRotation(initialPosition.Value.position,initialPosition.Value.rotation);
        }
        else
        {
            GetComponent<WeaponOffset>().Invoke("Awake",0);
        }
    }
}