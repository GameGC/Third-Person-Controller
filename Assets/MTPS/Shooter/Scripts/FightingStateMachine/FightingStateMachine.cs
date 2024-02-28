using System;
using System.Threading.Tasks;
using MTPS.Core.CodeStateMachine;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace MTPS.Shooter.FightingStateMachine
{
   [RequireComponent(typeof(FightingStateMachineVariables))]
   public class FightingStateMachine : CodeStateMachine
   {
      public bool hasGetWeaponState = true;
      public bool hasPutWeaponBackState = true;

      public async Task RequestForPutBack()
      {
         if(!hasPutWeaponBackState) return;

         var variables = Variables as IFightingStateMachineVariables;
         variables.RequestedHolsterWeapon = true;
      
         await variables.AnimationLayer.WaitForLastStateFinish();
      }

#if UNITY_EDITOR
   
      [ContextMenu("ConvertToFighting")]
      public void ConvertToFighting()
      {
         states = gameObject.GetComponent<CodeStateMachine>().states;
      }

      [MenuItem("CONTEXT/CodeStateMachine/ConvertToFighting", false, 612)]
      public static void ConvertToFighting2(MenuCommand command)
      {
         var sourceScript = command.context as CodeStateMachine;
         var fighting = sourceScript.gameObject.AddComponent<FightingStateMachine>();
         fighting.states = sourceScript.states;
         fighting.hasGetWeaponState = fighting.states[0].Name == "GetWeapon";  
         fighting.hasPutWeaponBackState = fighting.states[^1].Name == "PutWeaponBackFromAim";

         var allComponents = sourceScript.GetComponents(typeof(Component));
         var componentIndex = Array.IndexOf(allComponents, sourceScript);
         if(componentIndex< allComponents.Length)
            for (int i = 0; i < allComponents.Length  -1 -componentIndex; i++)
               ComponentUtility.MoveComponentUp(fighting);

         DestroyImmediate(sourceScript,true);
      }
#endif
   }
}