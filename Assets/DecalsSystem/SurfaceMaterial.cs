using System.Collections;
using System.Collections.Generic;
using GameGC.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Surface/Material", fileName = "SurfaceMaterial", order = 0)]
public class SurfaceMaterial : ScriptableObject
{
   [EnumedArray(typeof(SurfaceHitType))]
   public List<SurfaceEffect> SurfaceEffectsForHits;
}
