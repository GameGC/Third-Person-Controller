using System;
using System.Collections.Generic;
using System.Linq;
using GameGC.Collections;
using ThirdPersonController.Core.CodeStateMachine.CustomEditor.Editor;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

[CreateAssetMenu(menuName = "Create SurfaceEffect", fileName = "SurfaceEffect", order = 0)]
public class SurfaceEffect : ScriptableObject
{
    [Header("Decals & Effects")]
    public GameObject[] decalsVariant;
    public Vector2 minMaxRandomScale = Vector2.one;
    public float spawnDistance = 0.01f;
    public SNullable<float> destroyTimer =5;

    [Header("Audio")]
    [SerializeReference] 
    //[SerializeReferenceDropdown]
    public IAudioType Audio;

    public interface IAudioType
    {
        public float Pitch { get; }
        public float Volume { get; }
    }
    [Serializable]
    public class AudioAtlas : IAudioType
    {
        public AudioClip clip;
        public float[] timings;
        
        public Vector2 minMaxVolume = Vector2.one;
        public Vector2 minMaxPitch = Vector2.one;
        
        public void GetElement(int index, out AudioClip clip, out float start, out float end)
        {
            clip = this.clip;
            int timingsCount = timings.Length;
            bool isClipNotSplited = timingsCount < 1;
            start = index < 1? 0 : isClipNotSplited?0 : timings[index-1];
            end = isClipNotSplited ? clip.length : timingsCount -1 >index? timings[index]: clip.length;
        }
        
        public void GetRandomElement(out AudioClip clip, out float start, out float end)
        {
            clip = this.clip;

            var index = Random.Range(0,timings.Length+1);
            GetElement(index, out clip, out start, out end);
        }

        public float Pitch => Random.Range(minMaxPitch.x, minMaxPitch.y);
        public float Volume => Random.Range(minMaxVolume.x, minMaxVolume.y);
    }

    [Serializable]
    public class AudioClips : IAudioType
    {
        public AudioClip[] clipsVariants;

        public Vector2 minMaxVolume = Vector2.one;
        public Vector2 minMaxPitch = Vector2.one;
        public void GetRandomClip(out AudioClip clip)
        {
            clip = clipsVariants[Random.Range(0, clipsVariants.Length)];
        }

