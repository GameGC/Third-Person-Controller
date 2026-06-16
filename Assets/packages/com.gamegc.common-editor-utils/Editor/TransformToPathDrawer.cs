using CommonEditorUtils.Editor;
using GameGC.CommonEditorUtils.Attributes;
using GameGC.CommonEditorUtils.Editor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

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
    
    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        var container = UIElementsExtras.HorizontalContainer;

        var label = UIElementsExtras.Label(property.displayName);
        container.Add(label);

        var objectField = new ObjectField
        {
            objectType = typeof(Transform),
            allowSceneObjects = true,
            value = null
        };
        objectField.style.flexGrow = 1;
        container.Add(objectField);

        var targetMB = property.serializedObject.targetObject as MonoBehaviour;
        var attribute = this.attribute as TransformToPathAttribute;

        Transform root = null;
        if (attribute != null)
        {
            root = attribute.RootType == typeof(Transform) ?
                targetMB.transform.root : 
                targetMB.GetComponentInParent(attribute.RootType)?.transform;
        }

        Transform targetTransform = null;
        if (root != null && !string.IsNullOrEmpty(property.stringValue))
            targetTransform = root.Find(property.stringValue);

        objectField.value = targetTransform;

        objectField.RegisterValueChangedCallback(evt =>
        {
            property.stringValue = evt.newValue is Transform newTransform && root != null
                ? AnimationUtility.CalculateTransformPath(newTransform, root)
                : string.Empty;
            property.serializedObject.ApplyModifiedProperties();
        });

        return container;
    }
}