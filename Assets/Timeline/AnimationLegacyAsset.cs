using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace GameGC.Timeline
{
    [Serializable]
    public class AnimationLegacyAsset : PlayableAsset,ITimelineClipAsset
    {
        public AnimationClip clip;
        [SerializeField] private AnimationPlayableAsset.LoopMode m_Loop = AnimationPlayableAsset.LoopMode.UseSourceAsset;

        private ScriptPlayable<AnimationLegacyPlayable> playable;
        public override Playable CreatePlayable (PlayableGraph graph, GameObject owner)
        {
            playable = ScriptPlayable<AnimationLegacyPlayable>.Create(graph);

            var lightControlBehaviour = playable.GetBehaviour();
            lightControlBehaviour.clip = clip;
        
            lightControlBehaviour.easeInDuration = easeInDuration;
            lightControlBehaviour.easeOutStartTime = easeOutStartTime;
        
            return playable;
        }

        private void OnValidate()
        {
            if (!playable.IsNull() && playable.IsValid())
            {
                var legacyAnimationBehavior = playable.GetBehaviour();
                legacyAnimationBehavior.clip = clip;

                legacyAnimationBehavior.easeInDuration = easeInDuration;
                legacyAnimationBehavior.easeOutStartTime = easeOutStartTime;
            }
        }

        public ClipCaps clipCaps => ClipCaps.All;

        public override double duration => clip? clip.length:base.duration;

        public float easeInDuration{ get; set; }
        public bool isBlendIn { get; set; }
        public float easeOutStartTime{ get; set; }
        public bool isBlendOut{ get; set; }
    }
}