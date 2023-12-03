using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class SoundLayer : FollowingStateMachineManaged<SurfaceEffect.IAudioType>
{
    [field: SerializeReference]
    [field: SerializeReferenceDropdown]
    public override SurfaceEffect.IAudioType[] States { get; protected set; }

    private AudioSource _source;
    protected override void Awake()
    {
        base.Awake();
        _source = gameObject.AddComponent<AudioSource>();
        _codeStateMachine.onStateChanged.AddListener(OnStateChange);
    }

    private CancellationTokenSource _tokenSource;
    private async void OnStateChange()
    {
        _source.Stop();
        
        _tokenSource?.Cancel();
        
        var state = States[_codeStateMachine.CurrentStateIndex];
        AudioClip clip = null;
        
        switch (state)
        {
            case null:
                break;
            case SurfaceEffect.AudioAtlas audioAtlas:
            {
                audioAtlas.GetRandomElement(out clip, out var start, out var end);
                if (clip != null)
                {
                    _source.clip = clip;
                    _source.volume = audioAtlas.Volume;
                    _source.pitch = audioAtlas.Pitch;
                    _source.time = start;
                    _source.Play();
                    _tokenSource = new CancellationTokenSource();
                    await Task.Delay((int) ((end - start-0.01f) * 1000),_tokenSource.Token);
                    _source.Stop();
                }
                break;
            }
            case SurfaceEffect.AudioClips audioClips:
            {
                audioClips.GetRandomClip(out clip);
                if (clip != null)
                {
                    _source.clip = clip;
                    _source.volume = audioClips.Volume;
                    _source.pitch = audioClips.Pitch;
                    _source.Play();
                }

                break;
            }
        }
    }

    private void OnDisable()
    {
        _tokenSource?.Cancel();
    }
}
