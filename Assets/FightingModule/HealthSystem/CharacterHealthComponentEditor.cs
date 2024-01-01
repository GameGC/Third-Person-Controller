#if UNITY_2022_1_OR_NEWER
using UnityEditor;

[CustomEditor(typeof(CharacterHealthComponent))]
public class CharacterHealthComponentEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
    }
    //public override VisualElement CreateInspectorGUI()
    //{
    //    var root = new VisualElement();
    //    var iterator = serializedObject.GetIterator();
    //    
    //    iterator.NextVisible(true);
    //    while (iterator.NextVisible(false)) 
    //        root.Add(DoProperty(iterator));
//
    //    return root;
    //}
//
    //private void OnEnable()
    //{
    //    Repaint();
    //}
//
    //private VisualElement DoProperty(SerializedProperty p)
    //{
    //    if (p.isArray)
    //    {
    //        var pCopy = p.Copy();
    //        SerializedObject serializedObjectCopy = pCopy.serializedObject;
    //        string path = pCopy.propertyPath;
    //        
    //        return new IMGUIContainer(() =>
    //        {
    //            if (pCopy == null)
    //            {
    //                Debug.Log("p"+path);
    //                pCopy = serializedObject.FindProperty(path);
    //            }
    //            EditorGUILayout.PropertyField(pCopy);
    //        });
    //    }
    //    return new PropertyField(p);
    //}
//
    //private void OnDisable()
    //{
    //    serializedObject.Dispose();
    //}
}
#endif
