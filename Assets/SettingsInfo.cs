using System.Linq;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "SettingsInfo"),]
public class SettingsInfo :  ScriptableSingleton<SettingsInfo>
{
    [SerializeField] private TextAsset[] packages;
    
    
    private enum InputHandler
    {
        OldInputManager = 0,
        NewInputSystem = 1,
        InputBoth = 2
    };

    private enum ScriptingType
    {
        CodeStateMachine,
        VisualScriptingStateMachine,
        BothStateMachineTypes
    }
    
    [SerializeField] private InputHandler input;
    [SerializeField] private ScriptingType scriptingType;
    
    
    private void OnValidate()
    {
        input = (InputHandler) GetPropertyOrNull("activeInputHandler").intValue;
      //  AssetDatabase.ImportPackage();
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
