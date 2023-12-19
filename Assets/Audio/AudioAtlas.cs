using UnityEngine;
using Random = UnityEngine.Random;

[CreateAssetMenu(menuName = "Audio/Audio Atlas", fileName = "AudioClipWithSettings", order = 0)]
public class AudioAtlas : ScriptableObject, IAudioType
{
    public AudioClip clip;
    public float[] timings;
        
     [MinMax(0.1f,5)] public Vector2 minMaxVolume = Vector2.one;
     [MinMax(0.1f,2)] public Vector2 minMaxPitch = Vector2.one;

    private void GetElement(ref int index, out AudioClip clip, out float start, out float end)
    {
        if (index >= timings.Length)
        {
            index = 0;
        }
        
        clip = this.clip;
        int timingsCount = timings.Length;
        bool isClipNotSplited = timingsCount < 1;
        start = index < 1? 0 : isClipNotSplited?0 : timings[index-1];
        end = isClipNotSplited ? clip.length : timingsCount -1 >index? timings[index]: clip.length;
    }

    private void GetRandomElement(out AudioClip clip, out float start, out float end)
    {
        var index = Random.Range(0,timings.Length+1);
        GetElement(ref index, out clip, out start, out end);
    }

    public float Pitch => Random.Range(minMaxPitch.x, minMaxPitch.y);
    public float Volume => Random.Range(minMaxVolume.x, minMaxVolume.y);
    
    
    public float Play(AudioSource source,bool autoStop = true)
    {
        if (!EnsureNotPlaying(source)) return 0;

        GetRandomElement(out var clip,out float start,out float end);

        source.time = start;
        source.clip = clip;
        source.volume = Volume;
        source.pitch = Pitch;
        source.Play();
        
        if (autoStop)
            ScheduleAutoStop(source, end - start);
        else
            return end - start;
        return 0;
    }
    
    public float PlaySequence(AudioSource source,ref int index,bool autoStop = true)
    {
        if (!EnsureNotPlaying(source)) return 0;
        
        
        GetElement(ref index,out var clip,out float start,out float end);
        source.time = start;
        source.clip = clip;
        source.volume = Volume;
        source.pitch = Pitch;
        source.Play();

        if (autoStop)
            ScheduleAutoStop(source, end - start);
        else
            return end - start;
        return 0;
    }

    private bool EnsureNotPlaying(AudioSource source) => clip != source.clip;

    private void ScheduleAutoStop(AudioSource source, float timer) => source.SetScheduledEndTime(AudioSettings.dspTime + 3.0F + timer);
}