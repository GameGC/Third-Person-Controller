using System.Threading.Tasks;
using GameGC.CommonEditorUtils.Attributes;
using MTPS.Core;
using MTPS.Core.CodeStateMachine;
using MTPS.Inventory.ItemTypes;

using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace MTPS.Inventory
{
   [DisallowMultipleComponent]
   public class Inventory : BaseInventory
   {
      public UnityEvent<BaseItemData> onItemEquiped;
      public BaseItemData EquippedItemData => equippedItemData;
      public CodeStateMachine FightingStateMachine => fightingStateMachine;


      [FormerlySerializedAs("equipedItemData")] [SerializeField] [FormerlySerializedAs("equipedItemdData")]
      private BaseItemData equippedItemData;

      [SerializeField] private CodeStateMachine fightingStateMachine;

      [SerializeField,ValidateBaseType(typeof(AudioClip))] private Object equipSound;

      private AudioSource _audioSource;

      private void Awake()
      {
         var soundsObject = new GameObject("InventorySounds").transform;
         soundsObject.SetParent(transform);
         soundsObject.localPosition = Vector3.zero;
         _audioSource = soundsObject.gameObject.AddComponent<AudioSource>();
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

#pragma warning disable CS4014
      public override bool AddItem(BaseItemData itemData, int count = 1)
      {
         var bool_ = base.AddItem(itemData, count);
         if (itemData is WeaponData weaponData)
            Equip(weaponData);
         PlaySound(equipSound);
         return bool_;
      }

      public bool AddItemNonEquip(BaseItemData itemData, int count = 1)
      {
         return base.AddItem(itemData, count);
      }

      private void Equip(int i)
      {
         WeaponData weaponData;
         int index = i;
         while (items.KeysArray[index] is not WeaponData)
         {
            index++;
         }

         weaponData = items.KeysArray[index] as WeaponData;
         Equip(weaponData);
         PlaySound(equipSound);
      }

      public void Equip(BaseItemData data)
      {
         if (data is WeaponData weaponData)
            Equip(weaponData);
         PlaySound(equipSound);
      }
#pragma warning restore CS4014


      public async Task Equip(WeaponData weaponData)
      {
         if (weaponData == equippedItemData) return;

         Destroy(fightingStateMachine.gameObject);



         equippedItemData = weaponData;

         onItemEquiped.Invoke(weaponData);
      }
      
      private void PlaySound(Object audioType)
      {
         switch (audioType)
         {
            case null: {_audioSource.Stop(); return;}
            case AudioClip clip:
            {
               if (!_audioSource.isPlaying || _audioSource.clip != clip) 
                  PlayClip(clip);
               return;
            }
         }
      }
    
      private void PlayClip(AudioClip clip, float volume = 1f,float pitch = 1f)
      {
         if (_audioSource.isPlaying) 
            _audioSource.Stop();
         _audioSource.clip = clip;
         _audioSource.spatialBlend = 0f;
         _audioSource.volume = volume;
         _audioSource.pitch = pitch;
         _audioSource.Play();

         _audioSource.SetScheduledEndTime( AudioSettings.dspTime + 3.0F+clip.length);
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
         EquipImmediateEditor(weaponData);
      }

      public void EquipImmediateEditor(WeaponData weaponData)
      {
         if (fightingStateMachine)
         {
            if (PrefabUtility.IsAnyPrefabInstanceRoot(gameObject))
               PrefabUtility.UnpackPrefabInstance(gameObject, PrefabUnpackMode.OutermostRoot,
                  InteractionMode.AutomatedAction);
            DestroyImmediate(fightingStateMachine.gameObject);
         }


         //assign new Animations      
         var stateMachineParent = transform.Find("StateMachines");
         var instance =
            PrefabUtility.InstantiatePrefab(weaponData.stateMachine, stateMachineParent) as CodeStateMachine;
         instance.ReferenceResolver = GetComponent<ReferenceResolver>();


         equippedItemData = weaponData;
         fightingStateMachine = instance;
      }
   }
}