using System.Threading.Tasks;
using GameGC.CommonEditorUtils.Attributes;
using GameGC.SurfaceSystem.Audio;
using MTPS.Core;
using MTPS.Core.CodeStateMachine;
using MTPS.FightingStateMachine.Extras;
using MTPS.Inventory.ItemTypes;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations.Rigging;
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

      [SerializeField,ValidateBaseType(typeof(AudioClip),typeof(IAudioType))] private Object equipSound;

      private HybridAnimator _hybridAnimator;
      private Animator _animator;
      private RigBuilderFixed _rigBuilder;
      private AudioSource _audioSource;

      private void Awake()
      {
         _hybridAnimator = GetComponent<HybridAnimator>();
         _animator = GetComponent<Animator>();
         _rigBuilder = GetComponent<RigBuilderFixed>();
         
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

      public bool AddItemNonEqip(BaseItemData itemData, int count = 1)
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

         bool hasAnimatorOverride = fightingStateMachine.GetComponent<WeaponAnimatorOverride>();
         
         // wait for previous Fighting Hide Animation
         if (fightingStateMachine != null && fightingStateMachine is global::MTPS.Shooter.FightingStateMachine.FightingStateMachine fighting &&
             fighting.hasPutWeaponBackState)
            await fighting.RequestForPutBack();

         Destroy(fightingStateMachine.gameObject);

         var stateMachineParent = transform.Find("rig_controllers");

         // remove previous rig
         if (_rigBuilder.layers.Count > 0 && _rigBuilder.layers[0].rig)
            Destroy(_rigBuilder.layers[0].rig.gameObject);

         //assign new Rig
         if (weaponData.rigLayer)
         {
            var rig = Instantiate(weaponData.rigLayer, stateMachineParent);
            rig.name = weaponData.rigLayer.name;

            _rigBuilder.layers[(int) RigTypes.Fighting] = new RigLayer(rig);
         }
         else
         {
            _rigBuilder.layers[(int) RigTypes.Fighting] = new RigLayer(null);
         }

         //assign new Animations      
         stateMachineParent = transform.Find("StateMachines");
         var instance = Instantiate(weaponData.stateMachine, stateMachineParent);
         if (instance.GetComponent<WeaponAnimatorOverride>())
            hasAnimatorOverride = true;
         instance.ReferenceResolver = GetComponent<ReferenceResolver>();

         _hybridAnimator.stateMachines[0] = instance.GetComponent<AnimationLayer>();
         _hybridAnimator.Rebuild(1);

         _rigBuilder.Build();

         fightingStateMachine = instance;
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
            case IAudioType audio:
            {
               audio.Play(_audioSource);
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

         _hybridAnimator = GetComponent<HybridAnimator>();
         _rigBuilder = GetComponent<RigBuilderFixed>();

         //assign new Animations      
         var stateMachineParent = transform.Find("StateMachines");
         var instance =
            PrefabUtility.InstantiatePrefab(weaponData.stateMachine, stateMachineParent) as CodeStateMachine;
         instance.ReferenceResolver = GetComponent<ReferenceResolver>();

         _hybridAnimator.stateMachines[0] = instance.GetComponent<AnimationLayer>();

         stateMachineParent = transform.Find("rig_controllers");

         // remove previous rig
         if (_rigBuilder.layers.Count > 0 && _rigBuilder.layers[(int) RigTypes.Fighting].rig)
            DestroyImmediate(_rigBuilder.layers[(int) RigTypes.Fighting].rig.gameObject);

         //assign new Rig
         if (weaponData.rigLayer)
         {
            var rig = PrefabUtility.InstantiatePrefab(weaponData.rigLayer, stateMachineParent) as Rig;
            rig.name = weaponData.rigLayer.name;

            _rigBuilder.layers[(int) RigTypes.Fighting] = new RigLayer(rig);
         }
         else
         {
            _rigBuilder.layers[(int) RigTypes.Fighting] = new RigLayer(null);
         }

         equippedItemData = weaponData;
         fightingStateMachine = instance;
      }
   }
}