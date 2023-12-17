using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Surface/Material", fileName = "SurfaceMaterial", order = 0)]
public class SurfaceMaterial : ScriptableObject
{
   [EnumArray(typeof(SurfaceHitType))][ScriptableObjectCreate]
   public List<SurfaceEffect> SurfaceEffectsForHits;
}