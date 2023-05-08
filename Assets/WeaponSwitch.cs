using System;
using System.Collections;
using System.Collections.Generic;
using Fighting.Pushing;
using GameGC.Collections;
using ThirdPersonController.Core.DI;
using ThirdPersonController.Core.StateMachine;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.InputSystem;

public class WeaponSwitch : MonoBehaviour
{
   public STurple<string, GameObject, Rig>[] weapons;

   private void Awake()
   {
      
   }

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
   }

   private void Switch(int i)
   {
      Destroy(GetComponentInChildren<FightingStateMachineVariables>().gameObject);
      var parent = transform.Find("StateMachines");
      var instance = Instantiate(weapons[i].Item2, parent);
      instance.GetComponent<CodeStateMachine>().ReferenceResolver = GetComponent<ReferenceResolver>();

      GetComponent<HybridAnimator>().stateMachines[0] = instance.GetComponent<AnimationLayer>();
      GetComponent<HybridAnimator>().Rebuild(1);

      parent = transform.Find("rig_controllers");
      var builder = GetComponent<RigBuilder>();
      if(builder.layers.Count>0)
         Destroy(builder.layers[0].rig.gameObject);
      builder.Clear();

      if (weapons[i].Item3)
      {
         var rig = Instantiate(weapons[i].Item3, parent);
         if(builder.layers.Count<1)
            builder.layers.Add(default);
         builder.layers[0] = (new RigLayer(rig, true));
      }
      else
      {
         builder.layers.RemoveAt(0);
      }

      builder.Build();
   }
}
