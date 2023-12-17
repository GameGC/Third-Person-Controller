using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(TransformToPathAttribute))]
internal class TransformToPathDrawer : PropertyDrawerWithCustomData<TransformToPathDrawer.Data>
{
    public class Data
    {
        public bool inInited;
        public Transform root;
        public Transform target;
    }

    protected override void OnGUI(Rect position, SerializedProperty property, GUIContent label, Data customData)
    {
        var target = property.serializedObject.targetObject as MonoBehaviour;

        if (PrefabUtility.IsPartOfPrefabInstance(target.gameObject))
        {
            if (!customData.inInited)
            {
                var attribute = this.attribute as TransformToPathAttribute;

                if (attribute.RootType == typeof(Transform))
                {
                    customData.root = target.transform.root;
                }
                else
                {
                    customData.root = target.GetComponentInParent(attribute.RootType).transform;
                }

                customData.target = customData.root.Find(property.stringValue);
                customData.inInited = true;
            }

            using (var changeCheckScope = new EditorGUI.ChangeCheckScope())
            {
                var newTrnasform =
                    EditorGUI.ObjectField(position, label, customData.target, typeof(Transform)) as Transform;
                if (changeCheckScope.changed)
                {
                    property.stringValue = AnimationUtility.CalculateTransformPath(newTrnasform, customData.root);
                    customData.target = newTrnasform;
                    property.serializedObject.ApplyModifiedProperties();
                }
            }
        }
        else
        {
            EditorGUI.PropertyField(position, property, label);
            property.serializedObject.ApplyModifiedProperties();
        }
    }
}