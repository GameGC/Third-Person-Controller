using System;
using UnityEditor;
using UnityEngine;

namespace ThirdPersonController.ModuleInformation.Editor
{
    [CustomEditor(typeof(SettingsInfo))]
    public class SettingsInfoEditor : UnityEditor.Editor
    {
        private const string mtpsCore = "com.gamegc.mtps.core";
        
        private Texture _inputTypesTex;
        private Texture _stateMachinesTex;
        
        private SettingsInfo _target;
        
        private readonly GUIContent _importText = EditorGUIUtility.TrTextContent("Import");
        private readonly GUILayoutOption _importWidth = GUILayout.Width(50);
        private void Awake()
        {
            _inputTypesTex = EditorGUIUtility.Load("ThirdPersonController/Settings/inputtypes.png") as Texture;
            _stateMachinesTex = EditorGUIUtility.Load("ThirdPersonController/Settings/statemachines.png") as Texture;
            
            _target = base.target as SettingsInfo; 
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.DrawTextureTransparent(GUILayoutUtility.GetAspectRect(2,GUILayout.ExpandWidth(true)),_inputTypesTex);
            EditorGUI.DrawTextureTransparent(GUILayoutUtility.GetAspectRect(2,GUILayout.ExpandWidth(true)),_stateMachinesTex);

            if (DoSwitchWithButton("Renderer Pipeline", ref _target.rendererPipeline))
            {
                
            }
            
            GUILayout.Space(1);
            
            if (DoSwitchWithButton("InputHandler", ref _target.input))
            {
                if (_target.input == SettingsInfo.InputHandler.InputBoth)
                {
                    _target.ImportSample(mtpsCore,"Input New");
                    _target.ImportSample(mtpsCore,"Input Old");
                }
                else
                {
                    string sample = _target.input == SettingsInfo.InputHandler.NewInputSystem
                        ? "Input New"
                        : "Input Old";
                    _target.ImportSample(mtpsCore,sample);
                }
            }
            GUILayout.Space(1);

            if (DoSwitchWithButton("ScriptingType", ref _target.scriptingType))
            {
                if (_target.scriptingType == SettingsInfo.ScriptingType.BothStateMachineTypes)
                {
                    _target.ImportSample(mtpsCore,"Move State Machine");
                    _target.ImportSample(mtpsCore,"Move State Machine VS");
                }
                else
                {
                    string sample = _target.scriptingType == SettingsInfo.ScriptingType.CodeStateMachine
                        ? "Move State Machine"
                        : "Move State Machine VS";
                    _target.ImportSample(mtpsCore,sample);
                }
            }
            serializedObject.ApplyModifiedProperties();
        }

        private bool DoSwitchWithButton<T>(string label,ref T value) where T : Enum
        {
            GUILayout.BeginHorizontal();
            value = (T) EditorGUILayout.EnumPopup(label,value);
            var click = GUILayout.Button(_importText, _importWidth);
            GUILayout.EndHorizontal();
            return click;
        }
    }
}