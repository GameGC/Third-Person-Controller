using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GameGC.Collections;
using ThirdPersonController.Core.CodeStateMachine;
using ThirdPersonController.Core.StateMachine;
using UnityEngine;

public class FightingStateMachine : CodeStateMachine
{
   public bool hasGetWeaponState = true;
   public bool hasPutWeaponBackState = true;

   public async Task WaitForPutBack()
   {
      CurrentState = states.First(s=>s.Name == "PutWeaponBackFromAim");
      onStateChanged.Invoke();
      CurrentState.OnEnterState();
      
      if(hasPutWeaponBackState)
         await GetComponent<AnimationLayer>().WaitForAnimationFinish("PutWeaponBackFromAim");
   }
   
   [ContextMenu("ConvertToFighting")]
   public void ConvertToFighting()
   {
      states = gameObject.GetComponent<CodeStateMachine>().states;
   }
}
