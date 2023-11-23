using ThirdPersonController.Core.StateMachine;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Create WeaponData", fileName = "WeaponData")]
public class WeaponData : ScriptableObject
{
    public new string name;
    public Sprite icon;
   
    [Space(9)]
    public CodeStateMachine stateMachine;
    [FormerlySerializedAs("rig")] public Rig rigLayer;

    private void OnValidate() => name = base.name;
}