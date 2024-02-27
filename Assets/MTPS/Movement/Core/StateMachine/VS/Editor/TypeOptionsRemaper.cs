using System;
using System.Collections.Generic;
using GameGC.CommonEditorUtils.Editor;
using MTPS.Core;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

namespace MTPS.Movement.Core.StateMachine.VS.Editor
{
    public class TypeOptionsRemaper : MonoBehaviour
    {
        [MenuItem("Tools/MTPS/UpdateFeatures")]
        public static void RemapTypeOptions()
        {
                var targetAssembly = typeof(BaseFeature).Assembly;
            var targetTypes = GetNonAbstractTypesSubclassOf(typeof(BaseFeature));

            targetTypes.Add(typeof(BaseFeature));
            targetTypes.Add(typeof(ReferenceResolver));
            targetTypes.Add(typeof(IBaseInputReader));
            targetTypes.Add(typeof(object));
            targetTypes.AddRange(GetNonAbstractTypesWithInterface(typeof(IBaseInputReader)));    
            targetTypes.AddRange(GetNonAbstractTypesWithInterface(typeof(IStateMachineVariables)));
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
        
        private static List<Type> GetNonAbstractTypesSubclassOf(Type parentType)
        {
            List<Type> types = AllTypesContainer.AllTypes
                .FindAll(type => type.IsClass && !type.IsAbstract && type.IsSubclassOf(parentType));

            return types;
        }
        
        private static List<Type> GetNonAbstractTypesWithInterface(Type parentType)
        {
            List<Type> types = AllTypesContainer.AllTypes
                .FindAll(parentType.IsAssignableFrom);

            return types;
        }
    }
}
