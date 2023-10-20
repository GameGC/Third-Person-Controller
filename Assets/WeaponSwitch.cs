using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
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
      if (Keyboard.current.digit2Key.wasPressedThisFrame)
      {
         Switch(2);
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

      var animator = GetComponent<Animator>();
      animator.enabled = false;

      if (weapons[i].Item3)
      {
         var rig = Instantiate(weapons[i].Item3, parent);
         rig.name = weapons[i].Item3.name;
         //if(builder.layers.Count<1)
         //   builder.layers.Add(default);

         var oldConstaints = builder.layers[0].rig.GetComponentsInChildren<IRigConstraint>(true);
         var newConstaints = rig.GetComponentsInChildren<IRigConstraint>(true);

         foreach (var beh in oldConstaints)
         {
            if (beh is MultiAimConstraint multi)
            {
               int newConst = Array.FindIndex(newConstaints,
                  c => c.component.gameObject.name == multi.gameObject.name &&
                       c.component.transform.GetSiblingIndex() == multi.transform.GetSiblingIndex());

               if (newConst > -1)
               {
                  var newMulti = newConstaints[newConst] as MultiAimConstraint;
                  newMulti.weight = multi.weight;
                  newMulti.data = multi.data;
               }
            }
            else if(beh is TwoBoneIKConstraint two)
            {
               int newConst = Array.FindIndex(newConstaints,
                  c => c.component.gameObject.name == two.gameObject.name &&
                       c.component.transform.GetSiblingIndex() == two.transform.GetSiblingIndex());
               
               if (newConst > -1)
               {
                  var newTwo = newConstaints[newConst] as TwoBoneIKConstraint;
                  newTwo.weight = two.weight;
                  newTwo.data = two.data;
               }
            }
         }
         builder.Clear();

         if(builder.layers.Count>0)
            Destroy(builder.layers[0].rig.gameObject);

         builder.layers[0] = (new RigLayer(rig, true));
      }
      else
      {
         builder.layers.RemoveAt(0);
      }

      animator.UnbindAllStreamHandles();
      builder.Build();
      animator.enabled = true;
   }
}
