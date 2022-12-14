#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ThirdPersonController.ModuleInformation
{
   [CreateAssetMenu(menuName = "ModuleInformation")]
   public class ModuleInformation : ScriptableObject
   {
      public Object unityPackage;
      public Object[] files;

      private void Reset() => OnValidate();
      private void OnEnable() => OnValidate();
      private void OnValidate()
      {
         hideFlags = HideFlags.DontSaveInBuild;
      }



      [ContextMenu("GenerateUnityPackage")]
      public void GenerateUnityPackage()
      {
         List<string> pathes = new List<string>();
         foreach (var file in files)
         {
            pathes.Add(AssetDatabase.GetAssetPath(file));
         }
         AssetDatabase.ExportPackage(pathes.ToArray(),AssetDatabase.GetAssetPath(this).Replace(".asset",".unitypackage"),ExportPackageOptions.Recurse | ExportPackageOptions.IncludeLibraryAssets);
      }
   
      [ContextMenu("DeleteObjects")]
      public void DeleteObjects()
      {
         if (unityPackage == null)
            throw new NullReferenceException("progress could be lost");
      
         List<string> pathes = new List<string>();
         foreach (var file in files)
         {
            pathes.Add(AssetDatabase.GetAssetPath(file));
         }

         AssetDatabase.DeleteAssets(pathes.ToArray(), new List<string>());

      }
   }
}
#endif