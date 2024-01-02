using System.Threading.Tasks;
using ThirdPersonController.Core;
using ThirdPersonController.Core.DI;
using ThirdPersonController.Input;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class CollectFeature : BaseFeature
{
    public AvatarMask leftBodyMask;
    public AvatarMask rightBodyMask;

    private static AvatarMask _fullBodyMask;

    private AnimationLayer _layer;

    private ICollectStateMachineVariables _variables;
    
    private Transform _transform;
    private CapsuleCollider _capsuleCollider;
    private Rig _rig;
    private RigBuilder _rigBuilder;

    private Animator _animator;
    private IBaseInputReader _input;

    private Inventory Switch;
    public override void CacheReferences(IStateMachineVariables variables, IReferenceResolver resolver)
    {
        if (!_fullBodyMask) 
            _fullBodyMask = new AvatarMask();
        
        _variables = variables as ICollectStateMachineVariables;
        _layer = _variables.AnimationLayer;

        _animator = resolver.GetComponent<Animator>();
        _input = resolver.GetComponent<IBaseInputReader>();
        _transform = resolver.GetComponent<Transform>();
        Switch = resolver.GetComponent<Inventory>();
        
        _capsuleCollider = resolver.GetComponent<CapsuleCollider>();
        _rig = resolver.GetComponent<RigBuilder>().layers[(int) RigTypes.Collect].rig;
    }

    public override void OnEnterState()
    {
        _variables.OnItemCollect += OnItemCollect;
    }

    private async void OnItemCollect(ItemDetailedCollect item,Transform characterPoint, Transform leftHandPoint, Transform rightHandPoint)
    {
        _variables.IsCollecting = true;
        
        bool wait = ApplyBodyMask(leftHandPoint, rightHandPoint);
        if(wait)
            await Task.Delay(1000);

        await _input.MoveToPoint.Invoke(new Pose(characterPoint.position,characterPoint.rotation));
        
        SetItemHeight(leftHandPoint, rightHandPoint);

        var controller = _rig.GetComponent<CollectRigController>();
        controller.SetTargets(leftHandPoint,rightHandPoint);
        _rig.weight = 1;

        _layer.Weight = 1;

        await _layer.WaitForAnimationFinish(_layer.CurrentStateIndex, 0.5f);
        await PlaceItemInHands(item.transform,leftHandPoint,rightHandPoint,controller);
        _layer.Weight = 0;
        _rig.weight = 0;
        
        item.AddItemToInventory(Switch);
        
        _variables.IsCollecting = false;
    }

    private void SetItemHeight(Transform leftHandPoint, Transform rightHandPoint)
    {
        //calculate item height comparing to character
        float localY = 0;
        float originY = _transform.position.y;
        if (leftHandPoint && rightHandPoint)
        {
            var localPosA = leftHandPoint.position.y - originY;
            var localPosB = rightHandPoint.position.y - originY;
            localY = Mathf.Lerp(localPosA, localPosB, 0.5f);
        }
        else
        {
            if(leftHandPoint)
                localY = leftHandPoint.position.y - originY;
            if(rightHandPoint)
                localY = rightHandPoint.position.y - originY;
        }
        
        float height = Mathf.Clamp(localY, 0, _capsuleCollider.height);
        _layer.SetCustomVariables(_layer.CurrentStateIndex,height);
    }

    private async Task PlaceItemInHands(Transform item, Transform leftHandPoint, Transform rightHandPoint,CollectRigController controller)
    {
        var rightHand = _animator.GetBoneTransform(HumanBodyBones.RightHand);
        var leftHand = _animator.GetBoneTransform(HumanBodyBones.LeftHand);
        var copy = item.eulerAngles;
        copy.y += Mathf.DeltaAngle(leftHand.eulerAngles.y, rightHand.eulerAngles.y);
        item.eulerAngles = copy;

        var bounds = item.GetComponent<Collider>().bounds;

        var point = bounds.ClosestPoint(rightHand.position);
        item.position -= point - rightHand.position;
        
        controller.SetTargets(leftHandPoint,rightHandPoint);
        
        item.SetParent(_animator.GetBoneTransform(HumanBodyBones.Spine));
        await _layer.WaitForAnimationFinish(_layer.CurrentStateIndex);
        //item.SetParent(null);
//
        //controller.SetTargets(leftHandPoint,rightHandPoint);
        //item.SetParent(rightHand);
    }
    private bool ApplyBodyMask(Transform leftHandPoint, Transform rightHandPoint)
    {
        if (leftHandPoint && rightHandPoint && !_layer.AvatarMask) return false;
        _layer.AvatarMask = leftHandPoint ? leftBodyMask : rightBodyMask;
        return true;
    }
}
