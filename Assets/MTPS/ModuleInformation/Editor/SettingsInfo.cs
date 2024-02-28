using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.PackageManager.UI;
using UnityEngine;
using UnityEngine.Rendering;

namespace ThirdPersonController.ModuleInformation.Editor
{
    internal class SettingsInfo :  ScriptableObject
    {
        [Serializable]
        private struct PackagePathCollection
        {
            public string PackageName;

            public string CharacterPrefabPath;
            public string VSCharacterPrefabPath;

            public string InputOldPrefabPath;
            public string InputNewPrefabPath;
        }

        [SerializeField] private PackagePathCollection[] pathCollections;
        
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

            var packagePathCollection = settings.pathCollections.Last();
            string inputPath = Path.Combine("Assets/MTPS/",
                (int) settings.input > 0
                    ? packagePathCollection.InputNewPrefabPath
                    : packagePathCollection.InputOldPrefabPath);
            string characterPath = Path.Combine("Assets/MTPS/", 
                (settings.scriptingType == ScriptingType.CodeStateMachine || settings.scriptingType == ScriptingType.BothStateMachineTypes) || string.IsNullOrWhiteSpace(packagePathCollection.VSCharacterPrefabPath)
                ? packagePathCollection.CharacterPrefabPath
                    : packagePathCollection.VSCharacterPrefabPath);

            TPSMoveDemoSceneTemplatePipeline.InputPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(inputPath);
            TPSMoveDemoSceneTemplatePipeline.CharacterPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(characterPath);
        }

        public void ImportSample(string sampleName)
        {
            foreach (var packagePathCollection in pathCollections)
                ImportSample(packagePathCollection.PackageName, sampleName);
        }

        private static void ImportSample(string packageName,string sampleName)
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