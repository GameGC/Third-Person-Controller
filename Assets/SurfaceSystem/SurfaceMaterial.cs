using GameGC.SurfaceSystem;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Surface/Material", fileName = "SurfaceMaterial", order = 0)]
public class SurfaceMaterial : BaseSurfaceMaterial
{
   [FormerlySerializedAs("m_SurfaceEffectsForHits")]
   [FormerlySerializedAs("SurfaceEffectsForHitsOld")] [FormerlySerializedAs("SurfaceEffectsForHits")] 
   [EnumArray(typeof(SurfaceHitType))][ScriptableObjectCreate]
   [SerializeField] private SurfaceEffect[] surfaceEffectsForHits;

   public override SurfaceEffect[] SurfaceEffectsForHits => surfaceEffectsForHits;
}