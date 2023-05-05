#if UNITY_EDITOR
using System;
using UnityEditor;

public class BaseListSerializeReferenceDrawer
{
    protected bool DoesFitDrawCondition(SerializedProperty property)
    {
        var path = property.propertyPath;
        var index0 = path.LastIndexOf('[');

        try
        {
            var currentIndex = int.Parse(path.Substring(index0+1,path.Length-2 - index0));
            return currentIndex < 1;
        }
        catch (Exception e)
        {
            return false;
        }
    }

    protected SerializedProperty GetTargetProperty(SerializedProperty property)
    {
        var separated = property.propertyPath.Split('.');
        var target = separated[^3];

        if (separated.Length - 3 > 0)
        {
            var fullPathAdd = "";
            for (int i = 0; i < separated.Length - 3; i++)
            {
                fullPathAdd += separated[i] + '.';
            }

            target = fullPathAdd + target;
        }

        return property.serializedObject.FindProperty(target);
    }
}
#endif
