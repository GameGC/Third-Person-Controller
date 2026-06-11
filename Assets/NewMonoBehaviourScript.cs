using GameGC.CommonEditorUtils.Attributes;
using UnityEngine;

namespace CommonEditorUtils.Editor
{
    public class NewMonoBehaviourScript : MonoBehaviour
    {
        [ComponentSelect]public Component selectTes;
        [ComponentSelect]public Component[] selectTest;

        [ClipToSeconds]
        public float test;
        [TransformToPath]
        public string path;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
        
        }
    }
}
