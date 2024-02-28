using GameGC.SurfaceSystem;
using GameGC.SurfaceSystem.Audio;
using MTPS.Core;
using MTPS.Movement.Core.Input;
using MTPS.Movement.Core.StateMachine;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace MTPS.Shooter.FightingStateMachine.Features.Surface
{
    [MovedFrom("SoundFeature")]
    public class FootSurfaceFeature : StepDecalFeature
    {
        public FootSurfaceFeature()
        {
            movementFadeMultiplier = 2;
            movementPitchMultiplier = 1.3f;
            jumpSoundDelay = 0.1f;
        }
        [Min(1)]
        public float movementFadeMultiplier;
    
        [Min(1)]
        public float movementPitchMultiplier;
   
        public SurfaceEffect defaultEffect;
        public SurfaceEffect defaultJump;
    
        public float jumpSoundDelay;
    

        private AudioSource _source;
        private IMoveInput _inputReader;
        public override void CacheReferences(IStateMachineVariables variables, IReferenceResolver resolver)
        {
            base.CacheReferences(variables,resolver);
            _source = (moveVariables as MovementStateMachineVariables).gameObject.AddComponent<AudioSource>();
        
            _inputReader = resolver.GetComponent<IMoveInput>();
        }

        public override void OnUpdateState()
        {
            if (moveVariables.IsGrounded && !moveVariables.IsSlopeBadForMove)
            {
                PlayGroundSounds();
            }
        }



        private void PlayGroundSounds()
        {
            if (_inputReader.isJump)
            {
                var hitReaction =
                    SurfaceSystem.instance.GetSurfaceHitEffect(moveVariables.GroundHit, (int) SurfaceHitType.Jump, defaultJump);
            
                SetBothCountersTo0();

                Object audio = hitReaction.Audio ? hitReaction.Audio : defaultJump.Audio;
                PlaySound(audio);
                PlaceFootDecals(hitReaction);
                ResetBothCounters();
            }
            else
            {
                if (_inputReader.moveInputMagnitude > 0)
                {
                    var hitReaction =
                        SurfaceSystem.instance.GetSurfaceHitEffect(moveVariables.GroundHit, (int) SurfaceHitType.Foot, defaultEffect);
                
                    Object audio = hitReaction.Audio ? hitReaction.Audio : defaultEffect.Audio;
                    PlaySound(audio, _inputReader.moveInputMagnitude * movementPitchMultiplier);
                    if (_inputReader.moveInputMagnitude > 0)
                        PlaceFootDecals(hitReaction);
                }
                else
                {
                    if(_source.pitch>1)
                        _source.pitch = Mathf.Clamp(_source.pitch - Time.deltaTime*movementFadeMultiplier,1,4);
                    //else
                    //{
                    //    _source.Stop();
                    //}
                
                }
            }
        }

        private void PlaySound(Object audioType,float pitch = 1)
        {
            switch (audioType)
            {
                case null: {_source.Stop(); return;}
                case AudioClip clip:
                {
                    if (!_source.isPlaying || _source.clip != clip) 
                        PlayClip(clip,pitch);
                    else
                        _source.pitch = pitch;

                    return;
                }
                case IAudioType audio:
                {
                    audio.Play(_source);
                    _source.pitch = pitch;
                    return;
                }
            }
        

        }
    
        private void PlayClip(AudioClip clip, float volume = 1f,float pitch = 1f)
        {
            if (_source.isPlaying) 
                _source.Stop();
            _source.clip = clip;
            _source.spatialBlend = 0f;
            _source.volume = volume;
            _source.pitch = pitch;
            _source.Play();

            _source.SetScheduledEndTime( AudioSettings.dspTime + 3.0F+clip.length);
        }
    }
}
