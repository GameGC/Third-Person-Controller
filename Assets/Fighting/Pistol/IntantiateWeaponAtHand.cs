using System;
using ThirdPersonController.Core;
using ThirdPersonController.Core.DI;
using UnityEngine;
using Object = UnityEngine.Object;

[Serializable]
public class IntantiateWeaponAtHand : BaseFeature
{
    [SerializeField] private GameObject prefab;
    
    [SerializeField] private Vector3 localPosition;
    [SerializeField] private Quaternion localRotation;

    private Animator _animator;
    
    public override void CacheReferences(IStateMachineVariables variables, IReferenceResolver resolver)
    {
        _animator = resolver.GetComponent<Animator>();
    }

    public override void OnEnterState()
    {
        var instance = Object.Instantiate(prefab, _animator.GetBoneTransform(HumanBodyBones.RightHand),false).transform;
        //instance.localPosition = localPosition;
        //instance.localRotation = localRotation;
    }
}
