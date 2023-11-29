using GameGC.Collections;
using UnityEngine;
using static UnityEngine.Random;

public class SurfaceSystem : Singleton<SurfaceSystem>
{
   public SKeyValueReadonlyArray<Texture, SurfaceEffect> SurfaceEffects;

   public void OnSurfaceHit(RaycastHit hit,SurfaceEffect defaultEffect)
   {
      var filter = hit.collider.GetComponentInChildren<MeshFilter>();
      var renderer = hit.collider.GetComponentInChildren<Renderer>();
      int subMeshIndex = GetSubmeshIndex(hit.triangleIndex,filter.mesh);

      SurfaceEffects.TryGetValue(renderer.materials[subMeshIndex].mainTexture, out var effect);
      effect ??= defaultEffect;
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
   
   public void OnSurfaceHit(Collision collision,SurfaceEffect defaultEffect)
   {
      var contact = collision.GetContact(0);
      collision.collider.Raycast(new Ray(contact.point, contact.normal), out var hit, contact.separation);
      OnSurfaceHit(hit,defaultEffect);
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
      AudioClip clip = null;
      switch (effect.Audio)
      {
         case null:
            break;
         case SurfaceEffect.AudioAtlas audioAtlas:
         {
            audioAtlas.GetRandomElement(out clip, out var start, out var end);
            PlayClipAtlassAtPoint(effect.name, clip, position, start, end, audioAtlas.Pitch, audioAtlas.Volume);
            break;
         }
         case SurfaceEffect.AudioClips audioClips:
         {
            audioClips.GetRandomClip(out clip);
            PlayClipAtPoint(effect.name, clip, position, audioClips.Pitch, audioClips.Volume);
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
   
   private static void PlayClipAtlassAtPoint(string name,AudioClip clip, Vector3 position,float start,float end, float volume = 1f,float pitch = 1f)
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
      
      audioSource.time = start;
      audioSource.Play();
      Destroy(gameObject, end-start);
   }
   
   private static int GetSubmeshIndex(int triangleIndex, Mesh mesh)
   {
      for (int i = 0; i < mesh.subMeshCount; i++)
      {
         var subMesh = mesh.GetSubMesh(i);
         if (subMesh.firstVertex >= triangleIndex && triangleIndex < subMesh.firstVertex + subMesh.vertexCount)
            return i;
      }

      return 0;
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