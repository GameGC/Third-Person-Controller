using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "EasyEnum")]
public class EasyEnum : ScriptableObject
{
   public string[] enumNames;

   public string previousName;
  

   private void OnValidate()
   {
      if (!string.IsNullOrEmpty(previousName))
         if (previousName != name)
            AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(this).Replace("asset", "enum.cs")
               .Replace(name, previousName));
      
      previousName = name;
      var newFilepath = AssetDatabase.GetAssetPath(this).Replace("asset", "enum.cs");
      var result = enumNames.Aggregate((s, s0) => "    "+s + ",\n    " + s0);
      string format =$"public enum {this.name}"+
      "\n{"
      +$"\n{result}"
      +"\n}";
      File.WriteAllText(newFilepath,format);
      AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
      //AssetDatabase.ImportAsset(newFilepath);
   }
   
   
}
