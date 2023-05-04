#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


public abstract class PropertyDrawerWithCustomData<T> : PropertyDrawer
{
    Dictionary<string,T> collection = new Dictionary<string, T>();

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (!collection.TryGetValue(property.propertyPath, out var customData))
        {
            customData = Activator.CreateInstance<T>();
            collection.Add(property.propertyPath, customData);
        }

        return GetPropertyHeight(property, label, customData);
    }

    public virtual float GetPropertyHeight(SerializedProperty property, GUIContent label, T customData)
    {
        return base.GetPropertyHeight(property, label);
    }


    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (!collection.TryGetValue(property.propertyPath, out var customData))
        {
            customData = Activator.CreateInstance<T>();
            collection.Add(property.propertyPath, customData);
        }

        OnGUI(position,property, label, customData);
    }

    public abstract void OnGUI(Rect position, SerializedProperty property, GUIContent label, T customData);
}

#endif
