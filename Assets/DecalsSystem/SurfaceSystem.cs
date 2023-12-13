using System.Collections.Generic;
using GameGC.Collections;
using UnityEngine;
using UnityEngine.Serialization;
using static UnityEngine.Random;

public class SurfaceSystem : Singleton<SurfaceSystem>
{
   [FormerlySerializedAs("SurfaceEffects")] 
   public SKeyValueReadonlyArray<Texture, SurfaceMaterial> surfaceMaterials;

   //simple caching
   private int _colliderId = int.MaxValue;
   private readonly List<Material> _lastRendererMaterials = new List<Material>();
   private Mesh _lastSharedMesh;
   
   #region Default processing
   public void OnSurfaceHit(in RaycastHit hit,SurfaceHitType hitType,SurfaceEffect defaultEffect)
   {
      //simple caching
      GetRendererAndCollider(in hit);
      
      int subMeshIndex = GetSubmeshIndex(hit.triangleIndex,_lastRendererMaterials.Count,_lastSharedMesh);

      SurfaceEffect effect = defaultEffect;
      if (surfaceMaterials.TryGetValue(_lastRendererMaterials[subMeshIndex].mainTexture, out var material))
      {
         effect = material.SurfaceEffectsForHits[(int) hitType];
      }
      if (effect)
      {
         if(effect.decalsVariant != null && effect.decalsVariant.Length>0)
            SpawnEffect(effect, hit.point, hit.normal);
         if (effect.Audio != null)
         {
            PlayAudio(effect, hit.point);
         }
      }
   }
   
   public void OnSurfaceHit(Collision collision,SurfaceHitType hitType,SurfaceEffect defaultEffect)
   {
      var contact = collision.GetContact(0);
      collision.collider.Raycast(new Ray(contact.point, contact.normal), out var hit, contact.separation);
      OnSurfaceHit(hit,hitType,defaultEffect);
   }
   
   
   #endregion

   public bool OnSurfaceHitWithoutDefault(in RaycastHit hit,SurfaceHitType hitType)
   {
      //simple caching
      GetRendererAndCollider(in hit);
      
      int subMeshIndex = GetSubmeshIndex(hit.triangleIndex,_lastRendererMaterials.Count,_lastSharedMesh);
      
      if (surfaceMaterials.TryGetValue(_lastRendererMaterials[subMeshIndex].mainTexture, out var material))
      {
         var effect = material.SurfaceEffectsForHits[(int)hitType];
         if (effect)
         {
            if(effect.decalsVariant != null && effect.decalsVariant.Length>0)
               SpawnEffect(effect, hit.point, hit.normal);
            if (effect.Audio)
            {
               PlayAudio(effect, hit.point);
               return true;
            }
         }
      }
      return false;
   }
   
   
   private void SpawnEffect(SurfaceEffect effect,Vector3 position,Vector3 normal)
   {
      var prefab = effect.decalsVariant[Range(0, effect.decalsVariant.Length)];
      
      var effectInstance = Instantiate(prefab, position + normal * effect.spawnDistance, Quaternion.LookRotation(normal));
      effectInstance.transform.localScale = Vector3.one * Range(effect.minMaxRandomScale.x,effect.minMaxRandomScale.y); 
      
      if(effect.destroyTimer.HasValue)
         Destroy(effectInstance,effect.destroyTimer.Value);
   }

   private void PlayAudio(SurfaceEffect effect, Vector3 position)
   {
      switch (effect.Audio)
      {
         case null:
            break;
         case AudioClip clip:
         {
            PlayClipAtPoint(effect.name,clip, position);
            break;
         }
         case IAudioType audio:
         {
            var source = GetAudioSourceAtPoint(effect.name, position);
            float length = audio.Play(source,false);
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
      var filter   = hitCollider.GetComponentInChildren<MeshFilter>();
      var renderer = hitCollider.GetComponentInChildren<Renderer>();

      _lastSharedMesh = filter.sharedMesh; 
      renderer.GetSharedMaterials(_lastRendererMaterials);
      
      _colliderId = hit.colliderInstanceID;
   }
}

public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour{
   public static T instance { get; private set; }

   protected virtual void Awake()
   {
      instance = this as T;
      DontDestroyOnLoad(instance.gameObject);
   }
}