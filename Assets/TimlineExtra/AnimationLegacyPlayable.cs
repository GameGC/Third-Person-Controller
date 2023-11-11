using System;
using UnityEngine;
using UnityEngine.Playables;

[Serializable]
public class AnimationLegacyPlayable : PlayableBehaviour
{ 
    public AnimationClip clip;

    public float easeInDuration;
    public float easeOutStartTime;
    
    public bool isBlendIn;
    public bool isBlendOut;
    
    private Animation _component;

    private bool ValidateCanPlay( FrameData info)
    {
        if(!Application.isPlaying) return false;
        return _component && clip && info.evaluationType == FrameData.EvaluationType.Playback;
    }
    
    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
        _component = info.output.GetUserData() as Animation;
        
        Debug.Log(info.evaluationType);
        if (ValidateCanPlay(info))
        {
            if (!_component.enabled)
                _component.enabled = true;
            
            var tempClip = _component.GetClip(clip.name);
            if (tempClip != clip)
            {
                if(tempClip) 
                    _component.RemoveClip(tempClip.name);
                _component.AddClip(clip,clip.name);
            }
            
            float speed = (float)(playable.GetDuration() /(double)clip.length);
            if (Math.Abs(speed - 1f) > 0.01f) 
                _component[clip.name].speed = speed;
            
            if (easeInDuration > 0)
            {
                if(isBlendIn)
                    _component.Blend(clip.name,1,easeInDuration);
                else 
                    _component.CrossFade(clip.name,easeInDuration);
            }
            else
            {
                _component.Play(clip.name);
            }
        }
    }
    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
#if UNITY_EDITOR
        if (!ValidateCanPlay(info))
        {
            if(_component)
                clip.SampleAnimation(_component.gameObject, (float) playable.GetTime());
        }
        else
#endif
        if(playable.GetTime()>easeOutStartTime-0.01f){
            _component.Blend(clip.name,0,(float)playable.GetDuration()-easeOutStartTime);
        }
    }

    public override void OnBehaviourPause(Playable playable, FrameData info)
    {
        if(ValidateCanPlay(info))
            _component.Stop(clip.name);
        else
        {
            //  m_Clip.SampleAnimation(anim.gameObject,-1);
        }
    }

    public override void OnGraphStop(Playable playable)
    { 
        if(_component && clip)
            _component.Stop(clip.name);
    }
}