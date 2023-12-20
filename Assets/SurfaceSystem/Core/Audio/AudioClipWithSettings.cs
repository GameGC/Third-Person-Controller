using UnityEngine;

[CreateAssetMenu(menuName = "Audio/Audio Clip With Settings", fileName = "AudioClipWithSettings", order = 0)]
public class AudioClipWithSettings : ScriptableObject ,IAudioType
{
    public AudioClip Clip;
    [field:SerializeField][field:Range(0.1f,2)]
    public float Pitch { get; }
    [field:SerializeField][field:Range(0.1f,5)]
    public float Volume { get; }

    public float Play(AudioSource source, bool autoStop = true)
    {
        if(source.isPlaying && source.clip  == Clip) return 0;
        
        source.clip = Clip;
        source.volume = Volume;
        source.pitch = Pitch;
        source.Play();

        if (autoStop)
            ScheduleAutoStop(source, Clip.length);
        else
            return Clip.length;
        return 0;
    }
    
    private void ScheduleAutoStop(AudioSource source, float timer) => source.SetScheduledEndTime(AudioSettings.dspTime + 3.0F + timer);
}