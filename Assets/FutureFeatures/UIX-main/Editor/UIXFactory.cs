using System;
using System.Collections;
using System.Reflection;
using RedOwl.UIX.Engine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace RedOwl.UIX.Editor
{
    public class UIXBuilder
    {
        private object _instance;
        private FieldInfo _info;
        private Type _type;

        private string _label;

        public UIXBuilder() {}

        public UIXBuilder For<T>(T instance, FieldInfo info)
        {
            _instance = instance;
            _info = info;
            _type = info.FieldType;
            return this;
        }

        public UIXBuilder WithLabel(string label)
        {
            _label = label;
            return this;
        }
        
        private VisualElement Build()
        {
            // TODO: Re-add back field generation
            // if (_fieldType.GetInterface(nameof(IList)) != null)
            // {
            //     
            //     return new UIXListField(new ListProxy(instance, info),  label);
            // }
            
            /*
            #region PrimativeFields
            
            if (_type == typeof(string))
            {
                // TODO: "TagAttribute" == TagField ?
                // TODO: EnumAttribute" == MaskField ?
                {
                    return BuildField<string, TextField>();
                }
            }

            if (_type == typeof(bool))
                return BuildField<bool, Toggle>();

            if (_type == typeof(int))
                return BuildField<int, IntegerField>();

            if (_type == typeof(float))
                return BuildField<float, FloatField>();

            if (_type == typeof(double))
                return BuildField<double, DoubleField>();

            if (_type == typeof(long))
                return BuildField<long, LongField>();

            if (typeof(Enum).IsAssignableFrom(_type))
            {
                VisualElement f = null;
                if (_type.IsDefined(typeof(FlagsAttribute), false))
                {
                    f = new EnumFlagsField(label, (Enum) Convert.ChangeType(info.GetValue(instance), _type), true);
                }
                else
                {
                    f = new EnumField(label, (Enum) Convert.ChangeType(info.GetValue(instance), _type));
                }

                f.RegisterCallback<ChangeEvent<Enum>>(evt => { info.SetValue(instance, evt.newValue); });
                return f;
            }
            
            #endregion
            
            #region UnityFields
            
            if (_type == typeof(Vector2))
                return BuildField<Vector2, Vector2Field>();

            if (_type == typeof(Vector2Int))
                return BuildField<Vector2Int, Vector2IntField>();

            if (_type == typeof(Vector3))
                return BuildField<Vector3, Vector3Field>();

            if (_type == typeof(Vector3Int))
                return BuildField<Vector3Int, Vector3IntField>();

            if (_type == typeof(Vector4))
                return BuildField<Vector4, Vector4Field>();

            if (_type == typeof(Rect))
                return BuildField<Rect, RectField>();

            if (_type == typeof(RectInt))
                return BuildField<RectInt, RectIntField>();

            if (_type == typeof(Bounds))
                return BuildField<Bounds, BoundsField>();

            if (_type == typeof(BoundsInt))
                return BuildField<BoundsInt, BoundsIntField>();

            if (_type == typeof(Color))
                return BuildField<Color, ColorField>();

            if (_type == typeof(AnimationCurve))
                return BuildField<AnimationCurve, CurveField>();

            if (_type == typeof(Gradient))
                return BuildField<Gradient, GradientField>();

            if (_type == typeof(LayerMask))
            {
                // TODO: "LayerMaskAttribute" == LayerMaskField OR LayerField ?
                return BuildField<int, LayerMaskField>();
            }

            if (typeof(Object).IsAssignableFrom(_type))
                return BuildField<Object, ObjectField>();

            #endregion
            */
            return null;
        }

        private static VisualElement BuildField<TType, TElement>() where TElement : BaseField<TType>, new()
        {
            /*
            Debug.Log($"Creating Field '{typeof(TElement).FullName}' For: {info.Name}");
            var field new TElement {label = label, value = (TType) info.GetValue(instance)};
            field.RegisterCallback<ChangeEvent<TType>>(evt =>
            {
                info.SetValue(instance, evt.newValue);
            });
            return field;
            */
            return null;
        }

        public static implicit operator VisualElement(UIXBuilder builder) => builder.Build();
    }
}