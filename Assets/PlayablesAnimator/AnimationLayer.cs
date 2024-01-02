using System;
using System.Collections;
using System.Threading.Tasks;
using DefaultNamespace;
using ThirdPersonController.Core.StateMachine;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using Object = UnityEngine.Object;

[ExecuteAlways]
public class AnimationLayer : FollowingStateMachine<Object>
{
    #region Propertys
    public TimelineGraph CurrentGraph => _graphs[CurrentStateIndex];

    public float Weight
    {
        get => weight;
        set
        {
            weight = value;
            OnWeightChanged.Invoke(value);
        }
    }
    public AvatarMask AvatarMask
    {
        get => avatarMask;
        set
        {
            avatarMask = value;
            OnMaskChanged.Invoke(value);
        }
    }
    public bool IsAdditive
    {
        get => isAdditive;
        set
        {
            isAdditive = value;
            OnAdditiveChanged.Invoke(value);
        }
    }
    #endregion

    internal event Action<float> OnWeightChanged;
    internal event Action<AvatarMask> OnMaskChanged;
    internal event Action<bool> OnAdditiveChanged;

    [SerializeField] private float weight;
    [SerializeField] private AvatarMask avatarMask;
    [SerializeField] private bool isAdditive;

    
    
    [SerializeField] private AnimationTransition[] customTransitionTimes;
    [SerializeField] private float defaultTransitionTime = 0.02f;


    private AnimationMixerPlayable _mixerPlayable;
    private Coroutine _tempAsyncTransition;
    
    private int _tempPrevStateInd = -1;
    private int _tempCurrentStateInd = -1;

    private TimelineGraph[] _graphs;
    
    protected override void Awake()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            _codeStateMachine ??= GetComponent<CodeStateMachine>();
            _codeStateMachine.EDITOR_OnValidate += OnValidate;
            return;
        }
