using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ThirdPersonController.Core;
using ThirdPersonController.Core.DI;
using UnityEngine;
using Object = UnityEngine.Object;

public class DeathFeature : BaseFeature
{
    public Animator deathPrefab;

    private Animator _animator;
    private Transform _transform;

    private static readonly IEnumerable<HumanBodyBones> BoneValues 
        = Enum.GetValues(typeof(HumanBodyBones)).Cast<HumanBodyBones>();
    public override void CacheReferences(IStateMachineVariables variables, IReferenceResolver resolver)
    {
        _animator = resolver.GetComponent<Animator>();
        _transform = resolver.GetComponent<Transform>();
    }

    public override void OnEnterState()
    {
        var instance = Object.Instantiate(deathPrefab, _transform.position, _transform.rotation);
        foreach (var value in BoneValues)
        {
            if(value == HumanBodyBones.LastBone) continue;
            
            var newTransform = instance.GetBoneTransform(value);
            var oldTransform = _animator.GetBoneTransform(value);
                
            if(!oldTransform) continue;
            oldTransform.GetLocalPositionAndRotation(out var localPosition, out var localRotation);
            newTransform.SetLocalPositionAndRotation(localPosition,localRotation);
        }
    }
}
