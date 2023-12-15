using System;
using System.Collections;
using System.Threading.Tasks;
using DefaultNamespace;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using Object = UnityEngine.Object;

public abstract class AnimationValue : ScriptableObject
{
    public abstract Playable GetPlayable(PlayableGraph graph,GameObject root);
}

public class AnimationLayer : FollowingStateMachine<Object>
{
    public float weight;
    public AvatarMask avatarMask;
    public bool isAdditive;


    public AnimationTransition[] customTransitionTimes;
    public float defaultTransitionTime = 0.02f;


    private AnimationMixerPlayable _mixerPlayable;

    protected override void Awake()
    {
        base.Awake();
        _codeStateMachine.onStateChanged.AddListener(OnStateChanged);
    }

    public AnimationMixerPlayable ConstructPlayable(PlayableGraph graph,GameObject root)
    {
        int length = States.Length;
        _mixerPlayable = AnimationMixerPlayable.Create(graph,length);
        for (int i = 0; i < length; i++)
        {
            Playable playable = default;
            switch (States[i])
            {
                case null: playable = Playable.Null; break;
                
                // for some un common cases when playables buggy blend
                case RuntimeAnimatorController anim:
                {
                    playable =AnimatorControllerPlayable.Create(graph,anim); break;
                }

                case AnimationClip clip:
                {
                    var tempPlayable = AnimationClipPlayable.Create(graph, clip);
                    if(avatarMask && !avatarMask.GetHumanoidBodyPartActive(AvatarMaskBodyPart.LeftFootIK) &&
                       !avatarMask.GetHumanoidBodyPartActive(AvatarMaskBodyPart.LeftFootIK))
                        tempPlayable.SetApplyFootIK(false);
                    playable = tempPlayable;break;
                }
                case TimelineAsset asset:
                {
                    playable = asset.CreatePlayable(graph, root);;
                    Graph = new TimelineGraph();
                    Graph.Create(asset, root, ref playable);
                    //playable = asset.CreatePlayable(graph, root);
                    break;
                }
                case AnimationValue animationValue: playable = animationValue.GetPlayable(graph, root);          break;
                default:playable = Playable.Null; break;
            }
            graph.Connect(playable, 0, _mixerPlayable, i);
        }
        _mixerPlayable.SetInputWeight(0,1);

        return _mixerPlayable;
    }

    public TimelineGraph Graph;
    private void OnStateChanged()
    {
        Graph?.Stop();


        var prevIndex = CurrentStateIndex;
        var newIndex = Array.FindIndex(_codeStateMachine.states,s=>s.Name == _codeStateMachine.CurrentState.Name);

        CurrentStateIndex = newIndex;
        
        int transitionTimeInd = Array.FindIndex(customTransitionTimes, t => t.CouldBeApplyable(prevIndex,newIndex));

        if (defaultTransitionTime == 0 && transitionTimeInd < 0 && customTransitionTimes[transitionTimeInd].time <0.001f)
            SyncedTransition(prevIndex,newIndex);
        else
        {
            if(tempPrevStateInd> -1)
                _mixerPlayable.SetInputWeight(tempPrevStateInd,0);
            if(tempCurrentStateInd> -1)
                _mixerPlayable.SetInputWeight(tempCurrentStateInd,1);
            
            if(_tempAsyncTransition!=null)
                StopCoroutine(_tempAsyncTransition);
            _tempAsyncTransition = StartCoroutine(AsyncTransition(prevIndex, newIndex));
        }
    }

    private Coroutine _tempAsyncTransition;
    private int tempPrevStateInd = -1;
    private int tempCurrentStateInd = -1;


    public async Task WaitForNextState()
    {
        var newIndex = Array.FindIndex(_codeStateMachine.states,s=>s.Name == _codeStateMachine.CurrentState.Name);
        while (Application.isPlaying && _mixerPlayable.GetInputWeight(newIndex) < 1)
            await Task.Delay(100);
    }

