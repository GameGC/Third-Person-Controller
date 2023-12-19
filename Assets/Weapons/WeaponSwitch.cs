using System.Linq;
using ThirdPersonController.Code.AnimatedStateMachine;
using ThirdPersonController.Core.DI;
using ThirdPersonController.Core.StateMachine;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.InputSystem;

public class WeaponSwitch : MonoBehaviour
{
   public WeaponData[] weapons;

   public CodeStateMachine currentStateMachine;

   public PlayerHUD hud;

   private void Awake()
   {
      hud.DisplayAllWeapon(weapons,currentStateMachine.GetComponent<IFightingStateMachineVariables>());
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
      if (Keyboard.current.digit3Key.wasPressedThisFrame)
      {
         Switch(3);
      }
      if (Keyboard.current.digit4Key.wasPressedThisFrame)
      {
         Switch(4);
      }
   }

   public async void Switch(int i)
   {
      // wait for previous Fighting Hide Animation
      if (currentStateMachine !=null && currentStateMachine is FightingStateMachine fighting && fighting.hasPutWeaponBackState)
         await fighting.RequestForPutBack();

      Destroy(currentStateMachine.gameObject);

      var stateMachineParent = transform.Find("rig_controllers");
      var builder = GetComponent<RigBuilder>();

      var animator = GetComponent<Animator>();
      int prevState = animator.GetCurrentAnimatorStateInfo(0).shortNameHash;
      animator.enabled = false;

      // remove previous rig
      if(builder.layers.Count>0 && builder.layers[0].rig)
         Destroy(builder.layers[0].rig.gameObject);
      
      //assign new Rig
      if (weapons[i].rigLayer)
      {
         var rig = Instantiate(weapons[i].rigLayer, stateMachineParent);
         rig.name = weapons[i].rigLayer.name;

         builder.Clear();

         if(builder.layers.Count<1)
            builder.layers.Add(new RigLayer(rig, true));
         else builder.layers[0] = new RigLayer(rig, true);
      }
      else
      {
         builder.layers.RemoveAt(0);
      }

      //assign new Animations      
      stateMachineParent = transform.Find("StateMachines");
      var instance = Instantiate(weapons[i].stateMachine, stateMachineParent);
      instance.GetComponent<CodeStateMachine>().ReferenceResolver = GetComponent<ReferenceResolver>();

      GetComponent<HybridAnimator>().stateMachines[0] = instance.GetComponent<AnimationLayer>();
      GetComponent<HybridAnimator>().Rebuild(1);
      
      
      // rebuild everything
      builder.Build();
      animator.Rebind();

      animator.enabled = true;
      if (prevState != animator.GetCurrentAnimatorStateInfo(0).shortNameHash)
      {
         animator.Play(prevState);
      }
      currentStateMachine = instance;
      
      hud.SetSelection(i);
      hud.SetVariables(currentStateMachine.GetComponent<IFightingStateMachineVariables>());
   }

   public void SwitchImmediateEditor(int i)
   {
      DestroyImmediate(currentStateMachine.gameObject);
      
      //assign new Animations      
      var stateMachineParent = transform.Find("StateMachines");
      var instance = Instantiate(weapons[i].stateMachine, stateMachineParent);
      instance.GetComponent<CodeStateMachine>().ReferenceResolver = GetComponent<ReferenceResolver>();

      GetComponent<HybridAnimator>().stateMachines[0] = instance.GetComponent<AnimationLayer>();

      stateMachineParent = transform.Find("rig_controllers");
      var builder = GetComponent<RigBuilder>();

      // remove previous rig
      if(builder.layers.Count>0)
         DestroyImmediate(builder.layers[0].rig.gameObject);
      
      //assign new Rig
      if (weapons[i].rigLayer)
      {
         var rig = Instantiate(weapons[i].rigLayer, stateMachineParent);
         rig.name = weapons[i].rigLayer.name;

         builder.Clear();

         if(builder.layers.Count<1)
            builder.layers.Add(new RigLayer(rig, true));
         else builder.layers[0] = new RigLayer(rig, true);
      }
      else
      {
         builder.layers.RemoveAt(0);
      }

      currentStateMachine = instance;
   }
}

[CustomEditor(typeof(WeaponSwitch))]
public class WeaponSwitchEditor : Editor
{
   private GUIContent[] names;
   private int Selected;
   private void OnEnable()
   {
      names = EditorGUIUtility.TrTempContent((target as WeaponSwitch).weapons.Select(w => w.name).ToArray());
   }

   public override void OnInspectorGUI()
   {
      GUILayout.BeginHorizontal();
      if (GUILayout.Button("Switch GUN:"))
      {
         (target as WeaponSwitch).SwitchImmediateEditor(Selected);
      }
      Selected = EditorGUILayout.Popup(Selected, names);
      GUILayout.EndHorizontal();
      base.OnInspectorGUI();
   }
}