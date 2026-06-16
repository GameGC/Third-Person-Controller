using System;
using UnityEngine;

#if UNITY_EDITOR
using System.Collections.Generic;
using System.Reflection;
#endif

namespace GameGC.CommonEditorUtils.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class ValidateBaseTypeAttribute : PropertyAttribute
    {
        public readonly Type[] SupportedBaseTypes;

        public ValidateBaseTypeAttribute(params Type[] supportedBaseTypes)
        {
#if UNITY_EDITOR
            var list = new List<Type>(supportedBaseTypes);
            foreach (var t in supportedBaseTypes)
            {
                var childTypes = GetInheritedClasses(t);
                if(childTypes.Count > 0)
                    list.AddRange(childTypes);
            }

            for (var i = 0; i < supportedBaseTypes.Length; i++)
            {
                if(list[i].IsInterface)
                    list.RemoveAt(i);
            }
            SupportedBaseTypes = list.ToArray();
#endif

        }
#if UNITY_EDITOR
        private static List<Type> GetInheritedClasses(Type myType)
        {
            var target =
                Type.GetType(
                    "GameGC.CommonEditorUtils.Editor.AllTypesContainer, CommonEditorUtils.Editor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
            var AllTypes = target.GetField("AllTypes", BindingFlags.Static | BindingFlags.Public).GetValue(null) as List<Type>;
            if (myType.IsClass)
                return AllTypes.FindAll(t => t != myType && t.IsClass && !t.IsAbstract
                                             && t.IsSubclassOf(myType));

            if (myType.IsInterface)
                return AllTypes.FindAll(t => t != myType && t.IsClass && !t.IsAbstract
                                             && myType.IsAssignableFrom(t));
            throw new NotImplementedException();
        }
#endif
    }
}