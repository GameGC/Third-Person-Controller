using System;
using ThirdPersonController.Code.AnimatedStateMachine;
using ThirdPersonController.Core;
using ThirdPersonController.Core.DI;
using UnityEngine;
using Object = UnityEngine.Object;

[Serializable]
public class IntantiateWeaponAtHand : BaseFeature
{
    [SerializeField] private GameObject prefab;
    
   // [SerializeField] private Vector3 localPosition;
   // [SerializeField] private Quaternion localRotation;

    private Animator _animator;
    private IFightingStateMachineVariables _variables;
    
    public override void CacheReferences(IStateMachineVariables variables, IReferenceResolver resolver)
    {
        _animator = resolver.GetComponent<Animator>();
        _variables = variables as IFightingStateMachineVariables;
    }

    public override void OnEnterState()
    {
        var instance = Object.Instantiate(prefab, _animator.GetBoneTransform(HumanBodyBones.RightHand),false);
        _variables.weaponInstance = instance;
        //instance.localPosition = localPosition;
        //instance.localRotation = localRotation;
    }
}
