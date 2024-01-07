using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ThirdPersonController.ModuleInformation.Editor
{
    public class SettingsInfo :  ScriptableObject
    {
        [Serializable]
        private struct TwoKeysValuePair
        {
            public InputHandler input;
            public ScriptingType type;

            public ModuleInformation package;
        }
        [SerializeField] private TwoKeysValuePair[] packages;
    
    
        public enum InputHandler
        {
            OldInputManager = 0,
            NewInputSystem = 1,
            InputBoth = 2
        };

        public enum ScriptingType
        {
            CodeStateMachine = 0,
            VisualScriptingStateMachine = 1,
            BothStateMachineTypes = 2,
        }
    
        public InputHandler input;
        public ScriptingType scriptingType;


        [InitializeOnLoadMethod]
        public static void SelectOnStart()
        {
            const string kShowSettings = "Tps.Settings.ShowDefaults";
            if (!SessionState.GetBool(kShowSettings, false))
            {
                EditorApplication.delayCall += () =>
                {
                    SessionState.SetBool(kShowSettings, true);
                    Selection.activeObject =AssetDatabase.LoadAssetAtPath<SettingsInfo>(AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("t:"+typeof(SettingsInfo)).First()));
                };
            }

        }



        private void OnValidate()
        {
          //  if(EditorApplication.isCompiling || EditorApplication.isUpdating) return;
            
            var systemInputValue =  (InputHandler) GetPropertyOrNull("activeInputHandler").intValue;

            if (systemInputValue != input)
            {
              //  Selection.activeObject = this;
                return;
                ImportTargetPackages(systemInputValue, scriptingType);
            }
        }

        public void ImportTargetPackages(InputHandler input, ScriptingType scriptingType)
        {
            if(this.input == input && this.scriptingType == scriptingType) return;
        
            foreach (var twoKeysValuePair in packages) 
                twoKeysValuePair.package.UnloadPackage();

            // handle for both situations
            if (input == InputHandler.InputBoth)
            {
                var mType = scriptingType == ScriptingType.BothStateMachineTypes ? ScriptingType.CodeStateMachine : scriptingType;
            
            
                var oldInput = packages.First(p => p.input == InputHandler.OldInputManager && p.type == mType);
                var newInput = packages.First(p => p.input == InputHandler.NewInputSystem  && p.type == mType);

                oldInput.package.LoadPackage();
                newInput.package.LoadPackage();

                if (scriptingType == ScriptingType.BothStateMachineTypes)
                {
                    mType = ScriptingType.VisualScriptingStateMachine;
                    oldInput = packages.First(p => p.input == InputHandler.OldInputManager && p.type == mType);
                    newInput = packages.First(p => p.input == InputHandler.NewInputSystem  && p.type == mType);
                
                    oldInput.package.LoadPackage();
                    newInput.package.LoadPackage();
                }
            }
            else
            {
                if (scriptingType == ScriptingType.BothStateMachineTypes)
                {
                    var codeMachine = packages.First(p => p.input == input && p.type == ScriptingType.CodeStateMachine);
                    var visualMachine = packages.First(p => p.input == input && p.type == ScriptingType.VisualScriptingStateMachine);
                
                    //codeMachine.package.LoadPackage();
                    //visualMachine.package.LoadPackage();

                    codeMachine.package.LoadPackage();
                    visualMachine.package.LoadPackage();
                }
                else
                {
                    var machine = packages.First(p => p.input == input && p.type == scriptingType);
                
                    machine.package.LoadPackage();
                }

            }

           //if (this.input != input)
           //{
                var property =   GetPropertyOrNull("activeInputHandler");
                if (property != null)
                {
                    property.intValue = (int) input;
                    property.serializedObject.ApplyModifiedPropertiesWithoutUndo();
                    property.serializedObject.Update();
                }

                property = GetPropertyOrNull("enableNativePlatformBackendsForNewInputSystem");
                if (property != null)
                {
                    property.boolValue = input == InputHandler.InputBoth || input == InputHandler.NewInputSystem;
                    property.serializedObject.ApplyModifiedProperties();
                    property.serializedObject.Update();
                }
            
                property = GetPropertyOrNull( "disableOldInputManagerSupport");
                if (property != null)
                {
                    property.boolValue = input == InputHandler.NewInputSystem;
                    property.serializedObject.ApplyModifiedProperties();
                }
            
                // property?.serializedObject.Update();

                this.input = input;
            //}

            this.scriptingType = scriptingType;
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }
    
    
    
        private static SerializedProperty GetPropertyOrNull(string name)
        {
            var playerSettings = Resources.FindObjectsOfTypeAll<PlayerSettings>().FirstOrDefault();
            if (playerSettings == null)
                return null;
            var playerSettingsObject = new SerializedObject(playerSettings);
            return playerSettingsObject.FindProperty(name);
        }
    }

    [CustomEditor(typeof(SettingsInfo))]
    public class SettingsInfoEditor : UnityEditor.Editor
    {
        private Texture _inputTypesTex;
        private Texture _stateMachinesTex;
        
        private SettingsInfo _target;
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
            
            
            EditorGUI.BeginChangeCheck();
            var newInput =EditorGUILayout.EnumPopup("InputHandler", _target.input);
            var scriptingType =EditorGUILayout.EnumPopup("ScriptingType", _target.scriptingType);
            if (EditorGUI.EndChangeCheck())
            {
                _target.ImportTargetPackages((SettingsInfo.InputHandler) newInput,(SettingsInfo.ScriptingType) scriptingType);
            }
        }
    }
}