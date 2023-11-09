using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Fighting.Pushing;
using GameGC.Collections;
using ThirdPersonController.Core.DI;
using ThirdPersonController.Core.StateMachine;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Animations.Rigging;
using UnityEngine.InputSystem;

public class WeaponSwitch : MonoBehaviour
{
   public STurple<string, CodeStateMachine, Rig>[] weapons;
   public CodeStateMachine currentStateMachine;
   

   private void Update()
   {
      if (Keyboard.current.digit0Key.wasPressedThisFrame)
      {
         Switch(0);
      }
      if (Keyboard.current.digit1Key.wasPressedThisFrame)
      {
         Switch(1);
      }
      if (Keyboard.current.digit2Key.wasPressedThisFrame)
      {
         Switch(2);
      }
      if (Keyboard.current.digit3Key.wasPressedThisFrame)
      {
         Switch(3);
      }
      if (Keyboard.current.digit4Key.wasPressedThisFrame)
      {
         Switch(4);
      }
   }

   private async void Switch(int i)
   {
      // wait for previous Fighting Hide Animation
      if (currentStateMachine !=null && currentStateMachine is FightingStateMachine fighting && fighting.hasPutWeaponBackState)
         await fighting.RequestForPutBack();

      Destroy(currentStateMachine.gameObject);
      
      //assign new Animations      
      var parent = transform.Find("StateMachines");
      var instance = Instantiate(weapons[i].Item2, parent);
      instance.GetComponent<CodeStateMachine>().ReferenceResolver = GetComponent<ReferenceResolver>();

      GetComponent<HybridAnimator>().stateMachines[0] = instance.GetComponent<AnimationLayer>();
      GetComponent<HybridAnimator>().Rebuild(1);

      parent = transform.Find("rig_controllers");
      var builder = GetComponent<RigBuilder>();

      var animator = GetComponent<Animator>();
      animator.enabled = false;

      // remove previous rig
      if(builder.layers.Count>0)
         Destroy(builder.layers[0].rig.gameObject);
      
      //assign new Rig
      if (weapons[i].Item3)
      {
         var rig = Instantiate(weapons[i].Item3, parent);
         rig.name = weapons[i].Item3.name;

         builder.Clear();

         if(builder.layers.Count<1)
            builder.layers.Add((new RigLayer(rig, true)));
         else builder.layers[0] = (new RigLayer(rig, true));
      }
      else
      {
         builder.layers.RemoveAt(0);
      }

      // rebuild everything
      animator.UnbindAllStreamHandles();
      await Task.Delay(1000);
      builder.Build();
      animator.enabled = true;
      currentStateMachine = instance;
   }
}
