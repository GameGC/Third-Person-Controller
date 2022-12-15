#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ThirdPersonController.ModuleInformation
{
   [CreateAssetMenu(menuName = "ModuleInformation")]
   public class ModuleInformation : ScriptableObject
   {
      public Object unityPackage;
      public Object[] files;

      public AssemblyDefinitionAsset targetAsset;
      public string[] dependsOnNames;

      private void Reset() => OnValidate();
      private void OnEnable() => OnValidate();
      private void OnValidate()
      {
         hideFlags = HideFlags.DontSaveInBuild;
      }



      [ContextMenu(nameof(ExportUnityPackage))]
      public void ExportUnityPackage()
      {
         List<string> pathes = new List<string>();
         foreach (var file in files)
         {
            pathes.Add(AssetDatabase.GetAssetPath(file));
         }

         var targetPath = AssetDatabase.GetAssetPath(this).Replace(".asset", ".unitypackage");
         AssetDatabase.ExportPackage(pathes.ToArray(),targetPath,ExportPackageOptions.Recurse | ExportPackageOptions.IncludeLibraryAssets);
         AssetDatabase.ImportAsset(targetPath);
         
         unityPackage = AssetDatabase.LoadAssetAtPath<Object>(targetPath);
      }

      [ContextMenu(nameof(LoadPackage))]
      public void LoadPackage()
      {
         if (unityPackage == null)
            throw new NullReferenceException("no package");
         
         if(dependsOnNames==null)
            dependsOnNames = new string[0];
         
         AssetDatabase.ImportPackage(AssetDatabase.GetAssetPath(unityPackage),true);
         
         var targetAssembly = JsonConvert.DeserializeObject<Dictionary<string, object>>(targetAsset.text);

         targetAssembly["references"] = dependsOnNames;
         
         File.WriteAllText(AssetDatabase.GetAssetPath(targetAsset),JsonConvert.SerializeObject(targetAssembly));
         AssetDatabase.Refresh();
      }


      [ContextMenu(nameof(UnloadPackage))]
      public void UnloadPackage()
      {
         if (unityPackage == null)
            throw new NullReferenceException("progress could be lost");
         
         List<string> pathes = new List<string>();
         foreach (var file in files)
         {
            pathes.Add(AssetDatabase.GetAssetPath(file));
         }

         AssetDatabase.DeleteAssets(pathes.ToArray(), new List<string>());

         if (dependsOnNames != null && dependsOnNames.Length > 0)
         {
            var targetAssembly = JsonConvert.DeserializeObject<Dictionary<string, object>>(targetAsset.text);

            var sourceArray = (targetAssembly["references"] as JArray).ToObject<string[]>();
            var references = new List<string>(sourceArray);
            foreach (var asm in dependsOnNames)
            {
               if (references.Contains(asm))
               {
                  references.Remove(asm);
               }
            }

            targetAssembly["references"] = references.ToArray();


            File.WriteAllText(AssetDatabase.GetAssetPath(targetAsset), JsonConvert.SerializeObject(targetAssembly));
            AssetDatabase.Refresh();

         }
      }
   }
}
#endif