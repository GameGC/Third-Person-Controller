using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR

[CreateAssetMenu(menuName = "EasyEnum")]
internal class EasyEnum : ScriptableObject
{
   private enum BaseType
   {
      none_, int_,byte_,short_
   }

   [SerializeField] private string nameSpace;
   [SerializeField] private bool flags;
   [SerializeField] private BaseType baseType;
   [SerializeField] private bool hasNumbers = true;
   [SerializeField] private string[] enumNames;
   
   [HideInInspector]
   [SerializeField] private string previousName;
  

   private void OnValidate()
   {
      hideFlags = HideFlags.DontSaveInBuild;
      if (!string.IsNullOrEmpty(previousName))
         if (previousName != name)
            AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(this).Replace("asset", "enum.cs")
               .Replace(name, previousName));
      
      previousName = name;
      var newFilepath = AssetDatabase.GetAssetPath(this).Replace("asset", "enum.cs");
      string result = null;
      bool first = true;

      bool hasNamespace = !string.IsNullOrEmpty(nameSpace);

      var defaultLength = 4;
      var defaultStartLength = 2;
      
      string extraIndent = hasNamespace ? GetWhiteSpace(4) : "";
      
      if (enumNames != null && enumNames.Length > 0)
      {
         int maxChars = enumNames.Max(s => s.Length);
         result = GetWhiteSpace(defaultLength) + enumNames[0] +new string(' ', maxChars-enumNames[0].Length)+ (hasNumbers ? " = 0" : "");
         int index = 0;

         foreach (var enumName in enumNames)
         {
            if (first)
            {
               first = false;
               if (flags)
                  index += 2;
               else
                  index++;
               continue;
            }

            result = $"{result},\n{extraIndent+GetWhiteSpace(defaultLength)}" +
                     $"{enumName +GetWhiteSpace(maxChars-enumName.Length) + (hasNumbers ? $" = {index}" : "")}";
            Debug.Log(result);
            if (flags)
               index *= 2;
            else
               index++;
         }
      }

      string nameSpaceFormat = hasNamespace ? $"namespace {nameSpace}\n{{\n":"";

      string flagsString = flags ? "[System.Flags]\n" : "";
      string format =$"{nameSpaceFormat}{flagsString}{extraIndent}public enum {name}{GetBaseTypeString()}"+
                     "\n"+extraIndent+"{"
                     +$"\n{extraIndent}{result}"
                     +"\n"+extraIndent+"}";
      if (hasNamespace)
         format += "\n}";
      
      File.WriteAllText(newFilepath,format);
      AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
   }

   private string GetWhiteSpace(int count) => new(' ', count);
   private string GetBaseTypeString()
   {
      if (baseType == BaseType.none_) return "";
      else
      {
         return " : " + baseType.ToString().Replace("_", "");
      }
   }
}

#endif

public class EasyEnumRuntime
{
   public string[] enumNames;
   public int[] values;

   public EasyEnumRuntime(string[] enumNames)
   {
      this.enumNames = enumNames;
      int length = enumNames.Length;
      values = new int[length];
      for (int i = 0; i < length; i++)
      {
         values[i] = i;
      }
      values = values;
   }
}

public class EasyEnumRuntimeRef
{
   EasyEnumRuntime enum_;

   private int valueIndex;
   public EasyEnumRuntimeRef(EasyEnumRuntime @enum)
   {
      enum_ = @enum;
   }
   
   public EasyEnumRuntimeRef(EasyEnumRuntime @enum,string valueName)
   {
      enum_ = @enum;
      valueIndex = Array.IndexOf(enum_.enumNames, valueName);
   }
   
   public EasyEnumRuntimeRef(EasyEnumRuntime @enum,int value)
   {
      enum_ = @enum;
      valueIndex = value;
   }

   private int GetFlagsIndex() => valueIndex < 2 ? valueIndex : valueIndex * 2;
   
   public static EasyEnumRuntimeRef operator |(EasyEnumRuntimeRef a, EasyEnumRuntimeRef b)
   {
      return new EasyEnumRuntimeRef(a.enum_, a.GetFlagsIndex() | b.GetFlagsIndex());
   }

   public static EasyEnumRuntimeRef operator &(EasyEnumRuntimeRef a, EasyEnumRuntimeRef b)
   {
      return new EasyEnumRuntimeRef(a.enum_, a.GetFlagsIndex() & b.GetFlagsIndex());
   }
}