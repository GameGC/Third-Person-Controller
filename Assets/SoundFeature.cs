using ThirdPersonController.Core;
using ThirdPersonController.Core.DI;
using ThirdPersonController.Input;
using ThirdPersonController.MovementStateMachine;
using UnityEngine;

public class SoundFeature : BaseFeature
{

    public SoundFeature()
    {
        movementFadeMultiplier = 2;
        movementPitchMultiplier = 1.3f;
        jumpSoundDelay = 0.1f;
    }
    [Min(1)]
    public float movementFadeMultiplier;
    
    [Min(1)]
    public float movementPitchMultiplier;
   
    public SurfaceHitType HitType;
    
    public Object DefaultMoveSound;
    public Object JumpSound;
    public float jumpSoundDelay;
    

    private AudioSource _source;
    private IMoveStateMachineVariables _moveVariables;
    private IBaseInputReader _inputReader;
    
    public override void CacheReferences(IStateMachineVariables variables, IReferenceResolver resolver)
    {
        _moveVariables = variables as IMoveStateMachineVariables;
        _source = (_moveVariables as MovementStateMachineVariables).gameObject.AddComponent<AudioSource>();
        _inputReader = resolver.GetComponent<IBaseInputReader>();
    }

    public override void OnUpdateState()
    {
        if (_moveVariables.IsGrounded && !_moveVariables.IsSlopeBadForMove)
        {
            PlayGroundSounds();
        }
        else
        {
            _source.Stop();
        }
    }

    private void PlayGroundSounds()
    {
        if (_inputReader.isJump)
        {
            PlaySound(JumpSound);
        }
        else
        {
            if (_inputReader.moveInputMagnitude > 0)
            {
                if (!SurfaceSystem.instance.OnSurfaceHitWithoutDefault(_moveVariables.GroundHit,HitType))
                    PlaySound(DefaultMoveSound, _inputReader.moveInputMagnitude * movementPitchMultiplier);
            }
            else
            {
                if(_source.pitch>1)
                    _source.pitch = Mathf.Clamp(_source.pitch - Time.deltaTime*movementFadeMultiplier,1,4);
                else
                {
                    _source.Stop();
                }
                
            }
        }
    }

    private void PlaySound(Object audioType,float pitch = 1)
    {
        switch (audioType)
        {
            case AudioClip clip:
            {
                if (!_source.isPlaying || _source.clip != clip) 
                    PlayClip(clip);

                _source.pitch = pitch;
                break;
            }
            case IAudioType audio:
            {
                audio.Play(_source);
                _source.pitch = pitch;
                break;
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
