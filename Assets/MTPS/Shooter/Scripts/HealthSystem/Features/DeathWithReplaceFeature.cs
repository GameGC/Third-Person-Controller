using System;
using System.Collections.Generic;
using System.Linq;
using MTPS.Core;
using MTPS.Shooter.WeaponsSystem.ShootableWeapon;
using UnityEngine;
using Object = UnityEngine.Object;

public class DeathWithReplaceFeature : BaseHealthFeature
{
    [SerializeField] private Animator deathPrefab;

    private Animator _animator;
    private Transform _transform;
    private Rigidbody _rigidbody;

    private static readonly IEnumerable<HumanBodyBones> BoneValues 
        = Enum.GetValues(typeof(HumanBodyBones)).Cast<HumanBodyBones>();

    public override void CacheReferences(IHealthVariable variables, IReferenceResolver resolver)
    {
        _animator = resolver.GetComponent<Animator>();
        _transform = resolver.GetComponent<Transform>();
        _rigidbody = resolver.GetComponent<Rigidbody>();
    }

    public override void OnHit(in float previousHealth, in float newHealth, in Vector3 hitPoint, in Vector3 hitNormal, IDamageSender damageSender)
    {
        var instance = Object.Instantiate(deathPrefab, _transform.position, _transform.rotation);
        
        var velocity = _rigidbody.velocity;
        var angularVelocity = _rigidbody.angularVelocity;

        Rigidbody tempRigidBody;
        foreach (var value in BoneValues)
        {
            if(value == HumanBodyBones.LastBone) continue;
            
            var newTransform = instance.GetBoneTransform(value);
            var oldTransform = _animator.GetBoneTransform(value);
                
            if(!oldTransform) continue;
            oldTransform.GetLocalPositionAndRotation(out var localPosition, out var localRotation);
            newTransform.SetLocalPositionAndRotation(localPosition,localRotation);
            if (newTransform.TryGetComponent(out tempRigidBody))
            {
                tempRigidBody.velocity += velocity;
                tempRigidBody.angularVelocity += angularVelocity;
            }
        }
        Destroy.Invoke();
    }
}