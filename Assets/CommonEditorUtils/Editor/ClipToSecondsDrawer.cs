using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ClipToSecondsAttribute))]
internal class ClipToSecondsDrawer : PropertyDrawer
{
    private static Dictionary<string, AnimationClip> tempBuffer = new Dictionary<string, AnimationClip>(10);

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return 18;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        float width = position.width;
        position.width = width * 0.6666667F;
        EditorGUI.PropertyField(position, property, label);
        position.x +=  position.width;
        position.width = width * 0.33333334F;
        
        var path = property.propertyPath;
        tempBuffer.TryGetValue(path, out var clip);


      
        
        EditorGUI.BeginChangeCheck();
        clip= EditorGUI.ObjectField(position, clip,typeof(AnimationClip),false) as AnimationClip;
        if (EditorGUI.EndChangeCheck())
        {
            if (!tempBuffer.ContainsKey(path))
                tempBuffer.Add(path, clip);
            else tempBuffer[path] = clip;

            property.floatValue = clip.length;
            property.serializedObject.ApplyModifiedProperties();
        }
    }
}