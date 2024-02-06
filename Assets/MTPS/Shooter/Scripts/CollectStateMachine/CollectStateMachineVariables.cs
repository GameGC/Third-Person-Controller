using UnityEngine;


[DisallowMultipleComponent]
public class CollectStateMachineVariables : MonoBehaviour, ICollectStateMachineVariables
{
    public bool IsCollecting { get; set; }
    public AnimationLayer AnimationLayer { get; private set; }
    public ICollectStateMachineVariables.OnCollect OnItemCollect { get; set; }

    private void Awake() => AnimationLayer = GetComponent<AnimationLayer>();
}