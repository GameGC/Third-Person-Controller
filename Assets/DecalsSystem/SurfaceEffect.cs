using System;
using GameGC.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;

[CreateAssetMenu(menuName = "Surface/Effect", fileName = "SurfaceEffect", order = 0)]
public class SurfaceEffect : ScriptableObject
{
    [Header("Decals & Effects")]
    public GameObject[] decalsVariant;

    [MinMax(0.1f,2)]
    public Vector2 minMaxRandomScale = Vector2.one;
    public float spawnDistance = 0.01f;
    public SNullable<float> destroyTimer =10;

    [Header("Audio"),ScriptableObjectCreate(typeof(IAudioType))]
    public Object Audio;

}

public struct EasyGUI
    {
        private const float SingleLineHeight = 18f;
        
        private Rect _rect;
        private Vector2 _position;
        public EasyGUI(Rect rect)
        {
            _rect = rect;
            _position = new Vector2(rect.x, rect.y);
        }
        public void NextLine(float height, out Rect rect)
        {
            _position.x = _rect.x;
            rect = new Rect(_position, new Vector2(_rect.width, height));
            _position += Vector2.up * height;
        }
        
        public void NextSingleLine(out Rect rect)
        {
            _position.x = _rect.x;
            rect = new Rect(_position, new Vector2(_rect.width, SingleLineHeight));
            _position += Vector2.up * SingleLineHeight;
        }
        
        public void NextHalfLine(float height,float widthMultiplier, out Rect rect)
        {
            var size = new Vector2(_rect.width * widthMultiplier, height);
            rect = new Rect(_position, size);
            _position += size;
        }
        
        public void NextHalfSingleLine(float widthMultiplier, out Rect rect)
        {
            var size = new Vector2(_rect.width * widthMultiplier, SingleLineHeight);
            rect = new Rect(_position, size);
            _position += size;
        }
        
        public void CurrentHalfLine(float height,float widthMultiplier, out Rect rect)
        {
            float width = _rect.width * widthMultiplier;
            rect = new Rect(_position,  new Vector2(width, height));
            _position += Vector2.right * width;
        }
        
        public void CurrentHalfSingleLine(float widthMultiplier, out Rect rect)
        {
            float width = _rect.width * widthMultiplier;
            rect = new Rect(_position,  new Vector2(width, SingleLineHeight));
            _position += Vector2.right * width;
        }
        
        public void CurrentAmountLine(float height,float width, out Rect rect)
        {
            rect = new Rect(_position,  new Vector2(width, height));
            _position += Vector2.right * width;
        }
        
        public void CurrentAmountSingleLine(float width,out Rect rect)
        {
            rect = new Rect(_position,  new Vector2(width, SingleLineHeight));
            _position += Vector2.right * width;
        }
    }


public class MinMaxAttribute : PropertyAttribute
{
    public readonly float MinLimit;
    public readonly float MaxLimit;
    public MinMaxAttribute(float min, float max)
    {
        MinLimit = min;
        MaxLimit = max;
    }
}

[CustomPropertyDrawer(typeof(MinMaxAttribute))]
public class MinMaxDrawer : PropertyDrawer
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
