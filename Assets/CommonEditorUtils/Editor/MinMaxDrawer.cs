using System;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(MinMaxAttribute))]
internal class MinMaxDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // cast the attribute to make life easier
        MinMaxAttribute minMax = attribute as MinMaxAttribute;

        // This only works on a vector2! ignore on any other property type (we should probably draw an error message instead!)
        if (property.propertyType == SerializedPropertyType.Vector2)
        {
            // pull out a bunch of helpful min/max values....
            float minValue = (float) Math.Round(property.vector2Value.x, 4); // the currently set minimum and maximum value
            float maxValue = (float) Math.Round(property.vector2Value.y, 4);
            float minLimit = minMax.MinLimit; // the limit for both min and max, min cant go lower than minLimit and maax cant top maxLimit
            float maxLimit = minMax.MaxLimit;


            var fieldWidth = EditorGUIUtility.fieldWidth;

            var guiScope = new EasyGUI(position);
            guiScope.CurrentAmountSingleLine(EditorGUIUtility.labelWidth,out var tempRect);

            label = EditorGUI.BeginProperty(position, label, property);
            EditorGUI.PrefixLabel(tempRect, label);
            
            guiScope.CurrentAmountSingleLine(fieldWidth,out tempRect);
            minValue = EditorGUI.FloatField(tempRect, minValue);
            
            guiScope.CurrentAmountSingleLine(position.width-fieldWidth*2 - EditorGUIUtility.labelWidth,out tempRect);
            // and ask unity to draw them all nice for us!
            EditorGUI.MinMaxSlider(tempRect, ref minValue, ref maxValue, minLimit, maxLimit);

            guiScope.CurrentAmountSingleLine(fieldWidth,out tempRect);
            maxValue = EditorGUI.FloatField(tempRect, maxValue);
            
            
            var vec = Vector2.zero; // save the results into the property!
            vec.x = minValue;
            vec.y = maxValue;
            property.vector2Value = vec;
            
            EditorGUI.EndProperty();
        }
    }

    // this method lets unity know how big to draw the property. We need to override this because it could end up meing more than one line big
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUIUtility.singleLineHeight;
    }
}