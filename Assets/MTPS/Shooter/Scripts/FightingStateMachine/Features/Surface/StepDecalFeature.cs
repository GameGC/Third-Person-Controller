using GameGC.SurfaceSystem;
using ThirdPersonController.Core;
using ThirdPersonController.Core.DI;
using ThirdPersonController.MovementStateMachine;
using UnityEngine;

public class StepDecalFeature : BaseFeature
{
    public float footStepPlaceInterval = 0.5f;
    
    protected IMoveStateMachineVariables moveVariables;

    private Animator _animator;

    private float _leftFootCounter;
    private float _rightFootCounter;

    private Transform _leftFoot;
    private Transform _rightFoot;

    private float leftFootHeight;
    private float rightFootHeight;

    private bool _placeLeftDelayed;
    private bool _placeRightDelayed;
    
    private SurfaceEffect _lastEffect;
    private Transform _transform;
    private IKPassRedirectorBehavior[] _ikPassRedirectors;

    public override void CacheReferences(IStateMachineVariables variables, IReferenceResolver resolver)
    {
        moveVariables = variables as IMoveStateMachineVariables;

        _transform = resolver.GetComponent<Transform>();
        _animator = resolver.GetComponent<Animator>();
        
        _leftFoot = _animator.GetBoneTransform(HumanBodyBones.LeftFoot);
        _rightFoot = _animator.GetBoneTransform(HumanBodyBones.RightFoot);

        leftFootHeight = _animator.leftFeetBottomHeight;
        rightFootHeight = _animator.rightFeetBottomHeight;
        
        _ikPassRedirectors = _animator.GetBehaviours<IKPassRedirectorBehavior>();
        foreach (var redirector in _ikPassRedirectors) 
            redirector.OnStateIKEvent += OnStateIK;
        ResetBothCounters();
    }

    private void OnStateIK()
    {
        var up = _transform.up;
        GameObject prefab = null;

        Vector3 position;
        Quaternion rotation;
        
        if (_placeLeftDelayed)
        {
            _placeLeftDelayed = false;

            _leftFoot.GetPositionAndRotation(out position,out rotation);
            GetRealPose(ref position, ref rotation,up,leftFootHeight, AvatarIKGoal.LeftFoot);
            
            prefab = GetRandomPrefab(_lastEffect);
            var instance = PlaceDecal(position, rotation, _lastEffect, prefab, in up);

            if (_lastEffect.destroyTimer.HasValue) 
                Object.Destroy(instance, _lastEffect.destroyTimer.Value);
        }

        if (_placeRightDelayed)
        {
            _placeRightDelayed = false;

            _rightFoot.GetPositionAndRotation(out position,out rotation);
            GetRealPose(ref position, ref rotation,up,rightFootHeight, AvatarIKGoal.RightFoot);
            
            prefab ??= GetRandomPrefab(_lastEffect);
            var instance = PlaceDecal(position,rotation, _lastEffect, prefab, in up, true);

            if (_lastEffect.destroyTimer.HasValue) 
                Object.Destroy(instance, _lastEffect.destroyTimer.Value);
        }
    }

    private void GetRealPose(ref Vector3 position, ref Quaternion rotation,Vector3 up,float footStepHeight,AvatarIKGoal goal)
    {
        position = Vector3.Lerp(position,
            _animator.GetIKPosition(goal)-up * footStepHeight,
            _animator.GetIKPositionWeight(goal));

        rotation = Quaternion.Lerp(rotation,
            _animator.GetIKRotation(goal),
            _animator.GetIKRotationWeight(goal));
    }

    protected void PlaceFootDecals(SurfaceEffect effect)
    {
        if(effect.decalsVariant == null || effect.decalsVariant.Length < 1)return;

        _lastEffect = effect;
        if (_leftFootCounter < 0.01f)
        {
            _leftFootCounter = footStepPlaceInterval;
            _placeLeftDelayed = true;
        }

        if (_rightFootCounter < 0.01f)
        {
            _rightFootCounter = footStepPlaceInterval;
            _placeRightDelayed = true;
        }

        _leftFootCounter -= Time.deltaTime;
        _rightFootCounter -= Time.deltaTime;
    }

    protected void SetBothCountersTo0()
    {
        if(_leftFootCounter > footStepPlaceInterval * 0.6666667F && _rightFootCounter > footStepPlaceInterval * 0.6666667F) return;
        _leftFootCounter = 0;
        _rightFootCounter = 0;
    }
    
    protected void ResetBothCounters()
    {
        _leftFootCounter = footStepPlaceInterval * 2;
        _rightFootCounter = footStepPlaceInterval;
    }
    

    private GameObject PlaceDecal(Vector3 position,Quaternion rotation, SurfaceEffect effect, GameObject prefab, in Vector3 up, bool invertScale = false)
    {
        position.y = moveVariables.GroundHit.point.y + effect.spawnDistance;
        rotation = Quaternion.AngleAxis(rotation.eulerAngles.y, up);

        var instance = Object.Instantiate(prefab, position, rotation).transform;

        var scale = instance.localScale;
        scale *= Random.Range(effect.minMaxRandomScale.x, effect.minMaxRandomScale.y);
        if (invertScale) scale.x *= -1;
        instance.localScale = scale;

        return instance.gameObject;
    }
    private static GameObject GetRandomPrefab(SurfaceEffect effect)
    {
        return effect.decalsVariant[Random.Range(0, effect.decalsVariant.Length)];
    }
}