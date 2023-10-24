using System;
using UnityEngine;

public class AnimationEventReceiver : MonoBehaviour
{
    public event Action<AnimationEvent> OnEventReceived;
    
    public void OnAnimationEvent(AnimationEvent event_ )
    {
        OnEventReceived.Invoke(event_);
    }
}
