using Cinemachine;
using MTPS.Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEditor.SceneTemplate;
using UnityEngine;
using UnityEngine.Animations;

public class TPSMoveDemoSceneTemplatePipeline :  ISceneTemplatePipeline
{
    public static GameObject InputPrefab;
    public static GameObject CharacterPrefab;
    
    public virtual bool IsValidTemplateForInstantiation(SceneTemplateAsset sceneTemplateAsset)
    {
        return true;
    }

    public virtual void BeforeTemplateInstantiation(SceneTemplateAsset sceneTemplateAsset, bool isAdditive, string sceneName)
    {
    }

    public virtual void AfterTemplateInstantiation(SceneTemplateAsset sceneTemplateAsset, Scene scene, bool isAdditive, string sceneName)
    {
        var inputInstance = PrefabUtility.InstantiatePrefab(InputPrefab,scene) as GameObject;
        var inputReader = inputInstance.GetComponent<IBaseInputReader>();
        
        var characterInstance = PrefabUtility.InstantiatePrefab(CharacterPrefab,scene) as GameObject;
        characterInstance.GetComponent<ReferenceResolver>().AddCachedComponent((Component)inputReader);
        
        var freeLook = GameObject.FindObjectOfType<CinemachineFreeLook>();
        freeLook.m_Follow = characterInstance.transform;
        freeLook.m_LookAt = characterInstance.GetComponentInChildren<PositionConstraint>().transform;
        EditorUtility.SetDirty(freeLook);

        EditorSceneManager.SaveScene(scene);
    }
}