        public float Pitch => Random.Range(minMaxPitch.x, minMaxPitch.y);
        public float Volume => Random.Range(minMaxVolume.x, minMaxVolume.y);
    }

   
}

 [CustomPropertyDrawer(typeof(SurfaceEffect.AudioAtlas))]
    public class AudioAtlassDrawer : PropertyDrawerWithCustomData<AudioAtlassDrawer.Cache>
    {
        public class Cache
        {
            public SurfaceEffect.AudioAtlas target;
            public Rect propertyRect;
            public Rect clipDisplayRect;
            public Vector2 pointerSize;

            public List<Rect> otherRects = new List<Rect>();

            public GUIContent timelineSignal;
            public float[] keys;
            public int Selected = -1;

            public float lastMouseX;
        }
        private Action<SurfaceEffect.AudioAtlas> _delayedSave;

        protected override float GetPropertyHeight(SerializedProperty property, GUIContent label, Cache customData)
        {
            if (!property.isExpanded) return 18f;
            if (customData.target != null)
            {
                return customData.propertyRect.height + customData.clipDisplayRect.height +
                       customData.otherRects.Sum(r => r.height);
            }

            return 0;
        }

        private void LoadKeys(SerializedProperty property, Cache customData,float clipLength,float rectWidth)
        {
            customData.keys = new float[property.arraySize];
            float multiplier =  rectWidth /clipLength;
            for (int i = 0; i < property.arraySize; i++)
            {
                customData.keys[i] = multiplier * property.GetArrayElementAtIndex(i).floatValue;
            }
        }

        protected override void OnEnable(Rect position, SerializedProperty iterator, GUIContent label, Cache customData)
        {
            customData.target = iterator.GetProperty<SurfaceEffect.AudioAtlas>();
            LoadKeys(iterator.FindPropertyRelative("timings"),
                customData,customData.target.clip.length, position.width);
            customData.timelineSignal =
                EditorGUIUtility.IconContent(
                    $"Packages/com.unity.timeline/Editor/StyleSheets/Images/Icons/TimelineSignal.png");
            
            // rects calculations
            var guiScope = new EasyGUI(position);
                
            guiScope.NextSingleLine(out var tempRect);
            customData.propertyRect = tempRect;
                
            guiScope.NextLine(18*4,out tempRect);
            customData.clipDisplayRect = tempRect;
            customData.pointerSize = new Vector2(tempRect.height / 8, tempRect.height / 4);

            RecalculateOtherRects(guiScope,iterator, customData);
        }

        private void RecalculateOtherRects(EasyGUI guiScope ,SerializedProperty iterator,Cache customData)
        {
            iterator.NextVisible(true);
            guiScope.NextLine(EditorGUI.GetPropertyHeight(iterator),out var tempRect);

            if (customData.otherRects.Count > 0) 
                customData.otherRects.Clear();
            customData.otherRects.Add(tempRect);
            while (iterator.NextVisible(false))
            {
                guiScope.NextLine(EditorGUI.GetPropertyHeight(iterator),out tempRect);
                customData.otherRects.Add(tempRect);
            }
        }
        
        protected override void OnGUI(Rect position, SerializedProperty property, GUIContent label, AudioAtlassDrawer.Cache customData)
        {
            using (new EditorGUI.IndentLevelScope())
            {
                EditorGUI.PropertyField(customData.propertyRect,property,label,false);
                if (property.isExpanded)
                {
                    if (customData.target.clip)
                    {
                        DrawPreview(customData.clipDisplayRect, property, customData);
                    }

                    int i = 1;
                    property.NextVisible(true);
                    EditorGUI.PropertyField(customData.otherRects[0], property);
                
                    while (property.NextVisible(false))
                    {
                        EditorGUI.PropertyField(customData.otherRects[i], property);
                        i++;
                    }
                }
            }
        }

        private void DrawPreview(Rect position, SerializedProperty property,Cache cache)
        { 
            Texture2D waveformTexture = AssetPreview.GetAssetPreview(cache.target.clip);
            GUI.DrawTexture(position, waveformTexture,ScaleMode.StretchToFill,true);
            
            if (Event.current.type is EventType.MouseDown or EventType.MouseDrag)
            {
                var pos = Event.current.mousePosition;
                if(!position.Contains(pos)) return;
                cache.lastMouseX = pos.x;
            }

            if (Event.current.type is EventType.MouseUp)
            {
                if (cache.Selected > -1)
                {
                    float multiplier = cache.target.clip.length /position.width;
                    cache.target.timings[cache.Selected] = multiplier * cache.keys[cache.Selected];
                }
                cache.Selected = -1;
            }

            if (Event.current.type == EventType.ContextClick)
            {
                bool isSelected = cache.Selected > -1;
                var menu = new GenericMenu();

                var copy = GetCopy(property);
                if (isSelected)
                {
                    menu.AddDisabledItem(new GUIContent("Split"));
                    menu.AddItem(new GUIContent("Remove Split"),false,()=>
                    {
                        ArrayUtility.RemoveAt(ref cache.keys,cache.Selected);
                        ArrayUtility.RemoveAt(ref cache.target.timings, cache.Selected);
                        cache.Selected = -1;

                        var newRect = new Rect(new Vector2(position.x,position.y + position.height), position.size);
                        RecalculateOtherRects(new EasyGUI(newRect), copy,cache);
                    });
                }
                else
                {
                    float lastX = Event.current.mousePosition.x;
                    menu.AddItem(new GUIContent("Split"),false,()=>
                    {
                        float multiplier = cache.target.clip.length /position.width;
                        var valueConverted = multiplier * lastX;
                        
                        ArrayUtility.Add(ref cache.keys,lastX);
                        Array.Sort(cache.keys);

                        ArrayUtility.Add(ref cache.target.timings,valueConverted);
                        Array.Sort(cache.target.timings);
                        
                        var newRect = new Rect(new Vector2(position.x,position.y + position.height), position.size);
                        RecalculateOtherRects(new EasyGUI(newRect), copy,cache);
                    });
                    menu.AddDisabledItem(new GUIContent("Remove Split"));
                }
              
                
                menu.ShowAsContext();
            }

            for (int i = 0; i < cache.keys.Length; i++)
            {
                ref var key = ref cache.keys[i];
                
                float distance1d = Mathf.Abs(cache.lastMouseX - key);
                if (cache.Selected == i && distance1d < cache.pointerSize.x * 4 && Event.current.type == EventType.MouseDrag)
                    key = cache.lastMouseX;

                EditorGUI.BeginChangeCheck();
                var toggleRect = new Rect(new Vector2(key, position.y + position.height * 0.75f), cache.pointerSize);
                var newValue = GUI.Toggle(toggleRect, cache.Selected == i, cache.timelineSignal, GUI.skin.button);
                if (EditorGUI.EndChangeCheck())
                {
                    cache.Selected = newValue ? i : -1;
                }
            }
        }
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

