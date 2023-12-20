using System.Collections.Generic;
using GameGC.Collections;
using UnityEngine;
using UnityEngine.Serialization;
using static UnityEngine.Random;

public class SurfaceSystem : Singleton<SurfaceSystem>
{
   [FormerlySerializedAs("SurfaceEffects")] 
   public SKeyValueReadonlyArray<Object, BaseSurfaceMaterial> surfaceMaterials;

   //simple caching
   private int _colliderId = int.MaxValue;
   private readonly List<Material> _lastRendererMaterials = new List<Material>();
   private Mesh _lastSharedMesh;
   private Collider _lastCollider;
   
   private bool _isSkinnedMesh;
   private Transform[] _skinnedBones;
   
   #region Default processing
   public void OnSurfaceHit(RaycastHit hit,int hitType,SurfaceEffect defaultEffect)
   {
      //simple caching
      GetRendererAndCollider(in hit);
      
      SurfaceEffect effect = null;

      if (_lastCollider is MeshCollider)
      {
         int subMeshIndex = GetSubmeshIndex(hit.triangleIndex, _lastRendererMaterials.Count, _lastSharedMesh);

         //textures support
         if (surfaceMaterials.TryGetValue(_lastRendererMaterials[subMeshIndex].mainTexture, out var material))
            effect = material.SurfaceEffectsForHits[hitType];
         
         //materials support
         else if (surfaceMaterials.TryGetValue(_lastRendererMaterials[subMeshIndex], out material))
            effect = material.SurfaceEffectsForHits[hitType];
      }

      if(!effect)
         effect = defaultEffect;


      if (effect)
      {
         var decals = effect.decalsVariant;
         if (effect.decalsVariant == null || effect.decalsVariant.Length < 1)
            decals = defaultEffect.decalsVariant;

         if (effect.decalsVariant != null && effect.decalsVariant.Length > 0)
         {
            Transform parent = hit.collider.transform;
            if (_isSkinnedMesh)
            {
               float closestDistance = int.MaxValue;
               foreach (var t in _skinnedBones)
               {
                  var newDistance = Vector3.Distance(hit.point, t.position);
                  if (closestDistance > newDistance)
                  {
                     closestDistance = newDistance;
                     parent = t;
                  }
               }
            }
            SpawnDecalList(decals, effect, hit.point, hit.normal,parent);
         }

         var audio = effect.Audio;
         if (!audio)
            audio = defaultEffect.Audio;
         if (audio) 
            PlayAudio(effect, hit.point);
      }
   }
   
   public void OnSurfaceHit(Collision collision,int hitType,SurfaceEffect defaultEffect)
   {
      var contact = collision.GetContact(0);
      collision.collider.Raycast(new Ray(contact.point, contact.normal), out var hit, contact.separation);
      OnSurfaceHit(hit,hitType,defaultEffect);
   }
   
   /// <summary>
   /// note: this method not support hit - material detections
   /// it use effect that provided
   /// used when character pushing 
   /// </summary>
   public void OnSurfaceHit(Collider hitCollider,Collider rootCollider,in Vector3 hitPoint,in Vector3 hitNormal,SurfaceEffect effect)
   {
      //simple caching
      GetRendererAndCollider(hitCollider);
      
      if (effect)
      {
         var decals = effect.decalsVariant;

         if (effect.decalsVariant != null && effect.decalsVariant.Length > 0)
         {
            Transform parent = hitCollider.transform;
            if (_isSkinnedMesh && hitCollider == rootCollider)
            {
               float closestDistance = int.MaxValue;
               foreach (var t in _skinnedBones)
               {
                  var newDistance = Vector3.Distance(hitPoint, t.position);
                  if (closestDistance > newDistance)
                  {
                     closestDistance = newDistance;
                     parent = t;
                  }
               }
            }
            SpawnDecalList(decals, effect, hitPoint,hitNormal,parent);
         }

         if (effect.Audio) 
            PlayAudio(effect, hitPoint);
      }
   }
   
   
   #endregion

   public bool OnSurfaceHitWithoutDefault(in RaycastHit hit,int hitType)
   {
      //simple caching
      GetRendererAndCollider(in hit);
      
      int subMeshIndex = GetSubmeshIndex(hit.triangleIndex,_lastRendererMaterials.Count,_lastSharedMesh);
      
      if (surfaceMaterials.TryGetValue(_lastRendererMaterials[subMeshIndex].mainTexture, out var material))
      {
         var effect = material.SurfaceEffectsForHits[hitType];
         if (effect)
         {
            bool allValid = true;
            
            if(effect.decalsVariant != null && effect.decalsVariant.Length>0)
               SpawnDecalList(effect.decalsVariant,effect, hit.point, hit.normal);
            else
               allValid = false;
            
            if (effect.Audio)
               PlayAudio(effect.Audio, hit.point);
            else
               allValid = false;

            return allValid;
         }
      }
      return false;
   }
   
