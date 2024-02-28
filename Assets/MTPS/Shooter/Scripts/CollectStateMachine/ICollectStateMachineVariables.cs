using MTPS.Core;
using UnityEngine;

public interface ICollectStateMachineVariables : IStateMachineVariables
{
    public delegate void OnCollect(Vector3 collectOffset,ItemDetailedCollect item, Transform characterPose, Transform leftHandPoint,Transform rightHandPoint);

    public bool IsCollecting { get; set; }
    
    public AnimationLayer AnimationLayer { get; }

    public OnCollect OnItemCollect { get; set; }
}