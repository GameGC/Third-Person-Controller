using System.Linq;
using System.Threading.Tasks;
using GameGC.Collections;
using ThirdPersonController.Core.StateMachine;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using UnityEngine.Serialization;
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


    [FormerlySerializedAs("SDictionary")]
    public SDictionary<string, Object> States;
    public AnimationTransition defaultTransition;

    private AnimationMixerPlayable _mixerPlayable;

    public string CurrentState { get; set; }

    private CodeStateMachine _codeStateMachine;

    protected void Awake()
    {
        //CurrentState = new BaseAnimState(GetComponent<CodeAnimationStateMachine>().states.First().Name);
        CurrentState = States.First().Key;
        if (autoSyncWithOtherStateMachine)
        {
            _codeStateMachine = GetComponent<CodeStateMachine>();
            _codeStateMachine.onStateChanged.AddListener(OnStateChanged);
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
                case AnimationClip clip:            playable = AnimationClipPlayable.Create(graph, clip);break;
                case TimelineAsset asset:           playable = asset.CreatePlayable(graph, root);                break;
                case AnimationValue animationValue: playable = animationValue.GetPlayable(graph, root);          break;
            }
            graph.Connect(playable, 0, _mixerPlayable, i);
        }
        _mixerPlayable.SetInputWeight(0,1);

        return _mixerPlayable;
    }

    private void OnStateChanged()
    {
        if (defaultTransition.time == 0)
        {
            CurrentState = _codeStateMachine.CurrentState.Name;
            int i = 0;
        
            foreach (var element in States)
            {
                _mixerPlayable.SetInputWeight(i,element.Key == CurrentState?1:0);
                i++;
            }
        }
        else
            AsyncTransition();
    }


    public async Task WaitForStateWeight1(string stateName)
    {
        var newIndex=  ArrayUtility.IndexOf(States.Keys.ToArray(), stateName);
        while ( _mixerPlayable.GetInputWeight(newIndex)<1) 
            await Task.Delay(100);
    }
    
    public async Task WaitForAnimationFinish(string stateName)
    {
        await WaitForStateWeight1(stateName);
        switch (States[stateName])
        {
            case AnimationClip clip:            await Task.Delay((int)(clip.length * 1000));break;
            case TimelineAsset asset:           await Task.Delay((int)(asset.duration * 1000));break;
        }
    }
    
    public async Task WaitForStateWeight0(string stateName)
    {
        var newIndex=  ArrayUtility.IndexOf(States.Keys.ToArray(), stateName);
        while ( _mixerPlayable.GetInputWeight(newIndex)>0) 
            await Task.Delay(100);
    }
    
    private async void AsyncTransition()
    {
        var previousIndex = ArrayUtility.IndexOf(States.Keys.ToArray(), CurrentState);
        CurrentState = _codeStateMachine.CurrentState.Name;
        
        var newIndex=  ArrayUtility.IndexOf(States.Keys.ToArray(), CurrentState);
        _mixerPlayable.GetInput(newIndex).SetTime(0);
        
        float maxTimer = defaultTransition.time * 1000; 
        float timer = maxTimer;
        while (timer>0)
        {
            _mixerPlayable.SetInputWeight(previousIndex,timer/maxTimer);
            _mixerPlayable.SetInputWeight(newIndex,1-(timer/maxTimer));
            await Task.Delay(10);
            timer -= 10;
        }
        
        _mixerPlayable.SetInputWeight(previousIndex,0);
        _mixerPlayable.SetInputWeight(newIndex,1);
    }
}