using System.Threading.Tasks;
using MTPS.Inventory;
using MTPS.Movement.Core.Input;
using MTPS.Shooter.Scripts.GeneratedEnums;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public abstract class GrenadePickupHandler : MonoBehaviour
{
    [SerializeField] private AnimationLayer collectAnimLayer; // CollectLayer's AnimationLayer component
    [SerializeField] private AvatarMask leftBodyMask;
    [SerializeField] private AvatarMask rightBodyMask;

    private Animator _animator;
    private CapsuleCollider _capsuleCollider;
    private IMoveInput _input;
    private Inventory _inventory;
    private Transform _transform;
    private Rig _collectRig;

    private bool _isCollecting;

    private void Awake()
    {
        _animator        = GetComponent<Animator>();
        _capsuleCollider = GetComponent<CapsuleCollider>();
        _input           = GetComponent<IMoveInput>();
        _inventory       = GetComponent<Inventory>();
        _transform       = transform;

        // Mirror what CollectFeature does: grab the Collect rig layer from RigBuilder
        var rigBuilder = GetComponent<RigBuilder>();
        if (rigBuilder != null)
            _collectRig = rigBuilder.layers[(int)RigTypes.Collect].rig;
    }

    /// <summary>
    /// Entry point — called from ItemDetailedCollect.Collect() instead of the state machine delegate.
    /// </summary>
    public async void Collect(ItemDetailedCollect item)
    {
        if (_isCollecting) return;
        _isCollecting = true;

        // 1. Apply body mask so legs keep walking normally (same as CollectFeature.ApplyBodyMask)
        bool maskChanged = ApplyBodyMask(item.leftTarget, item.rightTarget);
        if (maskChanged)
            await Task.Delay(1000);

        // 2. Walk character to the item's stand point (same as CollectFeature: MoveToPoint)
        await _input.MoveToPoint.Invoke(new Pose(item.characterPoint.position, item.characterPoint.rotation));

        // 3. Drive anim height variable so blend tree picks the right pick-up pose
        SetItemHeight(item.leftTarget, item.rightTarget);

        // 4. Point IK constraints at the item's hand targets (same as CollectFeature: CollectRigController)
        if (_collectRig != null)
        {
            var controller = _collectRig.GetComponent<CollectRigController>();
            if (controller != null)
                controller.SetTargets(item.leftTarget, item.rightTarget);
            _collectRig.weight = 1;
        }

        // 5. Fade the collect animation layer in and wait for half the clip (same as CollectFeature)
        collectAnimLayer.Weight = 1;
        await collectAnimLayer.WaitForAnimationFinish(collectAnimLayer.CurrentStateIndex, 0.5f);

        // 6. Re-parent item to spine bone at the correct offset, disable its collider (PlaceItemInHands)
        item.GetComponent<Collider>().enabled = false;
        item.transform.SetParent(_animator.GetBoneTransform(HumanBodyBones.Spine));
        item.transform.localPosition += item.collectOffset;

        // 7. Wait for the rest of the animation to finish
        await collectAnimLayer.WaitForAnimationFinish(collectAnimLayer.CurrentStateIndex);

        // 8. Tear down — same as CollectFeature end of OnItemCollect
        collectAnimLayer.Weight = 0;
        if (_collectRig != null)
            _collectRig.weight = 0;

        item.AddItemToInventory(_inventory);
        _isCollecting = false;
    }

    // Exact copy from CollectFeature.SetItemHeight
    private void SetItemHeight(Transform leftHandPoint, Transform rightHandPoint)
    {
        float localY  = 0;
        float originY = _transform.position.y;

        if (leftHandPoint && rightHandPoint)
        {
            float localPosA = leftHandPoint.position.y  - originY;
            float localPosB = rightHandPoint.position.y - originY;
            localY = Mathf.Lerp(localPosA, localPosB, 0.5f);
        }
        else
        {
            if (leftHandPoint)  localY = leftHandPoint.position.y  - originY;
            if (rightHandPoint) localY = rightHandPoint.position.y - originY;
        }

        float height = Mathf.Clamp(localY, 0, _capsuleCollider.height);
        collectAnimLayer.SetCustomVariables(collectAnimLayer.CurrentStateIndex, height);
    }

    // Exact copy from CollectFeature.ApplyBodyMask
    // Returns true when a mask change happened and a delay is needed
    private bool ApplyBodyMask(Transform leftHandPoint, Transform rightHandPoint)
    {
        if (leftHandPoint && rightHandPoint && !collectAnimLayer.AvatarMask) return false;
        collectAnimLayer.AvatarMask = leftHandPoint ? leftBodyMask : rightBodyMask;
        return true;
    }
}