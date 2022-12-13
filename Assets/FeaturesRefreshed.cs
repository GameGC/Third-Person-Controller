using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ThirdPersonController.Core;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class FeaturesRefreshed : MonoBehaviour
{
    [MenuItem("Tools/UpdateFeatures")]
    public static void UpdateFeatures()
    {
        var targetAssembly = typeof(BaseFeature).Assembly;
        var targetTypes = targetAssembly.GetTypes().Where(t => t.IsSubclassOf(typeof(BaseFeature)));

        
        foreach (var typeOption in targetTypes)
        {
            Debug.Log(typeOption.Name);
            if (!BoltCore.Configuration.typeOptions.Contains(typeOption))
            {
                BoltCore.Configuration.typeOptions.Add(typeOption);
            }

        }
        BoltCore.Configuration.Save();
    }
}