#endif
        base.Awake();
        _codeStateMachine.onStateChanged.AddListener(OnStateChanged);
    }
    
    public AnimationMixerPlayable ConstructPlayable(PlayableGraph graph,GameObject root)
    {
        int length = States.Length;
        
        _graphs = new TimelineGraph[length];
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
                    playable = asset.CreatePlayable(graph, root);
                    var newGraph = new TimelineGraph();
                    newGraph.Create(asset, root, ref playable);
                    _graphs[i] = newGraph;
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

    private void OnStateChanged()
    {
        CurrentGraph?.Stop();


        var prevIndex = CurrentStateIndex;
        var newIndex = Array.FindIndex(_codeStateMachine.states,s=>s.Name == _codeStateMachine.CurrentState.Name);

        CurrentStateIndex = newIndex;
        
        int transitionTimeInd = Array.FindIndex(customTransitionTimes, t => t.CouldBeApplyable(prevIndex,newIndex));

        if (defaultTransitionTime == 0 && transitionTimeInd < 0 && customTransitionTimes[transitionTimeInd].time <0.001f)
            SyncedTransition(prevIndex,newIndex);
        else
        {
            if(_tempPrevStateInd> -1)
                _mixerPlayable.SetInputWeight(_tempPrevStateInd,0);
            if(_tempCurrentStateInd> -1)
                _mixerPlayable.SetInputWeight(_tempCurrentStateInd,1);
            
            if(_tempAsyncTransition!=null)
                StopCoroutine(_tempAsyncTransition);
            _tempAsyncTransition = StartCoroutine(AsyncTransition(prevIndex, newIndex));
        }
    }

   
    #region New Awaiters

    public async Task WaitForNextState()
    {
        var newIndex = _codeStateMachine.CurrentStateIndex;
        while (Application.isPlaying && _mixerPlayable.GetInputWeight(newIndex) < 1) await Task.Delay(100);
    }

    public async Task WaitForStateWeight0(int stateIndex)
    {
        while (Application.isPlaying && _mixerPlayable.GetInputWeight(stateIndex) > 0) 
            await Task.Delay(100);
    }

    public async Task WaitForStateWeight1(int stateIndex)
    {
        while (Application.isPlaying && _mixerPlayable.GetInputWeight(stateIndex) < 1) 
            await Task.Delay(100);
    }

    public async Task WaitForFirstStateFinish() => await WaitForAnimationFinish(0);
    public async Task WaitForLastStateFinish() => await WaitForAnimationFinish(States.Length - 1);

    public async Task WaitForAnimationFinish(int stateIndex,float percent = 1)
    {
        if (_mixerPlayable.GetInputWeight(stateIndex) < 1)
            await WaitForStateWeight1(stateIndex);
        float timeScale = 1 / Time.timeScale;
        switch (States[stateIndex])
        {
            case AnimationClip clip: await Task.Delay((int) (clip.length * 1000 * percent * timeScale)); break;
            case TimelineAsset asset: await Task.Delay((int) (asset.duration * 1000 * percent * timeScale)); break;
            case AnimationValue value: await Task.Delay((int) (value.MaxLength * 1000 * percent * timeScale)); break;
        }
    }

    
    private void GetAnimationValuePlayableRemainingTime(Playable playable,out float time)
    {
        for (int i = 0; i < playable.GetInputCount(); i++)
        {
            if (playable.GetInputWeight(i) > 0.5f)
            {
                var currentPlayble = playable.GetInput(i);
                Debug.Log(currentPlayble.GetLeadTime());
                Debug.Log(currentPlayble.GetPreviousTime());

                float duration = (float) currentPlayble.GetDuration();
                if (duration == Mathf.Infinity)
                {
                    GetAnimationValuePlayableRemainingTime(currentPlayble, out time);
                    return;
                }
                    
                float currentTime = (float) currentPlayble.GetTime();
                time = duration - currentTime;
                return;
            }
        }

        time = 0;
    }
    
    #endregion
    

    #region Old Awaiters (not recomend to use)

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
    
    #endregion

    protected virtual void SyncedTransition(int previousStateIndex,int newStateIndex)
    {
        _mixerPlayable.SetInputWeight(previousStateIndex,0);
        TryExecuteAnimationValueSetWeight(previousStateIndex, 0);
        
        _mixerPlayable.SetInputWeight(newStateIndex,1);
        TryExecuteAnimationValueSetWeight(newStateIndex, 1);
    }
    protected virtual IEnumerator AsyncTransition(int previousStateIndex,int newStateIndex)
    {
        _tempPrevStateInd = previousStateIndex;
        _tempCurrentStateInd = newStateIndex;

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

                CurrentGraph.Play();
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
        TryExecuteAnimationValueSetWeight(previousStateIndex, 0);

        _mixerPlayable.SetInputWeight(newStateIndex,1);
        TryExecuteAnimationValueSetWeight(newStateIndex, 1);
        
        _tempPrevStateInd = -1;
        _tempCurrentStateInd = -1;
    }


    private void TryExecuteAnimationValueSetWeight(int index, int weight)
    {
        if (States[index] is AnimationValue value) 
            value.OnSetWeight(_mixerPlayable.GetInput(index), weight);
    }
    
    public void SetCustomVariables<T0>(int index, T0 arg0) => 
        ((AnimationValue) States[index]).SetCustomVariables(_mixerPlayable.GetInput(index),arg0);

    public void SetCustomVariables<T0, T1>(int index, T0 arg0, T1 arg1) => 
        ((AnimationValue) States[index]).SetCustomVariables(_mixerPlayable.GetInput(index),arg0,arg1);

    public void SetCustomVariables<T0, T1, T2>(int index, T0 arg0, T1 arg1, T2 arg2) => 
        ((AnimationValue) States[index]).SetCustomVariables(_mixerPlayable.GetInput(index),arg0,arg1,arg2);

    public void SetCustomVariables<T0, T1, T2, T3>(int index, T0 arg0, T1 arg1, T2 arg2, T3 arg3) =>
        ((AnimationValue) States[index]).SetCustomVariables(_mixerPlayable.GetInput(index),arg0,arg1,arg2,arg3);
    

#if UNITY_EDITOR
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
        new_.Weight = Weight;
        new_.defaultTransitionTime = defaultTransitionTime;
        new_.isAdditive = isAdditive;
    }
#endif

}