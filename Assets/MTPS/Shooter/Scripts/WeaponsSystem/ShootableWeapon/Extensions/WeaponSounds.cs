using System;
using System.Threading.Tasks;
using GameGC.CommonEditorUtils.Attributes;
using GameGC.SurfaceSystem.Audio;
using UnityEngine;
using Object = UnityEngine.Object;

[DisallowMultipleComponent]
public class WeaponSounds : BaseWeaponExtension
{
    [SerializeField,ValidateBaseType(typeof(AudioClip),typeof(IAudioType))] 
    private Object shootSound;
    
    [SerializeField,ValidateBaseType(typeof(AudioClip),typeof(IAudioType))] 
    private Object reloadSound;
    
    [Tooltip("Adapt length of clip\nto weapon reload time\nworks only for audio clip")]
    [SerializeField] private bool adaptiveSpeed;

    private AudioSource _source;
    private IWeaponInfo _info;
    private void Awake()
    {
        _source = gameObject.AddComponent<AudioSource>();
        _info = GetComponent<IWeaponInfo>();
        
        if (adaptiveSpeed)
        {
            if (reloadSound is not AudioClip) 
                throw new InvalidCastException("adaptiveSpeed works only for audio clip");
          
        }
    }
    
    public override void OnShoot() => PlaySound(shootSound);

    public override void OnBeginReload()
    {
        PlaySound(reloadSound);
        if (reloadSound && reloadSound is AudioClip clip)
        {
            if (adaptiveSpeed)
            {
                _source.pitch = _info.reloadingOrCooldownTime / clip.length;
                _source.SetScheduledEndTime(AudioSettings.dspTime + 3.0F + _info.reloadingOrCooldownTime);
            }
            else
            {
                _source.loop = true;
            }
        }
      
    }

    public override void OnEndReload()
    {
        if (!adaptiveSpeed)
        {
            _source.Stop();
            _source.loop = false;
        }
    }

    private void PlaySound(Object audioType)
    {
        switch (audioType)
        {
            case null: {_source.Stop(); return;}
            case AudioClip clip:
            {
                if (!_source.isPlaying || _source.clip != clip) 
                    PlayClip(clip);
                else if (_source.clip == clip && clip == shootSound)
                {
                    _source.timeSamples = 0;
                    if (!_source.isPlaying) 
                        _source.Play();
                }
                return;
            }
            case IAudioType audio:
            {
                audio.Play(_source);
                return;
            }
        }
    }
    
    private async void PlayClip(AudioClip clip, float volume = 1f,float pitch = 1f)
    {
        int remainingSamples = 0;
        if (_source.isPlaying)
        {
            //_source.pitch = (_source.clip.length-_source.time) / 0.1f;
            int clipSamples = _source.clip.samples;
            int times = 200;
            int sampleAdd = (clipSamples - _source.timeSamples) /times;
            remainingSamples = (clipSamples - _source.timeSamples) / times;
            if (remainingSamples > 0)
            {
                while (clipSamples -1 > _source.timeSamples)
                {
                    _source.timeSamples = Mathf.Clamp(_source.timeSamples + sampleAdd,0,clipSamples-1);
                    if(_source.timeSamples == clipSamples -1)
                        break;
                    await Task.Yield();
                }
            }
            else
            {
                remainingSamples = 0;
            }
        }
        _source.Stop();

        _source.clip = clip;
        _source.spatialBlend = 0f;
        _source.volume = volume;
        _source.pitch = pitch;
        if(remainingSamples > 0)
            _source.timeSamples = Mathf.Clamp(remainingSamples,0,_source.clip.samples-1);
        _source.Play();

        _source.SetScheduledEndTime( AudioSettings.dspTime + 3.0F+clip.length);
    }
}