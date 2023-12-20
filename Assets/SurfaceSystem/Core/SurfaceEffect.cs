using GameGC.Collections;
using UnityEngine;
using Object = UnityEngine.Object;

[CreateAssetMenu(menuName = "Surface/Effect", fileName = "SurfaceEffect", order = 0)]
public class SurfaceEffect : ScriptableObject
{
    [Header("Decals & Effects")]
    public GameObject[] decalsVariant;

    [MinMax(0.1f,2)]
    public Vector2 minMaxRandomScale = Vector2.one;
    public float spawnDistance = 0.01f;
    public SNullable<float> destroyTimer =10;

    [Header("Audio")]
    [ValidateBaseType(typeof(AudioClip),typeof(IAudioType))]
     [ScriptableObjectCreate(typeof(IAudioType))]
    public Object Audio;

}