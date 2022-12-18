using System.Linq;
using ThirdPersonController.Core;
using ThirdPersonController.Core.DI;
using ThirdPersonController.Input;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

namespace ThirdPersonController.Code.MovementStateMachine.VisualScripting.Editor
{
    public class TypeOptionsRemaper : MonoBehaviour
    {
        [MenuItem("Tools/UpdateFeatures")]
        public static void RemapTypeOptions()
        {
            var targetAssembly = typeof(BaseFeature).Assembly;
            var targetTypes = targetAssembly.GetTypes().Where(t => t.IsSubclassOf(typeof(BaseFeature))).ToList();

            targetTypes.Add(typeof(BaseFeature));
            targetTypes.Add(typeof(ReferenceResolver));
            targetTypes.Add(typeof(BaseInputReader));
        
            foreach (var typeOption in targetTypes)
            {
                if (!BoltCore.Configuration.typeOptions.Contains(typeOption))
                {
                    BoltCore.Configuration.typeOptions.Add(typeOption);
                }
            }
            BoltCore.Configuration.Save(); 
            UnitBase.Rebuild();
        }
    }
}
