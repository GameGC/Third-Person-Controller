using UnityEngine;

public abstract class BaseSurfaceMaterial : ScriptableObject
{
    public abstract SurfaceEffect[] SurfaceEffectsForHits { get; }
}