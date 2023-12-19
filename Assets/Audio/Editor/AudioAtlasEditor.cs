using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

[CustomEditor(typeof(AudioAtlas))]
public class AudioAtlasEditor : Editor
{
    float[] keys = Array.Empty<float>();
    
    GUIContent timelineSignal;
    int Selected = -1;
    bool recalculate;
    float lastMouseX;

    private SerializedProperty clipProperty;
    private void OnEnable()
    {
        clipProperty = serializedObject.FindProperty("clip");
        timelineSignal = EditorGUIUtility.IconContent(
            $"Packages/com.unity.timeline/Editor/StyleSheets/Images/Icons/TimelineSignal.png");
    }

    private void LoadKeys(SerializedProperty property, float clipLength,float rectWidth)
    {
        keys = new float[property.arraySize];
        float multiplier =  rectWidth /clipLength;
        for (int i = 0; i < property.arraySize; i++)
            keys[i] = multiplier * property.GetArrayElementAtIndex(i).floatValue;
    }

    public override VisualElement CreateInspectorGUI()
    {
        var root = new VisualElement();
        var iterator = serializedObject.GetIterator();
        iterator.NextVisible(true);
        iterator.NextVisible(false);
        root.Add(new PropertyField(clipProperty));

        root.Add(new IMGUIContainer(DoInspectorGUI));
        while (iterator.NextVisible(false))
        {
            root.Add(new PropertyField(iterator));
        }
        return root;
    }

    private void DoInspectorGUI()
    {
        if(clipProperty.objectReferenceValue == null) return;
        var rect = GUILayoutUtility.GetRect(1, 18 * 4, GUILayout.ExpandWidth(true));
        if (keys.Length < 1 && Event.current.type == EventType.Repaint)
        {
            var target = this.target as AudioAtlas;
            LoadKeys(serializedObject.FindProperty("timings"), target.clip.length, rect.width);
        }
        
        DrawAudioPreview(rect);
    }

    private void DrawAudioPreview(Rect position)
    {
        Texture2D waveformTexture = AssetPreview.GetAssetPreview(clipProperty.objectReferenceValue);
        GUI.DrawTexture(position, waveformTexture, ScaleMode.StretchToFill, true);

        if (Event.current.type is EventType.MouseDown or EventType.MouseDrag)
        {
            var pos = Event.current.mousePosition;
            if (!position.Contains(pos)) return;
            lastMouseX = pos.x;
        }

        var target = this.target as AudioAtlas;
        if (Event.current.type is EventType.MouseUp)
        {
            if (Selected > -1)
            {
                float multiplier = target.clip.length / position.width;
                target.timings[Selected] = multiplier * keys[Selected];
            }

            Selected = -1;
        }

        if (Event.current.type == EventType.ContextClick)
        {
            bool isSelected = Selected > -1;
            var menu = new GenericMenu();

            if (isSelected)
            {
                menu.AddDisabledItem(new GUIContent("Split"));
                menu.AddItem(new GUIContent("Remove Split"), false, () =>
                {
                    ArrayUtility.RemoveAt(ref keys, Selected);
                    ArrayUtility.RemoveAt(ref target.timings, Selected);
                    Selected = -1;
                });
            }
            else
            {
                float lastX = Event.current.mousePosition.x;
                menu.AddItem(new GUIContent("Split"), false, () =>
                {
                    float multiplier = target.clip.length / position.width;
                    var valueConverted = multiplier * lastX;

                    ArrayUtility.Add(ref keys, lastX);
                    Array.Sort(keys);

                    ArrayUtility.Add(ref target.timings, valueConverted);
                    Array.Sort(target.timings);
                });
                menu.AddDisabledItem(new GUIContent("Remove Split"));
            }

            menu.ShowAsContext();
        }
        
        var pointerSize = new Vector2(position.height / 8, position.height / 4);
        for (int i = 0; i < keys.Length; i++)
        {
            ref var key = ref keys[i];
                
            float distance1d = Mathf.Abs(lastMouseX - key);
            if (Selected == i && distance1d < pointerSize.x * 4 && Event.current.type == EventType.MouseDrag)
                key = lastMouseX;

            EditorGUI.BeginChangeCheck();
            var toggleRect = new Rect(new Vector2(key, position.y + position.height * 0.75f), pointerSize);
            var newValue = GUI.Toggle(toggleRect, Selected == i, timelineSignal, GUI.skin.button);
            if (EditorGUI.EndChangeCheck())
            {
                Selected = newValue ? i : -1;
            }
        }
    }
}