   /// <returns>decal and sound</returns>
   public SurfaceEffect GetSurfaceHitEffect(in RaycastHit hit,int hitType,SurfaceEffect defaultEffect = null)
   {
      //simple caching
      GetRendererAndCollider(in hit);
      
      int subMeshIndex = GetSubmeshIndex(hit.triangleIndex,_lastRendererMaterials.Count,_lastSharedMesh);

      SurfaceEffect effect = null;

      if (surfaceMaterials.TryGetValue(_lastRendererMaterials[subMeshIndex].mainTexture, out var material))
      {
         effect = material.SurfaceEffectsForHits[hitType];
      }
      else if(surfaceMaterials.TryGetValue(_lastRendererMaterials[subMeshIndex], out material))
      {
         effect = material.SurfaceEffectsForHits[hitType];
      }

      if(!effect)
         effect = defaultEffect;

      return effect;
   }
   
   private void SpawnDecalList(GameObject[] decals,SurfaceEffect effect, in Vector3 position,in Vector3 normal,Transform parent = null)
   {
      var prefab = decals[Range(0, decals.Length)];


      var rotation =Quaternion.AngleAxis(Range(0,360), normal) * Quaternion.LookRotation(normal);
      var effectInstance = Instantiate(prefab, position + normal * effect.spawnDistance, rotation);
      
      effectInstance.transform.localScale = Vector3.one * Range(effect.minMaxRandomScale.x,effect.minMaxRandomScale.y); 
      if (parent) 
         effectInstance.transform.SetParent(parent);

      if(effect.destroyTimer.HasValue)
         Destroy(effectInstance,effect.destroyTimer.Value);
   }

   private void PlayAudio(Object audio, Vector3 position)
   {
      switch (audio)
      {
         case null:
            break;
         case AudioClip clip:
         {
            PlayClipAtPoint(audio.name,clip, position);
            break;
         }
         case IAudioType audioType:
         {
            var source = GetAudioSourceAtPoint(audio.name, position);
            float length = audioType.Play(source,false);
            if (length > 0)
               Destroy(source.gameObject, length);
            break;
         }
      }
   }

   
   private static void PlayClipAtPoint(string name,AudioClip clip, Vector3 position, float volume = 1f,float pitch = 1f)
   {
      GameObject gameObject = new GameObject(name)
      {
         transform =
         {
            position = position
         }
      };
      var audioSource = gameObject.AddComponent<AudioSource>();
      audioSource.clip = clip;
      audioSource.spatialBlend = 1f;
      audioSource.volume = volume;
      audioSource.pitch = pitch;
      audioSource.Play();
      Destroy(gameObject, clip.length);
   }

   private static AudioSource GetAudioSourceAtPoint(string name,Vector3 position)
   {
      GameObject gameObject = new GameObject(name)
      {
         transform =
         {
            position = position
         }
      };
      return gameObject.AddComponent<AudioSource>();
   }
   
   
   private static int GetSubmeshIndex(int triangleIndex,int materialsCount, Mesh mesh)
   {
      if (materialsCount < 2)
         return 0;
      
      for (int i = 0; i < mesh.subMeshCount; i++)
      {
         var subMesh = mesh.GetSubMesh(i);
         if (subMesh.firstVertex >= triangleIndex && triangleIndex < subMesh.firstVertex + subMesh.vertexCount)
            return Mathf.Clamp(i,0,materialsCount);
      }

      return 0;
   }

   /// <summary>
   /// simple caching method
   /// </summary>
   private void GetRendererAndCollider(in RaycastHit hit)
   {
      if (_colliderId == hit.colliderInstanceID) return;
      
      var hitCollider = hit.collider;
      var renderer = hitCollider.GetComponentInChildren<Renderer>();
      if (renderer is MeshRenderer mesh)
      {
         var filter = hitCollider.GetComponentInChildren<MeshFilter>();
         _lastSharedMesh = filter.sharedMesh;
         _isSkinnedMesh = false;
      }
      else if(renderer is SkinnedMeshRenderer skinnedMeshRenderer)
      {
         _lastSharedMesh = skinnedMeshRenderer.sharedMesh;
         _skinnedBones = skinnedMeshRenderer.bones;
         _isSkinnedMesh = true;
      }
      
      if(renderer)
         renderer.GetSharedMaterials(_lastRendererMaterials);
      _colliderId = hit.colliderInstanceID;
      _lastCollider = hitCollider;
   }
   
   /// <summary>
   /// simple caching method
   /// </summary>
   private void GetRendererAndCollider(Collider hitCollider)
   {
      if (_colliderId == hitCollider.GetInstanceID()) return;
      
      var renderer = hitCollider.GetComponentInChildren<Renderer>();
      if (renderer is MeshRenderer mesh)
      {
         var filter = hitCollider.GetComponentInChildren<MeshFilter>();
         _lastSharedMesh = filter.sharedMesh;
         _isSkinnedMesh = false;
      }
      else if(renderer is SkinnedMeshRenderer skinnedMeshRenderer)
      {
         _lastSharedMesh = skinnedMeshRenderer.sharedMesh;
         _skinnedBones = skinnedMeshRenderer.bones;
         _isSkinnedMesh = true;
      }
      
      if(renderer)
         renderer.GetSharedMaterials(_lastRendererMaterials);
      _colliderId = hitCollider.GetInstanceID();
      _lastCollider = hitCollider;
   }
}