# PROJECT ARCHITECTURE INDEX — MTPS (Multi-Third-Person Shooter)

**Unity 6000.0.48f1 | Input: Both (Legacy + New) | Built-in RP**

---

## TABLE OF CONTENTS

1. [Project Overview](#1-project-overview)
2. [Scene Hierarchy](#2-scene-hierarchy)
3. [Core Framework](#3-core-framework)
4. [Movement System](#4-movement-system)
5. [Shooter System](#5-shooter-system)
6. [Playables Animator](#6-playables-animator)
7. [Input System](#7-input-system)
8. [Third-Party / Utilities](#8-third-party--utilities)
9. [Asset Maps](#9-asset-maps) (Scenes, Prefabs, Materials, Audio, Animations, Models, Shaders, Textures)
10. [Weapon Pack / GTA Map / VS Generated / Editor Tooling](#10-misc-asset-groups)

---

## 1. PROJECT OVERVIEW

```
PROJECT:  MTPS — Third-person shooter framework with modular state machines
ENGINE:   Unity 6000.0.48f1
INPUT:    Both Legacy + New Input System
RENDER:   Built-in RP (no URP/HDRP)

FOLDERS:
  Assets/MTPS/                  Main framework (Movement + Shooter)
  Assets/PlayablesAnimator/     Custom Playables-based animation system
  Assets/FutureFeatures/        WIP (FootIK, UIX editor framework)
  Assets/Installer/             GTA map download/installer
  Assets/StarterAssets/         Unity Starter Assets (environment meshes)
  Assets/Weapons pack/          Third-party weapon models + VFX
  Assets/TestCharacter/         Test character models
  Assets/Unity.VisualScripting.Generated/  Auto-generated VS property providers

ARCHITECTURE PATTERN:
  - Composition-based state machines with [SerializeReference] features
  - Interface-driven DI via ReferenceResolver
  - Playables API for animation layering (not Mecanim-only)
  - Async/await for cross-system coordination (equip, collect, holster)
```

---

## 2. SCENE HIERARCHY (DemoScene — Primary)

```
DemoScene.unity (Assets/MTPS/Shooter/Demo/Scenes/DemoScenes/)

ROOT:
  CameraManager                    [CameraManager, 2 children]
  Canvas                           [Canvas, CanvasScaler, PlayerHUD]
    └── [UI children]              Weapon display, scope overlays
  CharacterController_testModel    [Animator, ReferenceResolver, Rigidbody]
    ├── rig_controllers            Animation Rigging rigs (Collect, Fighting)
    ├── StateMachines              CodeStateMachine instances (Movement, Fighting, Collect)
    ├── CapsuleCollider
    └── [bones/armature]
  Dummy (1)                        [Animator, ReferenceResolver, CapsuleCollider]
  InputNew                         ShooterNewInput (New Input System)
  InputOld                         MovementOldInput (Legacy Input)
  MainCamera                       [Camera, AudioListener, CinemachineBrain]
    └── Cinemachine child
  Map                              23 environment children
  SyrfaceSystem                    SurfaceSystem singleton

ALL 6 SCENES:
  1. Assets/MTPS/CharacterCreator/Adjust/AdjustmentScene.unity
  2. Assets/MTPS/Movement/Demo/Scenes/CodeStateMachineWithNewInput.unity
  3. Assets/MTPS/Movement/Demo/Scenes/LevelWithCamera.unity
  4. Assets/MTPS/Shooter/Demo/Scenes/CodeStateMachineNewInput.unity
  5. Assets/MTPS/Shooter/Demo/Scenes/DemoScenes/DemoScene.unity     ← MAIN
  6. Assets/vstest.unity
```

---

## 3. CORE FRAMEWORK

### 3.1 CodeStateMachine (in DLL — MTPS.Core.CodeStateMachine)

Not in project source. Base class for all state machines.

```
MEMBERS:
  states[]                          Array of state definitions (Name + Features[])
  CurrentState / CurrentStateIndex  Active state tracking
  onStateChanged                    UnityEvent fired on transitions
  ReferenceResolver                 IReferenceResolver for dependency lookup
  startWhenResolverIsReady          Deferred init flag

VIRTUAL LIFECYCLE:
  Awake() → Start() → Update() → FixedUpdate()

PATTERN: Each state holds a BaseFeature[] array. When a state is active,
its features receive OnUpdateState()/OnFixedUpdateState() calls.
```

### 3.2 Feature System (BaseFeature pattern)

```
BaseFeature (DLL — MTPS.Core) lifecycle:
  CacheReferences(IStateMachineVariables, IReferenceResolver)  ← one-time init
  OnEnterState()       ← state activated
  OnUpdateState()      ← Update() while active
  OnFixedUpdateState() ← FixedUpdate() while active
  OnExitState()        ← state deactivated

Serialized via [SerializeReference] + SerializeReferenceAddButton
→ allows polymorphic feature arrays in the Inspector
```

### 3.3 ReferenceResolver (DLL — MTPS.Core)

```
IoC container per character. Provides:
  GetComponent<T>()              Get component on same GO
  GetNamedComponent<T>(name)     Get named child component
  isReady                        bool, true when fully initialized

Features NEVER use FindObjectOfType — always go through ReferenceResolver.
```

### 3.4 IBaseInputReader / IStateMachineVariables (DLL)

```
IBaseInputReader      — Marker interface at root of input hierarchy
IStateMachineVariables — Base interface for variable containers
Both in MTPS.Core namespace.
```

---

## 4. MOVEMENT SYSTEM

### 4.1 Input Layer

```
INTERFACE: Assets/MTPS/Movement/Core/Input/IMoveInput.cs
  Extends: IBaseInputReader
  Properties:
    Vector2 lookInput, Quaternion cameraRotation
    Vector2 moveInput, Vector2 moveInputSmooth
    float moveInputMagnitude, Vector3 moveDirection
    bool isSprinting, isJump, isCrouch, isProne, isRoll
    float movementSmooth, bool isInputFrozen
    MoveToPointDelegate MoveToPoint  (for scripted/AI movement)

NEW INPUT: Assets/MTPS/Movement/Core/Input/New/MovementNewInput.cs
  Implements IMoveInput + InputActions.IMovementActions
  Uses InputSystem, static InputActions singleton
  Camera-relative direction calculation
  Wires CameraInputProvider instances

OLD INPUT: Assets/MTPS/Movement/Core/Input/Old/MovementOldInput.cs
  Implements IMoveInput
  Uses UnityEngine.Input, configurable axis/key names
  Same camera-relative direction logic

CAMERA INPUT: Assets/MTPS/Movement/Core/Input/New/CameraInputProvider.cs
  Bridges IMoveInput → Cinemachine input

INPUT ACTIONS: Assets/MTPS/Movement/Core/Input/New/InputActions.cs
  Auto-generated from .inputactions
  Maps: Movement (Move, Look, Jump, Sprint, Crouch, Prone)
        Shooter (Aim, Attack, LongShoot)
```

### 4.2 Movement State Machine

```
FILE: Assets/MTPS/Movement/Core/StateMachine/Code/MoveStateMachine.cs
  Extends: CodeStateMachine
  Requires: MovementStateMachineVariables

  KEY ADDITION: alwaysExecutedFeatures[]
    Features that run in ALL states regardless of active state
    (e.g., GroundCheckFeature, CheckSlopeFeature, FootStepFeature)
    Execute AFTER the active state's features.

VARIABLES: Assets/MTPS/Movement/Core/StateMachine/Code/MovementStateMachineVariables.cs
  Interface: Assets/MTPS/Movement/Core/IMoveStateMachineVariables.cs
    float MovementSmooth, LayerMask GroundLayer
    bool IsGrounded, RaycastHit GroundHit
    bool IsSlopeBadForMove, float SlopeAngle
    bool JumpCounterElapsed, float MoveSpeed

VS VARIANT: MovementStateMachineVariables_VS.cs (Visual Scripting compatible)
EDITOR: MoveStateMachineEditor.cs
```

### 4.3 Movement Features

```
ALWAYS-EXECUTED (Assets/MTPS/Movement/Features/AlwaysExecuted/):
  GroundCheckFeature.cs    SphereCast ground detection, physics material switching
                           Sets IsGrounded, GroundHit, applies extra gravity
                           3 physics materials: friction, slippy, maxFriction
  CheckSlopeFeature.cs     Slope angle calculation, IsSlopeBadForMove flag
  FootStepFeature.cs       Footstep audio trigger on ground contact

MOVE STATE FEATURES (Assets/MTPS/Movement/Features/Move/):
  BaseMoveFeature.cs       Abstract: MoveCharacter(), RotateToDirection(), UpdateAnimation()
                           Rigidbody velocity + root motion + animation param sync
  MoveFeature.cs           Walk (runningSpeed=4, rotationSpeed=16)
  SprintFeature.cs         Sprint (runningSpeed=6), magnitude boost for new input
  CrouchFeature.cs         Crouch (slower, collider resize)
  ProneFeature.cs          Prone (slowest, flat collider)
  SimpleMoveToPoint.cs     AI/scripted movement to world position

AIR FEATURES (Assets/MTPS/Movement/Features/):
  AirFeature.cs            Airborne (airSpeed=5, jumpWithRigidbodyForce, optional rotation)
  JumpFeature.cs           Jump trigger (jumpHeight=4, timer cooldown)
                           CrossFade to JumpIdle/JumpMove animations

EXTRAS:
  IKPassRedirectorBehavior.cs  Redirects IK pass for specific layers
```

### 4.4 Movement Transitions

```
Assets/MTPS/Movement/Transitions/:
  GroundedStateTransition.cs   IsGrounded check
  CrouchStateTransition.cs     isCrouch check
  ProneStateTransition.cs      isProne check
  RunStateTransition.cs        moveInputMagnitude > threshold
  SprintStateTransition.cs     isSprinting check
```

---

## 5. SHOOTER SYSTEM

### 5.1 Fighting State Machine

```
FILE: Assets/MTPS/Shooter/Scripts/FightingStateMachine/FightingStateMachine.cs
  Extends: CodeStateMachine
  Requires: FightingStateMachineVariables
  Properties: hasGetWeaponState, hasPutWeaponBackState
  Methods: RequestForPutBack() — async holster animation await
  Editor: ConvertToFighting() — converts generic CodeStateMachine

VARIABLES: FightingStateMachineVariables.cs
  Implements: IFightingStateMachineVariables
    GameObject weaponInstance, secondaryWeaponInstance
    AnimationLayer AnimationLayer
    bool couldAttack, isCooldown, isReloading, RequestedHolsterWeapon
    float MinAimingDistance (wall intersection prevention)

INTERFACE: IFightingStateMachineVariables.cs
  Extends: IStateMachineVariables + all combat state fields

EDITOR: FightingStateMachineEditor.cs
```

### 5.2 Fighting Features

```
Assets/MTPS/Shooter/Scripts/FightingStateMachine/Features/:

BaseFeatureWithAwaiters.cs   Base class with async IsRunning flag

COMBAT:
  ShootFeature.cs            Weapon shoot after animation delay
                             Temporarily disables hand aim MultiAimConstraint
  SetCooldownFeature.cs      Sets isCooldown
  ResetCooldownFeature.cs    Clears isCooldown

AIM:
  AimLookFeature.cs          Look-at during aiming
  AimForceLookFeature.cs     Forced look direction

ANIMATION RIGGING:
  BaseRigFeature.cs          Abstract rig weight control
  RigSetFeature.cs           Set rig weight
  RigFadeFeature.cs          Fade rig weight over time

GAMEOBJECT ACTIONS:
  InstantiateWeaponAtPivot.cs     Spawn weapon at pivot
  IntantiateWeaponAtHand.cs       Spawn at hand bone
  InstantiateTwoHandedWeapon.cs   Two-handed spawn
  DestroyWeaponAtHand.cs          Destroy equipped weapon

SCOPE:
  SetScopeActiveFeature.cs       Toggle scope UI
  SetSniperScopeActiveFeature.cs Toggle fullscreen sniper scope

CAMERA:
  SetCameraFeature.cs        Switch CameraType enum

WEAPON OFFSET:
  WeaponOffsetApplyFeature.cs

SOUND:
  PlaySoundFeature.cs

SURFACE:
  FootSurfaceFeature.cs      Footstep surface detection
  StepDecalFeature.cs        Footprint decal spawning

TIMELINE:
  ReadTimelineNotificationsToGun.cs
  SetAnimationReference.cs
  SetReferenceDefault.cs
```

### 5.3 Fighting Transitions

```
Assets/MTPS/Shooter/Scripts/FightingStateMachine/Transitions/:
  AimTransition.cs                    IsAim input
  AttackTransition.cs                 IsAttack input
  CooldownTransition.cs               isCooldown flag
  ReloadingTransition.cs              isReloading flag
  HolsterTransition.cs                RequestedHolsterWeapon
  EndPlayTransition.cs                Animation end detection
  DelayedTransition.cs               Time-based delay
  LongShootTransition.cs             Continuous fire
  MultipleConditionTransition.cs     AND/OR compound
  ProneTransition.cs                 Prone state
  Weight1Transition.cs               Animation weight threshold
  CouldAimNoIntersectTransition.cs   Wall intersection check
```

### 5.4 Collect State Machine

```
VARIABLES: CollectStateMachineVariables.cs
  Implements: ICollectStateMachineVariables
    bool IsCollecting, AnimationLayer, OnCollect delegate

FEATURE: CollectFeature.cs
  Full pickup sequence: walk to item → play animation → equip to inventory
  Uses AvatarMask (left/right body), CollectRigController (IK hands)
  Async: MoveToPoint → WaitForAnimationFinish → PlaceItemInHands

ITEM: ItemDetailedCollect.cs — per-item collect configuration
RIG: CollectRigController.cs — IK target control for collect
```

### 5.5 Weapons System

```
HIERARCHY:
  IDamageSender              { float damage, SurfaceHitType HitType }
  IWeaponInfo                { Shoot(), remainingAmmo, maxAmmo, reloadingOrCooldownTime }
  BaseWeaponWithExtensions   MonoBehaviour base with extension lifecycle
  ShootingWeapon             Main weapon (ammo, cooldown, reload, bullet spawn)

ShootingWeapon.cs:
  spawnPoint                 Transform for bullet instantiation
  ammoImMagazine, cooldownTime, reloadingTime
  Shoot():                   ammo-- → Instantiate bullet → Extensions.OnShoot → ImpulseSource
  Auto-reload when empty, cooldown between shots
  Inventory integration for ammo consumption

EXTENSIONS (BaseWeaponExtension pattern):
  Assets/MTPS/Shooter/Scripts/WeaponsSystem/ShootableWeapon/Extensions/
  BaseWeaponExtension.cs     Virtual: OnShoot, OnBeginReload, OnEndReload, OnBeginCooldown, OnEndCooldown
  WeaponMuzzle.cs            Flash muzzle GO on shoot
  WeaponSounds.cs            Shoot/reload sounds (adaptive speed for reload)
  ShellDispancer.cs          Eject shell casings with physics
  WeaponOffset.cs            Position/rotation offset
  ClipPlaceModule.cs         Magazine clip placement

BULLET:
  DefaultRaycastBullet.cs    Raycast bullet
    Start(): Raycast → damage Character → SurfaceSystem hit effects
    Visual bullet fly to hit point
    Character + Char_Collision layer detection

GRENADE SYSTEM:
  GrenadeBullet.cs                   Projectile physics
  BallisticTrajectoryGenerator.cs    Parabolic path calculation
  BallisticTrajectoryPreview.cs      Visual trajectory preview
  WeaponGrenadeInfo.cs               Grenade config

MELEE: HitZone.cs — hit zone detection
```

### 5.6 Health System

```
HIERARCHY:
  IHealthVariable            { float Health }
  HealthComponent            Health, default hit effect, OnHit()
  CharacterHealthComponent   HitBoxes[], OnHitFeatures[], OnDeathFeatures[]

HealthComponent.cs:
  float Health (default 100)
  OnHit(RaycastHit, IDamageSender) → apply damage + surface effect

CharacterHealthComponent.cs:
  HitBox[]                   Per-collider damage multipliers (head = higher)
  BaseHealthFeature[] OnHitFeatures   Run on every hit
  BaseHealthFeature[] OnDeathFeatures Run when Health <= 0
  Features receive: Destroy, DestroyDelayed, SetActive delegates

FEATURES:
  Core/BaseHealthFeature.cs          Abstract base
  Core/BaseHealthFeatureDrawer.cs    Custom property drawer
  Features/DeathWithReplaceFeature.cs — ragdoll replacement on death
```

### 5.7 Inventory System

```
BASE: Assets/MTPS/Shooter/Scripts/Inventory/BaseInventory.cs
  SKeyValueList<BaseItemData, int> items
  AddItem(), MinusItem(), RemoveItem()
  Events: onItemIncreased, onItemReduced, onItemRemoved

INVENTORY: Inventory.cs (full runtime)
  Equip(WeaponData) — async sequence:
    1. Request previous weapon holster animation
    2. Destroy previous fighting state machine
    3. Swap Animation Rigging rig layer
    4. Instantiate new weapon's state machine prefab
    5. Rebuild HybridAnimator layer + RigBuilder
  Keyboard 0-9 quick equip
  EquipImmediateEditor() — editor-time preview

ITEM DATA (ScriptableObjects):
  BaseItemData.cs   Abstract: name, icon, MaxItemCount
  WeaponData.cs     stateMachine prefab, rigLayer, ammoItem (MaxCount=1)
  AmmoData.cs       bulletPrefab (MaxCount=MaxInt)

COLLECT: ItemCollect.cs — world item pickup component
```

### 5.8 Camera System

```
FILE: Assets/MTPS/Shooter/Scripts/Cameras/CameraManager.cs
  CinemachineVirtualCameraBase[] cameras (indexed by CameraType enum)
  SetActiveCamera(CameraType)    Switch vCam, copy axis values between FreeLook/POV
  ReplaceCamera()                Hot-swap camera prefab at runtime
  CopyXAndY()                    Transfer FreeLook/POV axis state
  UpdateTargets()                Rebind Follow/LookAt/InputReader on all cameras

CameraType enum: CameraType.enum.cs
CameraTargetPlacer.cs — places aim target transform
CameraInputProvider.cs — bridges IMoveInput to Cinemachine input
```

### 5.9 Surface System

```
SurfaceSystem — Singleton (SyrfaceSystem GO in scene)
  OnSurfaceHit(RaycastHit, hitType, effect) → spawn VFX/SFX

SurfaceMaterial.cs — ScriptableObject per surface type
  SurfaceEffect[] per SurfaceHitType enum
  SurfaceHitType: Bullet, FootStep, Melee, etc.
  Created via: [CreateAssetMenu(menuName = "Surface/Material")]
```

### 5.10 Weapons UI

```
PlayerHUD.cs:
  Scope / fullscreen scope toggle
  WeaponItemDisplay[] per-weapon UI slots
  Ammo counter via IWeaponInfo interface
  Listens to Inventory.onItemEquiped

WeaponItemDisplay.cs — individual weapon slot display
```

---

## 6. PLAYABLES ANIMATOR

### 6.1 HybridAnimator

```
FILE: Assets/PlayablesAnimator/Core/HybridAnimator.cs
  PlayableGraph + AnimationLayerMixerPlayable
  Layer 0: AnimatorControllerPlayable (Mecanim base locomotion)
  Layer 1+: AnimationLayer instances (state machine driven)
  
  Rebuild(int i)           Reconnect a single layer (for weapon swap)
  MecanimOverride          AnimatorOverrideController with runtime swap
  OverrideAnimClip()       Runtime clip replacement (by name or clip)
  DiscardAnimClip()        Revert override
  RestoreStates()          Preserve animator state across override changes
```

### 6.2 AnimationLayer

```
FILE: Assets/PlayablesAnimator/Core/AnimationLayer.cs
  Extends: FollowingStateMachine<Object>
  
  States[] can be: AnimationClip | TimelineAsset | AnimationValue | RuntimeAnimatorController
  ConstructPlayable()    Build per-state Playables, connect to mixer
  Weight, AvatarMask, IsAdditive  Layer properties
  Async transitions      Coroutine-based with custom timing
  WaitForNextState(), WaitForAnimationFinish(), WaitForLastStateFinish()
  AnimationTransition[]  Custom transition timings per state pair
  SetCustomVariables()   Pass data to AnimationValue playables
```

### 6.3 FollowingStateMachine\<T\>

```
FILE: Assets/PlayablesAnimator/Core/FollowingStateMachine.cs
  Abstract generic that syncs States[] with CodeStateMachine.states[]
  Auto-detects state add/remove/reorder in editor
  Keeps parallel data (AnimationClip per state) synchronized
```

### 6.4 Supporting Types

```
AnimationTransition.cs        Transition timing config (from→to, duration)
AnimationValue.cs             Abstract Playable wrapper for custom logic
TimelineGraph.cs              Timeline playable management
CrossStateMachineValue.cs     Values shared across state machines
ExtendedAnimationLayer.cs     Extended layer with extra features
AnimationBlendValue1d.cs      1D blend tree as AnimationValue
RandomAnimationAsset.cs       Random clip selection
AnimationEventReceiver.cs     Animation event routing
```

---

## 7. INPUT SYSTEM

### 7.1 New Input System

```
InputActions.cs              Auto-generated from .inputactions asset
  IMovementActions: OnMove, OnLook, OnJump, OnSprint, OnCrouch, OnProne
  IShooterActions: OnAim, OnAttack, OnLongShoot

MovementNewInput.cs          Movement via InputSystem
  Static InputActions singleton
  Camera-relative move direction
  movementSmooth interpolation

ShooterNewInput.cs           Extends MovementNewInput + IShooterInput
  Adds IsAttack, IsAim from Shooter action map
```

### 7.2 Legacy Input

```
MovementOldInput.cs          Input.GetAxis/GetKey
  Configurable axis names (Horizontal, Vertical, Mouse X/Y)
  Configurable key codes (Space=jump, LeftShift=sprint, C=crouch, P=prone)

ShooterOldInput.cs           Extends MovementOldInput + IShooterInput
  Fire1/Fire2 axes for attack/aim

INTERFACES:
  IMoveInput (movement) + IShooterInput (combat) = full player input
  IShooterInput extends IBaseInputReader
```

---

## 8. THIRD-PARTY / UTILITIES

```
ROOT-LEVEL SCRIPTS:
  AltCache.cs                Visual Scripting custom node (value caching with GUID keys)
  GroundMeshScanner.cs       Runtime ground mesh via SphereCast (3 scan modes)
                             Also: ConnectedPlanes utility class
  TestController.cs          Foot IK test (CapsuleCast foot placement, Gizmo visualization)
  NewMonoBehaviourScript.cs  Empty placeholder

INSTALLER (Assets/Installer/):
  Installer.cs               Download/install orchestrator
  DownloaderItem.cs          Individual download task with progress
  CustomWebClient.cs         HTTP client wrapper
  Fixed Button.cs / FixedTouchField.cs  Mobile UI helpers

FUTURE FEATURES (Assets/FutureFeatures/):
  FingersConstraint.cs       Animation Rigging finger IK
  Limb.cs                    Limb IK setup
  NewFootFeature.cs          Foot IK for movement system
  LateUpdateCaller.cs        LateUpdate event bridge
  TestFeature/               WIP feature experiments
  UIX-main/                  Attribute-driven editor UI framework
    Editor: UIXEditor, UIXEditorWindow, UIXElement, UIXFactory
    Engine: Attributes (Callback, USS, UXML, Schedule), Core (BetterType, Timer, TypeCache)

WEAPON PACK SCRIPTS (Assets/Weapons pack/):
  SpawnEffect.cs             Particle spawn utility
  ProximityActivate.cs       Proximity activation
  SimpleCameraController.cs  Demo camera
  SimpleCharacterMotor.cs    Demo character motor
  RampAsset.cs               Gradient ramp for VFX (+ RampAssetEditor.cs)

STARTER ASSETS (Assets/StarterAssets/):
  Triplanar.shader           Environment triplanar mapping
  Environment mesh FBX files (Box, Ramp, Stairs, Wall, Ground, Tunnel, Structure)
```

---

## 9. ASSET MAPS

### 9.1 Scenes (6 total)

```
1. Assets/MTPS/CharacterCreator/Adjust/AdjustmentScene.unity
2. Assets/MTPS/Movement/Demo/Scenes/CodeStateMachineWithNewInput.unity
3. Assets/MTPS/Movement/Demo/Scenes/LevelWithCamera.unity
4. Assets/MTPS/Shooter/Demo/Scenes/CodeStateMachineNewInput.unity
5. Assets/MTPS/Shooter/Demo/Scenes/DemoScenes/DemoScene.unity        ← MAIN
6. Assets/vstest.unity
```

### 9.2 Prefabs (261 total)

```
CHARACTER:
  MTPS/Movement/Demo/Prefabs/CharacterController_testModel.prefab
  MTPS/Movement/Demo/Character/Default/MoveDemoCharacter.prefab
  MTPS/Movement/Demo/Character/Default/MovementMinified.prefab
  MTPS/Movement/Demo/Character/VSStateMachinew/MoveDemoCharacterVC.prefab
  MTPS/Shooter/Demo/Prefabs/Characters/Dummy.prefab
  MTPS/Shooter/Demo/Prefabs/Characters/DeadRagdoll.prefab
  TestCharacter/ — 3 character prefabs

INPUT:
  MTPS/Movement/Demo/Input/InputNew.prefab
  MTPS/Movement/Demo/Input/InputOld.prefab

CAMERA:
  MTPS/Movement/Demo/Prefabs/FollowCam.prefab
  MTPS/Movement/Demo/Prefabs/FollowCamManager.prefab

WEAPONS:
  Pistol: Eagle + FightingPistolLayer + shooter_layer + AimCam + Bullet 0.45
  Rifle:  WPN_AKM + FightingRifleLayer + shooter_layer + AimCam + Shell + cal_7_62x39
  Sniper: AWP Variant + FightingSniperRifleLayer + shooter_layer + AimCam + AimCamFPS
  Grenade: M26 + GranadeShooter + FightingLayer + shooter_layer + AimCam
  Melee:  HandHitBox + MaleeLayer

COLLECT ITEMS: AWP_Collectable, Eagle Collect, M26, WPN_AKM_Collectable

EFFECTS (50+):
  Impact VFX: Bricks/, Ceramics/, Dirt/, Flesh/, Glass/, Ground/, Metal/, Sand/, Stone/, Water/, Wood/
  Footsteps: DefaultFootStep, DirtFootStep, GroundFootStep, HexFootStep, smoke, SimpleStepMark

WEAPON PACK:
  Guns: Ak47, Eagle, Glock, Ithaca, Spas12, AWP
  Grenades: M18 (3 colors), M26 (2 colors), M84, MK141, MK2
  Melee: Baseball_Bat, Fire_Axe, Hatchet, Kama, Knife, Knuckles, Tube
  Mosin: Bipod, Magazine, Mosin, Scope

PRO EFFECTS DECALS: 60+ surface-type decals (Asphalt, Bricks, Concrete, Glass, Metal, Mud, Rock, Sand, Skin, Tile, Wood)

VFX: ParticlePack effects (Fire, Explosion, Magic, Misc, Smoke, Steam, Water, Weapon, Goop)
```

### 9.3 Materials (898 total)

```
CHARACTER: body1, GreyBlue_Mat
ENVIRONMENT: GridWhite_01_Mat, NavyGrid, Grid_01/02_BaseMap
GROUND: asphalt_01/02/03, grass02, brick_01/02/03/04, brick_broken_01
GTA MAP: ~700+ LOD materials (CH1-6, CS1-6, DT, HW, ID, KT, MW, OS, PO, SC, SM, SP, VB)
GTA PROPS: 50+ prop materials (telegraph, dumpsters, trees, benches, streetlights)
SOUTH_LS: 100+ road/building/ground materials
WEAPON PACK: 50+ materials for weapons, VFX, environment
```

### 9.4 Audio Clips (29 total)

```
FOOTSTEPS:
  Metal:   Footsteps_MetalV1_Run_02/03/04, Footsteps_MetalV2_Jump_Land_03
  Grass:   Footsteps_Grass_Run_01/03/04, Footsteps_Grass_Jump_Land_01
  Sand:    Footsteps_Sand_Run_02/10/19, Footsteps_Sand_Jump_Land_02
  Water:   Footsteps_Water_Run_01/02/03, Footsteps_Water_Jump_Big_03

WEAPONS:
  PistolShoot.mp3, RifleShoot.wav, SniperShoot.wav
  PistolReload.wav, RifleReload.mp3, SniperReload.wav
  9mm-pistol-shoot-short-reverb-7152.mp3

IMPACTS: metal bullet impact, sand bullet impact
OTHER: CollectSound.wav, GranadeExplosion.wav, ThrowSound.wav, punch-2-37333.mp3
```

### 9.5 Animations (54 total)

```
MOVEMENT (Assets/MTPS/Movement/Demo/Animations/):
  Move:   Idle, Walk, Run, Sprint (.anim)
  Crouch: Crouch Idle, Crouch Forward
  Prone:  Prone Idle, Prone Forward
  InAir:  Falling, JumpIdle, JumpMove, Land
  Controllers: Locomotion4SideMove, LocomotionOneSideMove

SHOOTER (Assets/MTPS/Shooter/Demo/Animations/):
  Pistol: Aim_Idle, Fire_Single, Relaxed_Idle/Reload/Holster/Unholster (+ .anim Shoot/Reload)
  Rifle:  Aim_Idle, Fire_Single, Fire_Continuous, Relaxed_Idle/Reload/Holster/Unholster
          MoveOverrides: Run_F_Loop, Crouch_Idle_Aiming, Crouch_Walk
  Collect: Picking Up, Grab Torch Floor/Wall
  Pushing: Boxing
  Granade: Shooter_Shot&Reload, Shooter_UpperBodyPoses

OTHER: Mosin.fbx (weapon anim), Ellen idle (ParticlePack)
```

### 9.6 Models (144 total)

```
CHARACTER: GameGCStandartCharacter.fbx, testModel.fbx, Adult_Male.fbx, trevor.obj
LEVEL: Terrain.fbx, map.fbx, StarterAssets/ (Box, Ramp, Stairs, Wall, Ground, Tunnel, Structure)
WEAPONS:
  Guns: ak47, Eagle, Glock, Ithaca, Spas12, AWP, Holder
  Mosin: Bipod, Magazine, Mosin, MosinFinal, Scope
  Grenades: M18, M26, M84, MK141, MK2
  Melee: BaseballBat, FireAxe, Hatchet, Kama, Knife, Knuckles, Tube
  Bullet: Bullet_045 LP + LOD0
ANIMATION FBX: 20+ with baked anims (Pistol/Rifle/Collect/Pushing)
VFX MODELS: IceLance, IceShard, RockDebris, RockSpike, Candle, Ellen, FireFly, Flakes, DropSplash
GTA MAP: ~50 OBJ/FBX (south_ls buildings/roads, foundpropingamemap props, stp vegetation)
```

### 9.7 Shaders (17 total)

```
MTPS:         CrossHair_BuildIn.shader
StarterAssets: Triplanar.shader
PRO Effects:  Distortion, Fire PBR, Fire, Liquid Errosion, Particle Channel Packed/Unlit,
              Particle Specular/Transparent, Flashbang Postprocess, Demo Water
ParticlePack: Ice, Dissolve, FireFly, Respawn, ProjectorTexture
```

### 9.8 Textures (1122+ total)

```
GRID:   Grid_01_BaseMap, Grid_01_Emissive, Grid_01_Normal, Grid_02_BaseMap
GROUND: asphalt_01/02/03 (+ NRM), brick_01/02/03/04 (+ NRM)
UI:     FootIK Icon, Screenshot, gradientBG
GTA MAP: ~1000+ PNG/DDS (south_ls roads/buildings, foundpropingamemap props/trees)
```

---

## 10. MISC ASSET GROUPS

### Weapon Pack (Assets/Weapons pack/)

```
Bullet 0.45/       Bullet mesh (LP + LOD0)
GigantMosinNagant/  Mosin rifle (meshes, materials, prefabs, textures)
Grenades/           M18/M26/M84/MK141/MK2
Guns/               AK47, Eagle, Glock, Ithaca, Spas12, AWP
Melee Weapons/      7 melee weapons
PRO Effects FPS/    60+ decal prefabs + VFX particles + custom shaders
ParticlePack/       Fire, Goop, Magic, Misc, Smoke, Water, Weapon effects
Weapons_ChamferZone/ AKM model (WPN_AKM.FBX)
```

### GTA Map / Installer (Assets/Installer/)

```
Installer system: Installer.cs, DownloaderItem.cs, CustomWebClient.cs
Map data: GTAV_SLOD_MAP.fbx, Map.prefab, trevor character
Materials: ~180 LOD materials
Models: south_ls (buildings/roads), foundpropingamemap (props), props (streetlights), stp (vegetation)
```

### Visual Scripting Generated (Assets/Unity.VisualScripting.Generated/)

```
AotStubs.cs              AOT compilation stubs
Property Providers/      82 auto-generated scripts
  Mirror all MTPS features, transitions, variables
  + Unity types (Rigidbody, UI, InputSystem, Cinemachine, Rigging, ProBuilder)
```

### Editor Tooling

```
MTPS/CharacterCreator/Editor/CharacterCreator.cs         Character setup wizard
MTPS/CharacterCreator/Adjust/                            AdjustmentBehavior + Editor
MTPS/ModuleInformation/Editor/                           Module info window
MTPS/Movement/Core/StateMachine/Code/Editor/             MoveStateMachineEditor
MTPS/Shooter/Scripts/Editor/                             AnimationRiggingEditor
MTPS/Shooter/Scripts/FightingStateMachine/Editor/        FightingStateMachineEditor
MTPS/Shooter/Scripts/WeaponsSystem/.../Editor/            Weapon editors
MTPS/Shooter/Scripts/Inventory/InventoryEditor.cs
MTPS/Shooter/Scripts/HealthSystem/...Editor.cs
PlayablesAnimator/Editor/                                AnimationLayer inspectors
FutureFeatures/UIX-main/Editor/                          UIX framework (UIXEditor, UIXFactory, etc.)
```

---

## ARCHITECTURE DIAGRAM

```
┌──────────────────────────────────────────────────────────────────────┐
│                          INPUT LAYER                                  │
│  ┌────────────────┐  ┌────────────────┐  ┌────────────────────────┐  │
│  │MovementNewInput │  │MovementOldInput │  │ShooterNew/OldInput     │  │
│  │   IMoveInput    │  │   IMoveInput    │  │   IShooterInput        │  │
│  └────────┬────────┘  └────────┬────────┘  └──────────┬─────────────┘  │
└───────────┼────────────────────┼──────────────────────┼───────────────┘
            │                    │                      │
┌───────────▼────────────────────▼──────────────────────▼───────────────┐
│                      REFERENCE RESOLVER (IoC per character)            │
└───────────────────────────────┬──────────────────────────────────────┘
                                │
┌───────────────────────────────▼──────────────────────────────────────┐
│                      CODE STATE MACHINE                               │
│  ┌──────────────────┐ ┌────────────────────┐ ┌────────────────────┐  │
│  │ MoveStateMachine  │ │FightingStateMachine │ │CollectStateMachine │  │
│  │ Features:         │ │ Features:            │ │ Features:          │  │
│  │  GroundCheck      │ │  Shoot, Aim, Rig     │ │  Collect           │  │
│  │  CheckSlope       │ │  Scope, Camera       │ │                    │  │
│  │  FootStep         │ │  WeaponSpawn, Sound  │ │                    │  │
│  │  Move/Sprint      │ │  Surface, Timeline   │ │                    │  │
│  │  Jump/Air         │ │                      │ │                    │  │
│  │  Crouch/Prone     │ │                      │ │                    │  │
│  └────────┬──────────┘ └──────────┬───────────┘ └────────┬───────────┘  │
└───────────┼──────────────────────┼───────────────────────┼─────────────┘
            │                      │                       │
┌───────────▼──────────────────────▼───────────────────────▼─────────────┐
│                      HYBRID ANIMATOR (PlayableGraph)                    │
│  Layer 0: Mecanim AnimatorControllerPlayable (base locomotion)         │
│  Layer 1: AnimationLayer (movement state machine)                      │
│  Layer 2: AnimationLayer (fighting state machine)                      │
│  Layer 3: AnimationLayer (collect state machine)                       │
└───────────────────────────────┬───────────────────────────────────────┘
                                │
┌───────────────────────────────▼───────────────────────────────────────┐
│                      WEAPON SYSTEM                                     │
│  ShootingWeapon → IDamageSender → DefaultRaycastBullet → HealthComp   │
│  Extensions: Muzzle, Sounds, ShellDispenser, Offset, ClipPlace        │
│  Inventory: Equip/Swap weapons, ammo management (BaseItemData SOs)    │
└───────────────────────────────┬───────────────────────────────────────┘
                                │
┌───────────────────────────────▼───────────────────────────────────────┐
│                      CAMERA + UI                                       │
│  CameraManager (Cinemachine vCam switching with axis transfer)         │
│  PlayerHUD (weapon display, scope overlay, ammo counter)               │
└───────────────────────────────────────────────────────────────────────┘
```