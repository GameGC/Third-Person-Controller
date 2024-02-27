using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.AccessControl;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.UI;
using UnityEngine;
using UnityEngine.Rendering;

namespace ThirdPersonController.ModuleInformation.Editor
{
    internal class SettingsInfo :  ScriptableObject
    {
        [Serializable]
        private struct TwoKeysValuePair
        {
            public InputHandler input;
            public ScriptingType type;

            public ModuleInformation package;
        }
        [SerializeField] private TwoKeysValuePair[] packages;
    
    
        public enum RendererPipeline
        {
            BuildIn = 0,
            Universal = 1,
            HDRP = 2
        }
        
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
        public RendererPipeline rendererPipeline;


        [InitializeOnLoadMethod]
        public static void SelectOnStart()
        {
            const string kShowSettings = "MTps.Settings.ShowDefaults";
            if (!SessionState.GetBool(kShowSettings, false))
            {
                EditorApplication.delayCall += () =>
                {
                    SessionState.SetBool(kShowSettings, true);
                    Selection.activeObject =AssetDatabase.LoadAssetAtPath<SettingsInfo>(AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("t:"+typeof(SettingsInfo)).First()));
                };
            }

            PassReferences();
        }

        private static void PassReferences()
        {
            var settings = AssetDatabase.LoadAssetAtPath<SettingsInfo>(
                AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("t:" + typeof(SettingsInfo)).First()));

            string inputPath = Path.Combine("Assets/",
                (int) settings.input > 0
                    ? "MTPS/Movement/Demo/Input/InputNew.prefab"
                    : "MTPS/Movement/Demo/Input/InputOld.prefab");
            string characterPath = Path.Combine("Assets/", 
                (settings.scriptingType == ScriptingType.CodeStateMachine || settings.scriptingType == ScriptingType.BothStateMachineTypes)
                ? "MTPS/Movement/Demo/Prefabs/Character/Default/MoveDemoCharacter.prefab"
                    : "MTPS/Movement/Demo/Prefabs/Character/VSStateMachinew/MoveDemoCharacterVC.prefab");

            TPSMoveDemoSceneTemplatePipeline.InputPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(inputPath);
            TPSMoveDemoSceneTemplatePipeline.CharacterPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(characterPath);
        }


        public void ImportSample(string packageName,string sampleName)
        {
            string jsonText = File.ReadAllText("Packages/manifest.json");
            var json = JObject.Parse(jsonText);
            
            var values = json["dependencies"].ToObject<Dictionary<string, string>>();
            if (!values.TryGetValue(packageName, out var packageVersion)) return;

            if (packageVersion.EndsWith("git"))
                packageVersion = "1.0.0";
            
            Sample sample = default;
            try
            {
                sample = Sample.FindByPackage(packageName, packageVersion).FirstOrDefault(s=>s.displayName == sampleName);
            }
            catch (Exception)
            {
                // ignored
            }
            if(string.IsNullOrEmpty(sample.resolvedPath)) return;
            
            string targetPath = Path.Combine(Application.dataPath, "MTPS");

            var proxyField = typeof(Sample).GetField("m_IOProxy", BindingFlags.Instance | BindingFlags.NonPublic);
            var proxyValue = proxyField.GetValue(sample);
            
            var dirCopyMethod =proxyValue.GetType()
                .GetMethod("DirectoryCopy", BindingFlags.Public | BindingFlags.Instance);

            string[] files = Directory.GetFiles(sample.resolvedPath, "*", SearchOption.AllDirectories);
            foreach (var file in Directory.GetFiles(targetPath,"*",SearchOption.AllDirectories))
            {
                foreach (var s in files)
                {
                    if (s.Replace(sample.resolvedPath,"") == file.Replace(targetPath,""))
                    {
                        File.Delete(file);
                        break;
                    }
                }
            }
            
            Action<string, float> progressAction = (fileName, progress) =>
            {
                string info = fileName.Replace(sample.resolvedPath + Path.DirectorySeparatorChar.ToString(), "");
                EditorUtility.DisplayProgressBar(L10n.Tr("Copying samples files"), info, progress);
            };
            dirCopyMethod.Invoke(proxyValue, new object[] {sample.resolvedPath, targetPath, true, progressAction});
            EditorUtility.ClearProgressBar();
            AssetDatabase.Refresh();
            //this.m_IOProxy.DirectoryCopy(sourcePath, this.importPath, true, (Action<string, float>) ((fileName, progress) =>
            //{
            //    string info = fileName.Replace(sourcePath + Path.DirectorySeparatorChar.ToString(), "");
            //    EditorUtility.DisplayProgressBar(Sample.k_CopySamplesFilesTitle, info, progress);
            //}));
        }
        private void OnValidate()
        {
          //  if(EditorApplication.isCompiling || EditorApplication.isUpdating) return;
            
            var systemInputValue =  (InputHandler) GetPropertyOrNull("activeInputHandler").intValue;

            //if (systemInputValue != input)
            //{
            //  //  Selection.activeObject = this;
            //    return;
            //    ImportTargetPackages(systemInputValue, scriptingType);
            //}

            DetectRendererPipeline();
            PassReferences();
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

        private void DetectRendererPipeline()
        {
            if (RenderPipelineManager.currentPipeline == null)
            {
                rendererPipeline = RendererPipeline.BuildIn;
                return;
            }
            switch (RenderPipelineManager.currentPipeline.ToString())
            {
                case "Built-in Pipeline": rendererPipeline = RendererPipeline.BuildIn; break;
            }
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
}