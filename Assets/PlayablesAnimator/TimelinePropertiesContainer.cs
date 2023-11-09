using System;
using GameGC.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using Object = UnityEngine.Object;

[CreateAssetMenu(menuName = "Create TimelinePropertiesContainer", fileName = "TimelinePropertiesContainer", order = 0)]
public class TimelinePropertiesContainer:AnimationValue
{
    [Serializable]
    public struct SceneValue: ITimelineValue
    {
        public string path;
        public string Type;

        public SceneValue(Object value,Transform root)
        {
            var transform = ((Component) value).transform;
            path = AnimationUtility.CalculateTransformPath(transform, root);
            this.Type = value.GetType().AssemblyQualifiedName;
        }
        public Object GetValue(Transform root)
        {
            var type = System.Type.GetType(this.Type);
            return string.IsNullOrEmpty(path) ? root.GetComponent(type) : root.Find(path).GetComponent(type);
        }
    }
    [Serializable]
    public struct ProjectValue : ITimelineValue
    {
        public Object obj;
        public Object GetValue(Transform root) => obj;
    }
    
    public interface ITimelineValue
    {
        public Object GetValue(Transform root);
    }
    
   // [SerializeReference]
    public SDictionary<int, ITimelineValue> values;
    
    public TimelineAsset Asset;

    public void SetValue(PropertyName id, Object value,Transform root)
    {
        if (value is Component c)
        {
            if (values.TryGetValue(id.GetHashCode(), out var tempValue))
            {
                values[id.GetHashCode()] = c.gameObject.scene.IsValid()
                    ? new SceneValue(value, root)
                    : new ProjectValue() {obj = value};
            }
            else
            {
                values.Add(id.GetHashCode(), c.gameObject.scene.IsValid()
                    ? new SceneValue(value, root)
                    : new ProjectValue() {obj = value});
            }
        }
    }
    

    public Object GetReferenceValue(PropertyName id,Transform root)
    {
        if (values.TryGetValue(id.GetHashCode(),out var tempValue))
            return tempValue.GetValue(root);
        return null;
    }

    public void ClearReferenceValue(PropertyName id) => values.Remove(id.GetHashCode());

    public override Playable GetPlayable(PlayableGraph graph, GameObject root)
    {
        return Asset.CreatePlayable(graph, root);
    }
}