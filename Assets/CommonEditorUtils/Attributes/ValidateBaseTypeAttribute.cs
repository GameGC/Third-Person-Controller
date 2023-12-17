using System;
using System.Collections.Generic;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class ValidateBaseTypeAttribute : PropertyAttribute
{
    public readonly Type[] SupportedBaseTypes;

    public ValidateBaseTypeAttribute(params Type[] supportedBaseTypes)
    {
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
    }
    private static List<Type> GetInheritedClasses(Type myType)
    {
        if (myType.IsClass)
            return AllTypesContainer.AllTypes.FindAll(t => t != myType && t.IsClass && !t.IsAbstract
                                                           && t.IsSubclassOf(myType));

        if (myType.IsInterface)
            return AllTypesContainer.AllTypes.FindAll(t => t != myType && t.IsClass && !t.IsAbstract
                                                           && myType.IsAssignableFrom(t));
        throw new NotImplementedException();
    }
}