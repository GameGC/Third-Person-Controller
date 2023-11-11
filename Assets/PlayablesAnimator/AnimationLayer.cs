using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using DefaultNamespace;
using Fighting.Pushing;
using GameGC.Collections;
using ThirdPersonController.Core.StateMachine;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Audio;
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


    public SDictionary<string, Object> States;
    public AnimationTransition defaultTransition;

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
                case AnimationClip clip:            playable = AnimationClipPlayable.Create(graph, clip);break;
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
        Graph.Stop();
        //for (int i = 1; i <  _mixerPlayable.GetGraph().GetOutputCount(); i++)
        //{
        //    _mixerPlayable.GetGraph().DestroyOutput(_mixerPlayable.GetGraph().GetOutput(i));
        //}
        
        if (defaultTransition.time == 0)
            SyncedTransition();
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
        try
        {
            var newIndex=  ArrayUtility.IndexOf(States.Keys.ToArray(), stateName);
            while ( _mixerPlayable.GetInputWeight(newIndex)>0) 
                await Task.Delay(100);
        }
        catch (Exception e)
        {
            Debug.LogError("Error at state: "+stateName);
        }
    }

    protected virtual void SyncedTransition()
    {
        CurrentState = _codeStateMachine.CurrentState.Name;
        int i = 0;
        
        foreach (var element in States)
        {
            _mixerPlayable.SetInputWeight(i,element.Key == CurrentState?1:0);
            i++;
        }
    }
    protected virtual async void AsyncTransition()
    {
        var previousIndex = ArrayUtility.IndexOf(States.Keys.ToArray(), CurrentState);
        CurrentState = _codeStateMachine.CurrentState.Name;

        int newIndex = -1;
        try
        {
            newIndex=  ArrayUtility.IndexOf(States.Keys.ToArray(), CurrentState);
            if (States[CurrentState] is AnimationClip clip && !clip.isLooping)
            {
                var playable = _mixerPlayable.GetInput(newIndex);
                if (!playable.IsNull() && playable.IsValid())
                    playable.SetTime(0);
            }
            else if(States[CurrentState] is TimelineAsset asset)
            {
                var playable = _mixerPlayable.GetInput(newIndex);
                if (!playable.IsNull() && playable.IsValid())
                    playable.SetTime(0);

                Graph.Play();
                /*
                int index = 0;
                bool scriptCreated = false;
                var graph = _mixerPlayable.GetGraph();
                foreach (var outputTrack in asset.GetOutputTracks())
                {
                    if (index < 1)
                    {
                        index++;
                        continue;
                    }
                    
                    switch (outputTrack)
                    {
                        case ActivationTrack activationTrack:ScriptPlayableOutput.Create(graph, outputTrack.name); break;
                        case AnimationTrack animationTrack: AnimationPlayableOutput.Create(graph, outputTrack.name, null); break;
                        case AudioTrack audioTrack: AudioPlayableOutput.Create(graph, outputTrack.name, null); break;
                        case ControlTrack controlTrack: ScriptPlayableOutput.Create(graph, outputTrack.name); break;
                        case SignalTrack signalTrack: ScriptPlayableOutput.Create(graph, outputTrack.name); break;
                        case MarkerTrack markerTrack: ScriptPlayableOutput.Create(graph, outputTrack.name); break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(outputTrack));
                    }

                    index++;
                }
                
                var script = (ScriptPlayableOutput) playable.GetGraph().GetOutput(1);
                
                var receiver = GetComponent<FightingStateMachineVariables>().weaponInstance
                    .GetComponent<SignalReceiver>();
                script.AddNotificationReceiver(receiver);

                Debug.Log(receiver);
                
                script.SetSourcePlayable(playable);

              */

                
         
          //     var script = (ScriptPlayableOutput) playable.GetGraph().GetOutput(1);
          //     script.GetSourcePlayable().SetInputWeight(0,1);
          //     script.GetSourcePlayable().Pause();
          //     script.GetSourcePlayable().SetTime(0);
          //     script.GetSourcePlayable().Play();
          //     
          //    // (playable.GetOutput(1).GetOutput(0) as ScriptPlayable<TimeNotificationBehaviour>).GetBehaviour().
                
                //script.SetReferenceObject(GetComponent<FightingStateMachineVariables>().weaponInstance.GetComponent<SignalReceiver>());
                //script.SetUserData(GetComponent<FightingStateMachineVariables>().weaponInstance.GetComponent<SignalReceiver>());

            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error at state: "+CurrentState);
            Debug.LogError(newIndex);
            Debug.LogError(e);
        }
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
        
        if(States[CurrentState] is TimelineAsset a)
        {
            var playable = _mixerPlayable.GetInput(newIndex);
            if (!playable.IsNull() && playable.IsValid())
                playable.SetTime(0);
       //        
       //    for (int j = 0; j < playable.GetGraph().GetOutputCount(); j++)
       //    {
       //        // (asset.GetOutputTrack(0) as SignalTrack).
       //        //   playable.GetGraph().GetOutput(j).
       //        var ouput = playable.GetGraph().GetOutput(j);
       //        Debug.Log(j+" "+ playable.GetGraph().GetOutput(j).GetPlayableOutputType());
       //    }
        }
    }


    [ContextMenu("ConvertToExtended")]
    public void ConvertToExtended()
    {
        var new_ = gameObject.AddComponent<ExtendedAnimationLayer>();
        new_.States = States;
        new_.avatarMask = avatarMask;
        new_.weight = weight;
        new_.defaultTransition = defaultTransition;
        new_.isAdditive = isAdditive;
        new_.autoSyncWithOtherStateMachine = autoSyncWithOtherStateMachine;
    }
}