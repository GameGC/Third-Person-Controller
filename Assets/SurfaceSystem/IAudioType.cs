using UnityEngine;

public interface IAudioType
{
    public float Pitch { get; }
    public float Volume { get; }

    public float Play(AudioSource source,bool autoStop = true);
}