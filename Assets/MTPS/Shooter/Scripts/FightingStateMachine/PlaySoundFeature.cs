using GameGC.CommonEditorUtils.Attributes;
using GameGC.SurfaceSystem.Audio;
using MTPS.Core;
using UnityEngine;

public class PlaySoundFeature : BaseFeature
{
    [SerializeField,ValidateBaseType(typeof(AudioClip),typeof(IAudioType))] 
    private Object sound;
    
    private AudioSource _source;

    public override void CacheReferences(IStateMachineVariables variables, IReferenceResolver resolver)
    {
        _source = (variables as Component).gameObject.AddComponent<AudioSource>();
    }

    public override void OnEnterState() => PlaySound(sound);

    public override void OnExitState()
    {
        _source.Stop();
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
