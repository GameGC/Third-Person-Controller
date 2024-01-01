using ThirdPersonController.Core.StateMachine;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Create WeaponData", fileName = "WeaponData")]
public class WeaponData : BaseItemData
{
    [Space(9)]
    public CodeStateMachine stateMachine;
    [FormerlySerializedAs("rig")] public Rig rigLayer;
    
    [field:SerializeField]
    public override int maxAmmoInInventory { get; protected set; } = 1;
}