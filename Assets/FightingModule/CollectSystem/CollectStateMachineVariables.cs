using ThirdPersonController.Core.DI;
using UnityEngine;

public class CollectStateMachineVariables : MonoBehaviour, ICollectStateMachineVariables
{
    public AnimationLayer AnimationLayer { get; private set; }
    public ICollectStateMachineVariables.OnCollect OnItemCollect { get; set; }

    private void Awake() => AnimationLayer = GetComponent<AnimationLayer>();
}

public interface ICollectStateMachineVariables : IStateMachineVariables
{
    public delegate void OnCollect(ItemDetailedCollect item, Transform characterPose, Transform leftHandPoint,Transform rightHandPoint);
        
    public AnimationLayer AnimationLayer { get; }

    public OnCollect OnItemCollect { get; set; }
}