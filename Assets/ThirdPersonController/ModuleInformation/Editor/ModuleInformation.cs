using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Unity.EditorCoroutines.Editor;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace ThirdPersonController.ModuleInformation.Editor
{
   [CreateAssetMenu(menuName = "ModuleInformation")]
   public class ModuleInformation : ScriptableObject
   {
      [SerializeField] private Object unityPackage;
      [SerializeField] private Object[] files;
      
      [SerializeField] private AssemblyDefinitionAsset targetAsset;
      [SerializeField] private string[] dependsOnNames;

      private void Reset() => OnValidate();
      private void OnEnable() => OnValidate();
      private void OnValidate()
      {
         hideFlags = HideFlags.DontSaveInBuild;
      }



      [ContextMenu(nameof(ExportUnityPackage))]
      public void ExportUnityPackage()
      {
         var targetPath = AssetDatabase.GetAssetPath(this).Replace(".asset", ".unitypackage");
         AssetDatabase.ExportPackage(files.Select(AssetDatabase.GetAssetPath).ToArray(),targetPath,ExportPackageOptions.Recurse);
         AssetDatabase.ImportAsset(targetPath);
         
         unityPackage = AssetDatabase.LoadAssetAtPath<Object>(targetPath);
      }


      [ContextMenu(nameof(ExportUnityPackageTest))]
      public void ExportUnityPackageTest()
      {
         ExportPackageOptions options = ExportPackageOptions.Recurse | ExportPackageOptions.IncludeDependencies;
         var targetPath = AssetDatabase.GetAssetPath(this).Replace(".asset", "test.unitypackage");
         AssetDatabase.ExportPackage(files.Select(AssetDatabase.GetAssetPath).ToArray(),targetPath,options);
      }
      
      
      [ContextMenu(nameof(LoadPackage))]
      public void LoadPackage()
      {
         if (unityPackage == null)
            throw new NullReferenceException("no package");
         
         if(dependsOnNames==null)
            dependsOnNames = new string[0];
        
         AssetDatabase.ImportPackage(AssetDatabase.GetAssetPath(unityPackage),false);

         
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

         
         var listFiles = new List<string>();
         foreach (var file in files)
         {
            if(file)
               listFiles.Add(AssetDatabase.GetAssetPath(file));
         }
         
         AssetDatabase.DeleteAssets(listFiles.ToArray(), new List<string>());

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

         }
         AssetDatabase.Refresh();
      }
   }
}