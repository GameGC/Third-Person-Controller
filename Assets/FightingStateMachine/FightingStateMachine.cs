using System.Linq;
using System.Threading.Tasks;
using ThirdPersonController.Code.AnimatedStateMachine;
using ThirdPersonController.Core.StateMachine;
using UnityEngine;

public class FightingStateMachine : CodeStateMachine
{
   public bool hasGetWeaponState = true;
   public bool hasPutWeaponBackState = true;

   public async Task RequestForPutBack()
   {
      if(!hasPutWeaponBackState) return;

      var variables = Variables as IFightingStateMachineVariables;
      variables.RequestedHolsterWeapon = true;
      
      await GetComponent<AnimationLayer>().WaitForAnimationFinish("PutWeaponBackFromAim");
   }
   
   [ContextMenu("ConvertToFighting")]
   public void ConvertToFighting()
   {
      states = gameObject.GetComponent<CodeStateMachine>().states;
   }
}
