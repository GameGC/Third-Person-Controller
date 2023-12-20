using System;
using UnityEngine;
using Random = UnityEngine.Random;

[CreateAssetMenu(menuName = "Audio/Audio Clip List", fileName = "AudioClipWithSettings", order = 0)]
public class AudioClipList : ScriptableObject, IAudioType
{
    public AudioClip[] clipsVariants;

    [MinMax(0.1f,5)] public Vector2 minMaxVolume = Vector2.one;
    [MinMax(0.1f,2)] public Vector2 minMaxPitch = Vector2.one;
   
    private void GetRandomClip(out AudioClip clip) => clip = clipsVariants[Random.Range(0, clipsVariants.Length)];

    //for sequence play
    private void GetClipByIndex(ref int index,out AudioClip clip)
    {
        if (index >= clipsVariants.Length)
        {
            index = 0;
        }

        clip = clipsVariants[index];
    }

    public float Pitch => Random.Range(minMaxPitch.x, minMaxPitch.y);
    public float Volume => Random.Range(minMaxVolume.x, minMaxVolume.y);
    
    public float Play(AudioSource source,bool autoStop = true)
    {
        if (!EnsureNotPlaying(source)) return 0;

        GetRandomClip(out var clip);
        source.clip = clip;
        source.volume = Volume;
        source.pitch = Pitch;
        source.Play();

        if (autoStop)
            ScheduleAutoStop(source, clip.length);
        else
            return clip.length;

        return 0;
    }
    
    public float PlaySequence(AudioSource source,ref int index,bool autoStop = true)
    {
        if (!EnsureNotPlaying(source)) return 0;
        
        
        GetRandomClip(out var clip);
        source.clip = clip;
        source.volume = Volume;
        source.pitch = Pitch;
        source.Play();
        
        if (autoStop)
            ScheduleAutoStop(source, clip.length);
        else
            return clip.length;

        return 0;
    }

    private bool EnsureNotPlaying(AudioSource source)
    {
        if (source.isPlaying)
        {
            if (Array.IndexOf(clipsVariants,source.clip) > -1)
                return false;
        }
        return true;
    }

    private void ScheduleAutoStop(AudioSource source, float timer) => source.SetScheduledEndTime(AudioSettings.dspTime + 3.0F + timer);
}