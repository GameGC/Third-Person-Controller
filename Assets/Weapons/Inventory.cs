using System;
using System.Collections.Generic;
using System.Linq;
using GameGC.Collections;
using ThirdPersonController.Code.AnimatedStateMachine;
using ThirdPersonController.Core.CodeStateMachine.CustomEditor.Editor;
using ThirdPersonController.Core.DI;
using ThirdPersonController.Core.StateMachine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

public class Inventory : BaseInventory
{
   public UnityEvent<BaseItemData> onItemEquiped;
   
   [FormerlySerializedAs("weaponStateMachine")] [FormerlySerializedAs("currentStateMachine")] public CodeStateMachine fightingStateMachine;
   
   private HybridAnimator _hybridAnimator;
   private Animator _animator;
   private RigBuilderFixed _rigBuilder;


   public PlayerHUD hud;

   private void Awake()
   {
      _hybridAnimator = GetComponent<HybridAnimator>();
      _animator = GetComponent<Animator>();
      _rigBuilder = GetComponent<RigBuilderFixed>();

      hud.DisplayAllWeapon(items.KeysArray,fightingStateMachine.GetComponent<IFightingStateMachineVariables>());
   }

   private void Update()
   {
      if (Keyboard.current.digit0Key.wasPressedThisFrame)
      {
         Equip(0);
      }
      if (Keyboard.current.digit1Key.wasPressedThisFrame)
      {
         Equip(1);
      }
      if (Keyboard.current.digit2Key.wasPressedThisFrame)
      {
         Equip(2);
      }
      if (Keyboard.current.digit3Key.wasPressedThisFrame)
      {
         Equip(3);
      }
      if (Keyboard.current.digit4Key.wasPressedThisFrame)
      {
         Equip(4);
      }
   }

   public override bool AddItem(BaseItemData itemData, int count = 1)
   {
      var bool_ = base.AddItem(itemData, count);
      if(itemData is WeaponData weaponData)
         Equip(weaponData);
      return bool_;
   }

   private void Equip(int i)
   {
      WeaponData weaponData;
      int index =i;
      while (items.KeysArray[index] is not WeaponData)
      {
         index++;
      }
      weaponData = items.KeysArray[index] as WeaponData;
      Equip(weaponData);
   }
   public void Equip(BaseItemData data)
   {
      if(data is WeaponData weaponData)
         Equip(weaponData);
      
      onItemEquiped.Invoke(data);
   }
   
   public async void Equip(WeaponData weaponData)
   {
      // wait for previous Fighting Hide Animation
      if (fightingStateMachine !=null && fightingStateMachine is FightingStateMachine fighting && fighting.hasPutWeaponBackState)
         await fighting.RequestForPutBack();

      Destroy(fightingStateMachine.gameObject);

      var stateMachineParent = transform.Find("rig_controllers");

      int prevState = _animator.GetCurrentAnimatorStateInfo(0).shortNameHash;
      _animator.enabled = false;

      // remove previous rig
      if(_rigBuilder.layers.Count>0 && _rigBuilder.layers[0].rig)
         Destroy(_rigBuilder.layers[0].rig.gameObject);
      
      //assign new Rig
      if (weaponData.rigLayer)
      {
         var rig = Instantiate(weaponData.rigLayer, stateMachineParent);
         rig.name = weaponData.rigLayer.name;

         _rigBuilder.Clear();

         _rigBuilder.layers[(int)RigTypes.Fighting] = new RigLayer(rig);
      }
      else
      {
         _rigBuilder.layers[(int) RigTypes.Fighting] = new RigLayer(null);
      }

      //assign new Animations      
      stateMachineParent = transform.Find("StateMachines");
      var instance = Instantiate(weaponData.stateMachine, stateMachineParent);
      instance.ReferenceResolver = GetComponent<ReferenceResolver>();

      _hybridAnimator.stateMachines[0] = instance.GetComponent<AnimationLayer>();
      _hybridAnimator.Rebuild(1);
      
      
      // rebuild everything
      _rigBuilder.Build();
      _animator.Rebind();

      _animator.enabled = true;
      if (prevState != _animator.GetCurrentAnimatorStateInfo(0).shortNameHash)
      {
         _animator.Play(prevState);
      }
      fightingStateMachine = instance;
      
      hud.SetSelection(items.KeysArray.IndexOf(weaponData));
      hud.SetVariables(fightingStateMachine.GetComponent<IFightingStateMachineVariables>());
   }

