   using System.Threading.Tasks;
using ThirdPersonController.Core.DI;
using ThirdPersonController.Core.StateMachine;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UTPS.FightingStateMachine.Extras;
   using UTPS.Inventory.ItemTypes;

   namespace UTPS.Inventory
{
   public class Inventory : BaseInventory
   {
      public UnityEvent<BaseItemData> onItemEquiped;
      public BaseItemData EquipedItemData => equipedItemData;
      public CodeStateMachine FightingStateMachine => fightingStateMachine;


      [SerializeField] [FormerlySerializedAs("equipedItemdData")]
      private BaseItemData equipedItemData;

      [SerializeField] private CodeStateMachine fightingStateMachine;


      private HybridAnimator _hybridAnimator;
      private Animator _animator;
      private RigBuilderFixed _rigBuilder;

      private void Awake()
      {
         _hybridAnimator = GetComponent<HybridAnimator>();
         _animator = GetComponent<Animator>();
         _rigBuilder = GetComponent<RigBuilderFixed>();
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
      }

      public void Equip(BaseItemData data)
      {
         if (data is WeaponData weaponData)
            Equip(weaponData);
      }
#pragma warning restore CS4014


      public async Task Equip(WeaponData weaponData)
      {
         if (weaponData == equipedItemData) return;

         // wait for previous Fighting Hide Animation
         if (fightingStateMachine != null && fightingStateMachine is global::FightingStateMachine fighting &&
             fighting.hasPutWeaponBackState)
            await fighting.RequestForPutBack();

         Destroy(fightingStateMachine.gameObject);

         var stateMachineParent = transform.Find("rig_controllers");

         int prevState = _animator.GetCurrentAnimatorStateInfo(0).shortNameHash;
         _animator.enabled = false;

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

         equipedItemData = weaponData;

         onItemEquiped.Invoke(weaponData);
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
         _animator = GetComponent<Animator>();
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

         equipedItemData = weaponData;
         fightingStateMachine = instance;
      }
   }
}