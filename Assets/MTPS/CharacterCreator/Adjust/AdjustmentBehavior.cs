using MTPS.Core;
using MTPS.Core.CodeStateMachine;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using MTPS.Inventory;
using MTPS.Inventory.ItemTypes;
using MTPS.Shooter.Cameras;

public class AdjustmentBehavior : MonoBehaviour
{
   #region Prefabs

   public GameObject[] characterPrefabs;
   public WeaponData[] weapons;
   
   #endregion

   #region References

   public BlankAdjustmenInput input;
   public CameraManager CameraManager;
   public Transform TargetLook;
   
   #endregion
   public ReferenceResolver _currentCharacter;
   
   public int selectedCharacter;
   public int selectedWeapon;
   
   public void Start()
   {
      if(!_currentCharacter)
         SelectCharacter(selectedCharacter);
      if (CameraManager) 
         CameraManager.UpdateTargets();
   }

   private void OnValidate()
   {
      if(Application.isPlaying) return;
      selectedCharacter = _currentCharacter == null ? -1 : selectedCharacter;
   }

   public void SelectCharacter(int i)
   {
      if (_currentCharacter)
      {
         if(Application.isPlaying) Destroy(_currentCharacter.gameObject);
         else DestroyImmediate(_currentCharacter.gameObject);
      }
      selectedCharacter = i;
      selectedWeapon = -1;
      
      var character = Instantiate(characterPrefabs[i]);
      _currentCharacter = character.GetComponent<ReferenceResolver>();
      
      if (input)
      {
         _currentCharacter.AddCachedComponent(input);
      }

      if (CameraManager)
      {
         _currentCharacter.AddNamedCachedComponent("CameraManager", CameraManager);
         CameraManager.resolver = _currentCharacter;
         CameraManager.UpdateTargets();
      }

      if (TargetLook)
      {
         _currentCharacter.AddNamedCachedComponent("TargetLook", TargetLook);
      }

      if (weapons.Length > 0)
      {
         var inventory = _currentCharacter.gameObject.GetComponent<Inventory>();
         foreach (var t in weapons)
         {
            inventory.AddItemNonEqip(t);
         }
      }
      
      _currentCharacter.isReady = true;
      Hide();
   }

   private void OnGUI()
   {
      if (GUI.Button(new Rect(Screen.width - 100, Screen.height-25, 100, 25), "Rebuild Rig")) 
         RebuildRig();
   }

   public void RebuildRig()
   {
      var builder =  _currentCharacter.GetComponent<RigBuilder>();

      var animator = _currentCharacter.GetComponent<Animator>();
      int prevState = animator.GetCurrentAnimatorStateInfo(0).shortNameHash;
      animator.enabled = false;
        
      // rebuild everything
      builder.Build();
      animator.Rebind();

      animator.enabled = true;
      if (prevState != animator.GetCurrentAnimatorStateInfo(0).shortNameHash) 
         animator.Play(prevState);
   }
   public void SelectWeapon(int i)
   {
      var inventory = _currentCharacter.gameObject.GetComponent<Inventory>();
      if (Application.isPlaying)
      {
#pragma warning disable CS4014
         inventory.Equip(weapons[i]);
#pragma warning restore CS4014
      }
      else 
         inventory.EquipImmediateEditor(weapons[i]);
      selectedWeapon = i;
   }

   [ContextMenu("Hide")]
   private void Hide()
   {
      var character = _currentCharacter?.gameObject;
      foreach (var rootGameObject in gameObject.scene.GetRootGameObjects())
      {
         if (rootGameObject != gameObject && rootGameObject != character) rootGameObject.hideFlags = HideFlags.HideInHierarchy;
      }

      if (character)
      {
         var hips = character.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.Hips);
         var stateMachineParent = character.GetComponentInChildren<CodeStateMachine>().transform.parent;
         Transform rigParent = character.transform.Find("rig_controllers");

         foreach (var rigLayer in character.GetComponent<RigBuilder>().layers)
         {
            if (rigLayer.rig)
               rigParent = rigLayer.rig.transform.parent;
         }

         foreach (Transform t in character.transform)
         {
            if (t != hips && t != stateMachineParent && t != rigParent) t.hideFlags = HideFlags.HideInHierarchy;
         }
      }
   }
   
   [ContextMenu("UnHide")]
   private void UnHide()
   {
      foreach (var rootGameObject in gameObject.scene.GetRootGameObjects())
      {
         rootGameObject.hideFlags = HideFlags.None;
      }
      var character = _currentCharacter?.gameObject;
      if (character)
      {
         foreach (Transform t in character.transform)
         {
            t.hideFlags = HideFlags.None;
         }
      }
   }
}