   public void EquipImmediateEditor(int i)
   {
      WeaponData weaponData;
      int index = i;
      while (items.KeysArray[index] is not WeaponData)
      {
         index++;
      }
      weaponData = items.KeysArray[index] as WeaponData;
      
      if(fightingStateMachine)
         DestroyImmediate(fightingStateMachine.gameObject);
      
      //assign new Animations      
      var stateMachineParent = transform.Find("StateMachines");
      var instance = PrefabUtility.InstantiatePrefab(weaponData.stateMachine, stateMachineParent) as CodeStateMachine;
      instance.ReferenceResolver = GetComponent<ReferenceResolver>();

      GetComponent<HybridAnimator>().stateMachines[0] = instance.GetComponent<AnimationLayer>();

      stateMachineParent = transform.Find("rig_controllers");
      var builder = GetComponent<RigBuilder>();

      // remove previous rig
      if(builder.layers.Count>0 && builder.layers[0].rig)
         DestroyImmediate(builder.layers[0].rig.gameObject);
      
      //assign new Rig
      if (weaponData.rigLayer)
      {
         var rig = PrefabUtility.InstantiatePrefab(weaponData.rigLayer, stateMachineParent) as Rig;
         rig.name = weaponData.rigLayer.name;

         builder.Clear();

         if(builder.layers.Count<1)
            builder.layers.Add(new RigLayer(rig, true));
         else builder.layers[0] = new RigLayer(rig, true);
      }
      else
      {
         builder.layers.RemoveAt(0);
      }

      fightingStateMachine = instance;
   }
}


public class BaseInventory  : MonoBehaviour
{
   public SKeyValueList<BaseItemData, int> AllItems => items;
   
   [SerializeField] protected SKeyValueList<BaseItemData,int> items;

   public List<BaseItemData> removeExceptions;

   public UnityEvent<BaseItemData, int> onItemAdded;
   public UnityEvent<BaseItemData, int> onItemMinus;
   public UnityEvent<BaseItemData> onItemRemoved;

   public virtual bool AddItem(BaseItemData itemData,int count = 1)
   {
      int prevCount = items[itemData];
      if (itemData.maxAmmoInInventory > prevCount + count)
      {
         items[itemData] += count;
         onItemAdded.Invoke(itemData,count);
         return true;
      }
      return false;
   }
   public bool MinusItem(BaseItemData itemData,int count = 1)
   {
      if (itemData is WeaponData) return false;
      
      if (items[itemData]-count > -1)
      {
         items[itemData]-=count;
         
         if (items[itemData] < 1) 
            RemoveItem(itemData);
         onItemMinus.Invoke(itemData,count);
         return true;
      }
      return false;
   }
   
   public bool RemoveItem(BaseItemData itemData)
   {
      if (removeExceptions.Contains(itemData))
         return false;

      onItemRemoved.Invoke(itemData);
      return items.Remove(itemData);
   }


#if UNITY_EDITOR
   public SKeyValueList<BaseItemData, int> EDItorREF => items;
#endif

}

[CustomEditor(typeof(Inventory))]
public class InventoryEditor : Editor
{
   private string[] names;
   private int Selected;

   StyleLength w50
   {
      get
      { 
         var style =  new StyleLength();
         style.value = new Length(50,LengthUnit.Percent);
         return style;
      }
   }

   private void OnEnable() => names = (target as Inventory).EDItorREF.KeysArray.Select(w => w.name).ToArray();

   private void OnValidate()
   {
      names = (target as Inventory).EDItorREF.KeysArray.Select(w => w.name).ToArray();
      choises.choices = new List<string>(names);
   }

   private PopupField<string> choises;
   public override VisualElement CreateInspectorGUI()
   {
      var root = new VisualElement();
      
      var horizontalScope = new VisualElement();
      horizontalScope.style.flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row);
      
      var button = new Button(Switch);
      button.text = "Switch GUN:";
      button.style.width = w50;
      horizontalScope.Add(button);
      

      choises = new PopupField<string>(names.ToList(), Selected)
      {
         style =
         {
            width = w50,
            paddingRight = new StyleLength(6),
            marginRight = 0,
            marginLeft = 0
         }
      };
      choises.RegisterValueChangedCallback(OnPopupValueChanged);
     

      horizontalScope.Add(choises);
      
      root.Add(horizontalScope);
      
      GCUIElementsUtils.FillDefaultInspectorWithExclude(root,serializedObject,this, new []{"m_Script"});
      return root;
   }

   private void OnPopupValueChanged(ChangeEvent<string> evt)
   {
      var target = evt.target as PopupField<string>;
      Selected = target.index;
   }

   private void Switch()
   {
      (target as Inventory).EquipImmediateEditor(Selected);
   }
   //public override void OnInspectorGUI()
   //{
   //   GUILayout.BeginHorizontal();
   //   if (GUILayout.Button("Switch GUN:"))
   //   {
   //      (target as WeaponSwitch).SwitchImmediateEditor(Selected);
   //   }
   //   Selected = EditorGUILayout.Popup(Selected, names);
   //   GUILayout.EndHorizontal();
   //   base.OnInspectorGUI();
   //}
}