using System;
using UnityEngine;

public class IKPassRedirectorBehavior : StateMachineBehaviour
{
    public event Action OnStateIKEvent;
    // OnStateIK is called before OnStateIK is called on any state inside this state machine
    public override void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        OnStateIKEvent?.Invoke();
    }
}
