using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using DefaultNamespace;
using GameGC.Collections;
using ThirdPersonController.Core.StateMachine;
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

public class AnimationLayer : MonoBehaviour
{
    public bool autoSyncWithOtherStateMachine = true;

    public float weight;
    public AvatarMask avatarMask;
    public bool isAdditive;


    public SDictionary<string, Object> States;
    
    public AnimationTransition[] customTransitionTimes;
    public float defaultTransitionTime = 0.02f;


    private AnimationMixerPlayable _mixerPlayable;

    public string CurrentState { get; set; }

    protected CodeStateMachine _codeStateMachine;


    protected virtual void Awake()
    {
        CurrentState = States.First().Key;
        if (autoSyncWithOtherStateMachine)
        {
            _codeStateMachine = GetComponent<CodeStateMachine>();
            _codeStateMachine.onStateChanged.AddListener(OnStateChanged);
            
            //if syncing state not exist in States
            int length = _codeStateMachine.states.Length;
            if(length > States.Count)
                for (int i = 0; i < length; i++)
                {
                    if (States.ContainsKey(_codeStateMachine.states[i].Name)) continue;
                    States.Add(_codeStateMachine.states[i].Name,null);
                }
        }
    }

    public AnimationMixerPlayable ConstructPlayable(PlayableGraph graph,GameObject root)
    {
        _mixerPlayable = AnimationMixerPlayable.Create(graph, States.Count);
        for (int i = 0, length =States.Count; i < length; i++)
        {
            var element = States.ElementAt(i);
            Playable playable = default;
            switch (element.Value)
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

        var prevState = CurrentState;
        var newState = _codeStateMachine.CurrentState.Name;
        CurrentState = newState;
        
        int transitionTimeInd = Array.FindIndex(customTransitionTimes, t => t.CouldBeApplyable(prevState,newState));

        if (defaultTransitionTime == 0 && transitionTimeInd < 0 && customTransitionTimes[transitionTimeInd].time <0.001f)
            SyncedTransition(prevState,newState);
        else
        {
            if(tempPrevStateInd> -1)
                _mixerPlayable.SetInputWeight(tempPrevStateInd,0);
            if(tempCurrentStateInd> -1)
                _mixerPlayable.SetInputWeight(tempCurrentStateInd,1);
            
            if(_tempAsyncTransition!=null)
                StopCoroutine(_tempAsyncTransition);
            _tempAsyncTransition = StartCoroutine(AsyncTransition(prevState, newState));
        }
    }

    private Coroutine _tempAsyncTransition;
    private int tempPrevStateInd = -1;
    private int tempCurrentStateInd = -1;


    public async Task WaitForNextState()
    {
        var newIndex = Array.IndexOf(States.Keys.ToArray(), _codeStateMachine.CurrentState.Name);
        while (Application.isPlaying && _mixerPlayable.GetInputWeight(newIndex) < 1)
            await Task.Delay(100);
    }

    public async Task WaitForStateWeight1(string stateName)
    {
        var newIndex=  Array.IndexOf(States.Keys.ToArray(), stateName);
        while (Application.isPlaying && _mixerPlayable.GetInputWeight(newIndex)<1) 
            await Task.Delay(100);
    }
    
    public async Task WaitForAnimationFinish(string stateName)
    {
        await WaitForStateWeight1(stateName);
        float timeScale = 1 / Time.timeScale;
        switch (States[stateName])
        {
            case AnimationClip clip:            await Task.Delay((int)(clip.length * 1000 * timeScale));break;
            case TimelineAsset asset:           await Task.Delay((int)(asset.duration * 1000 *timeScale));break;
        }
    }
    
    public async Task WaitForStateWeight0(string stateName)
    {
        try
        {
            var newIndex=  Array.IndexOf(States.Keys.ToArray(), stateName);
            while ( _mixerPlayable.GetInputWeight(newIndex)>0) 
                await Task.Delay(100);
        }
        catch (Exception e)
        {
            Debug.LogError("Error at state: "+stateName);
        }
    }

    protected virtual void SyncedTransition(string previosState,string newState)
    {
        int i = 0;
        
        foreach (var element in States)
        {
            _mixerPlayable.SetInputWeight(i,element.Key == newState?1:0);
            i++;
        }
    }
    protected virtual IEnumerator AsyncTransition(string prevState,string newState)
    {
        var previousIndex = Array.IndexOf(States.Keys.ToArray(), prevState);
        var newStateIndex = Array.IndexOf(States.Keys.ToArray(), newState);

        tempPrevStateInd = previousIndex;
        tempCurrentStateInd = newStateIndex;

        try
        {
            if (States[newState] is AnimationClip clip && !clip.isLooping)
            {
                var playable = _mixerPlayable.GetInput(newStateIndex);
                if (!playable.IsNull() && playable.IsValid())
                    playable.SetTime(0);
            }
            else if(States[newState] is TimelineAsset asset)
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
            Debug.LogError(newStateIndex);
            Debug.LogError(e);
        }
        
        int transitionTimeInd =
            Array.FindIndex(customTransitionTimes, t => t.CouldBeApplyable(previousIndex, newStateIndex));

        float maxTimer =
            (transitionTimeInd < 0 ? defaultTransitionTime : customTransitionTimes[transitionTimeInd].time); 
        float timer = maxTimer;
        
        var wait = new WaitForEndOfFrame();
        while (timer > 0)
        {
            _mixerPlayable.SetInputWeight(previousIndex,timer/maxTimer);
            _mixerPlayable.SetInputWeight(newStateIndex,1-(timer/maxTimer));
            timer -= Time.deltaTime;
            yield return wait;
        }
        
        _mixerPlayable.SetInputWeight(previousIndex,0);
        _mixerPlayable.SetInputWeight(newStateIndex,1);
        
        tempPrevStateInd = -1;
        tempCurrentStateInd = -1;
    }

    
    
    
    
    private void OnValidate()
    {
        if (customTransitionTimes != null)
        {
            bool isDirty = false;
            for (var i = 0; i < customTransitionTimes.Length; i++)
            {
                if (customTransitionTimes[i].Validate(States.Keys.ToArray()))
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
        new_.autoSyncWithOtherStateMachine = autoSyncWithOtherStateMachine;
    }
}