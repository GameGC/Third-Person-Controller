using System;
using System.Threading.Tasks;
using GameGC.Timeline;
using UnityEditor.Playables;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Audio;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using Object = UnityEngine.Object;

namespace DefaultNamespace
{
    public class TimelineGraph
    {
        private PlayableGraph _graph;
        public void Create(TimelineAsset asset,GameObject root,ref Playable timelinePlayable)
        {
            _graph = PlayableGraph.Create($"{asset.name}_TimelineGraph"); 
            
            int j = 1;
            for (int i = 0; i < asset.outputTrackCount; i++)
            {
                var outputTrack = asset.GetOutputTrack(i);
                if(i < 2 && outputTrack is AnimationTrack) continue;

               
                
                if(outputTrack.isEmpty) continue;

                var playable = timelinePlayable.GetInput(j);
                j++;
                
                PlayableOutput output = PlayableOutput.Null;
                switch (outputTrack)
                {
                    case AnimationTrack: output = AnimationPlayableOutput.Create(_graph, outputTrack.name, null); break;
                    case AudioTrack:     output = AudioPlayableOutput.Create(_graph, outputTrack.name, null); break;
                    case AnimationLegacyTrack: output = ScriptPlayableOutput.Create(_graph, outputTrack.name); break;
                    
                    
                    case ActivationTrack:output = ScriptPlayableOutput.Create(_graph, outputTrack.name); break;
                    case ControlTrack:   output = ScriptPlayableOutput.Create(_graph, outputTrack.name); break;
                    case MarkerTrack:    output = ScriptPlayableOutput.Create(_graph, outputTrack.name); break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(outputTrack));
                }

                output.SetSourcePlayable(playable);
            }
        }

        public void Play() => _graph.Play();
        
        public async void Stop()
        {
            while (_graph.IsDone() == false)
            {
                await Task.Delay(100);
            }
            if (_graph.IsPlaying())
                _graph.Stop();
        }

        public void Destroy() => _graph.Destroy();

        public void SetReference(string outputName, Object value)
        {
            for (int i = 0; i < _graph.GetOutputCount(); i++)
            {
                var outPut =  _graph.GetOutput(i);
                if(outPut.GetEditorName() != outputName) continue;
                
                var outPutType = outPut.GetPlayableOutputType();

                if (outPutType == typeof(AnimationPlayableOutput))
                    ((AnimationPlayableOutput) outPut).SetTarget(value as Animator);
                else if (outPutType == typeof(AudioTrack))
                    ((AudioPlayableOutput) outPut).SetTarget(value as AudioSource);
                else
                {
                    outPut.SetUserData(value);
                    outPut.SetReferenceObject(value);
                }
            }
        }
        
        public void SubscribeNotification(string outputName, INotificationReceiver receiver)
        {
            for (int i = 0; i < _graph.GetOutputCount(); i++)
            {
                var outPut =  _graph.GetOutput(i);
                if(outPut.GetEditorName() != outputName) continue;
                outPut.AddNotificationReceiver(receiver);
            }
        }
        
        public async void UnSubscribeNotification(string outputName, INotificationReceiver receiver)
        {
            while (_graph.IsPlaying()) 
                await Task.Delay(100);
            await Task.Delay(100);
            for (int i = 0; i < _graph.GetOutputCount(); i++)
            {
                var outPut =  _graph.GetOutput(i);
                if(outPut.GetEditorName() != outputName) continue;
                outPut.RemoveNotificationReceiver(receiver);
            }
        }
    }
}