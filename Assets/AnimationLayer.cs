using System.Linq;
using System.Threading.Tasks;
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
    public bool autoSyncWithOtherStateMachine;

    public float weight;
    public AvatarMask avatarMask;
    public bool isAdditive;


    public SDictionary<string, Object> SDictionary;

    public AnimationTransition defaultTransition;

    private AnimationMixerPlayable _mixerPlayable;

    public string CurrentState;

   // private void OnValidate()
   // {
   //     SDictionary = new SDictionary<string, AnimationValue>(new Dictionary<string, AnimationValue>()
   //     {
   //         {"clip",ScriptableObject.CreateInstance<ClipValue>()},
   //         {"timeline",ScriptableObject.CreateInstance<TimelineValue>()}
   //     });
   // }


    protected void Awake()
    {
        //CurrentState = new BaseAnimState(GetComponent<CodeAnimationStateMachine>().states.First().Name);
        CurrentState = SDictionary.First().Key;
        GetComponent<CodeStateMachine>().onStateChanged.AddListener(OnStateChanged);
    }

    public AnimationMixerPlayable ConstructPlayable(PlayableGraph graph,GameObject root)
    {
        _mixerPlayable = AnimationMixerPlayable.Create(graph, SDictionary.Count);
        for (int i = 0, length =SDictionary.Count; i < length; i++)
        {
            var element = SDictionary.ElementAt(i);
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
            CurrentState = GetComponent<CodeStateMachine>().CurrentState.Name;
            int i = 0;
        
            foreach (var element in SDictionary)
            {
                _mixerPlayable.SetInputWeight(i,element.Key == CurrentState?1:0);
                i++;
            }
        }
        else
        {
            AsyncTransition();
        }
        
        //for (int i = 0, length = SDictionary.Count; i < length; i++)
        //{
        //    _mixerPlayable.SetInputWeight(i,SDictionary[i].Name == CurrentState.Name?1:0);
        //}
    }

    private async void AsyncTransition()
    {
        var previousIndex = ArrayUtility.IndexOf(SDictionary.Keys.ToArray(), CurrentState);
        CurrentState = GetComponent<CodeStateMachine>().CurrentState.Name;
        
        var newIndex=  ArrayUtility.IndexOf(SDictionary.Keys.ToArray(), CurrentState);

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