using ThirdPersonController.Core.DI;
using UnityEngine;

public interface ICollectStateMachineVariables : IStateMachineVariables
{
    public delegate void OnCollect(ItemDetailedCollect item, Transform characterPose, Transform leftHandPoint,Transform rightHandPoint);

    public bool IsCollecting { get; set; }
    
    public AnimationLayer AnimationLayer { get; }

    public OnCollect OnItemCollect { get; set; }
}