    public async Task WaitForStateWeight1(string stateName)
    {
        var newIndex = Array.FindIndex(_codeStateMachine.states,s=>s.Name == stateName);
        while (Application.isPlaying && _mixerPlayable.GetInputWeight(newIndex)<1) 
            await Task.Delay(100);
    }
    
    public async Task WaitForAnimationFinish(string stateName)
    {
        await WaitForStateWeight1(stateName);
        float timeScale = 1 / Time.timeScale;
        var newIndex = Array.FindIndex(_codeStateMachine.states,s=>s.Name == stateName);
        switch (States[newIndex])
        {
            case AnimationClip clip:            await Task.Delay((int)(clip.length * 1000 * timeScale));break;
            case TimelineAsset asset:           await Task.Delay((int)(asset.duration * 1000 *timeScale));break;
        }
    }
    
    public async Task WaitForStateWeight0(string stateName)
    {
        try
        {
            var newIndex = Array.FindIndex(_codeStateMachine.states,s=>s.Name == stateName);
            while ( _mixerPlayable.GetInputWeight(newIndex)>0) 
                await Task.Delay(100);
        }
        catch (Exception e)
        {
            Debug.LogError("Error at state: "+stateName);
        }
    }

    protected virtual void SyncedTransition(int previousStateIndex,int newStateIndex)
    {
        _mixerPlayable.SetInputWeight(previousStateIndex,0);
        _mixerPlayable.SetInputWeight(newStateIndex,1);
    }
    protected virtual IEnumerator AsyncTransition(int previousStateIndex,int newStateIndex)
    {
        tempPrevStateInd = previousStateIndex;
        tempCurrentStateInd = newStateIndex;

        try
        {
            if (States[newStateIndex] is AnimationClip clip && !clip.isLooping)
            {
                var playable = _mixerPlayable.GetInput(newStateIndex);
                if (!playable.IsNull() && playable.IsValid())
                    playable.SetTime(0);
            }
            else if(States[newStateIndex] is TimelineAsset asset)
            {
                var playable = _mixerPlayable.GetInput(newStateIndex);
                if (!playable.IsNull() && playable.IsValid())
                    playable.SetTime(0);

                Graph.Play();
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error at state: "+newStateIndex);
            Debug.LogError(e);
        }
        
        int transitionTimeInd =
            Array.FindIndex(customTransitionTimes, t => t.CouldBeApplyable(previousStateIndex, newStateIndex));

        float maxTimer =
            transitionTimeInd < 0 ? defaultTransitionTime : customTransitionTimes[transitionTimeInd].time; 
        float timer = maxTimer;
        
        var wait = new WaitForEndOfFrame();
        while (timer > 0)
        {
            _mixerPlayable.SetInputWeight(previousStateIndex,timer/maxTimer);
            _mixerPlayable.SetInputWeight(newStateIndex,1-timer/maxTimer);
            timer -= Time.deltaTime;
            yield return wait;
        }
        
        _mixerPlayable.SetInputWeight(previousStateIndex,0);
        _mixerPlayable.SetInputWeight(newStateIndex,1);
        
        tempPrevStateInd = -1;
        tempCurrentStateInd = -1;
    }


    protected override void OnValidate()
    {
        base.OnValidate();
        //base.States = states.Values.ToArray();
        //EditorUtility.SetDirty(this);
        if (customTransitionTimes != null)
        {
            bool isDirty = false;
            for (var i = 0; i < customTransitionTimes.Length; i++)
            {
                if (customTransitionTimes[i].Validate(EDITOR_statesNames))
                    isDirty = true;
            }
            if(isDirty)
                EditorUtility.SetDirty(this);
        }
    }

    [ContextMenu("ConvertToExtended")]
    public void ConvertToExtended()
    {
        var new_ = gameObject.AddComponent<ExtendedAnimationLayer>();
        new_.States = States;
        new_.avatarMask = avatarMask;
        new_.weight = weight;
        new_.defaultTransitionTime = defaultTransitionTime;
        new_.isAdditive = isAdditive;
    }
}