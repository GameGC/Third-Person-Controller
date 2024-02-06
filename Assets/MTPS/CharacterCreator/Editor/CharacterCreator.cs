#if  UNITY_EDITOR

#if ENABLE_CODE_MOVEMENTSTATEMACHINE
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ThirdPersonController.Core.DI;
using ThirdPersonController.Core.StateMachine;
using ThirdPersonController.Input;
using ThirdPersonController.MovementStateMachine.Code;
using UnityEditor;
using UnityEditor.PackageManager.UI;
using UnityEditor.Presets;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Animations.Rigging;
using Object = UnityEngine.Object;

namespace ThirdPersonController.CharacterCreator.Editor
{
    public class CharacterCreator : EditorWindow
    {
        private GameObject copyFrom;
        private Animator charAnimator;
        public  RuntimeAnimatorController controller;
        public GameObject hud;
        private Vector2 rect = new Vector2(500, 540);
        private Vector2 scrool;
        private UnityEditor.Editor humanoidpreview;
        
        private Dictionary<string,Preset> stateMachinesPresets = new Dictionary<string, Preset>()
        {
            { "Movement State Machine", null },
        };
        
        
        [MenuItem("Tools/CharacterCreator", false, 0)]
        public static void CreateNewCharacter()
        {
            var size = new Vector2(500, 540);
            var window = GetWindow<CharacterCreator>("CharacterCreator", true);
            window.maxSize = new Vector2(500,window.maxSize.y);
            window.minSize = new Vector2(500,window.minSize.y);

        }

        private bool isHuman, isValidAvatar, charExist;

        private void OnEnable()
        {
            charAnimator = Selection.activeGameObject?.GetComponent<Animator>();
            charExist = charAnimator != null;

            if (charExist)
            {
                humanoidpreview = UnityEditor.Editor.CreateEditor(charAnimator.gameObject);
                 
                isHuman = charAnimator.isHuman;
                isValidAvatar = charAnimator.avatar.isValid;
            }
        }

        private void OnGUI()
        {
            GUILayout.Box("Character Creator Window");
            GUILayout.Space(5);

            GUILayout.BeginVertical("box");

            if (!charAnimator)
                EditorGUILayout.HelpBox("Make sure your model is set as Humanoid!", MessageType.Info);
            else if (!charExist)
                EditorGUILayout.HelpBox("Missing a Animator Component", MessageType.Error);
            else if (!isHuman)
                EditorGUILayout.HelpBox("This is not a Humanoid", MessageType.Error);
            else if (!isValidAvatar)
                EditorGUILayout.HelpBox(charAnimator.name + " is a invalid Humanoid", MessageType.Info);

            charAnimator = EditorGUILayout.ObjectField("Model", charAnimator, typeof(Animator), true, GUILayout.ExpandWidth(true)) as Animator;

            copyFrom = EditorGUILayout.ObjectField("Copy params From", copyFrom, typeof(GameObject), true, GUILayout.ExpandWidth(true)) as GameObject;
           
            
            if (GUI.changed && charAnimator != null && charAnimator.GetComponent<MoveStateMachine>() == null)
                humanoidpreview = UnityEditor.Editor.CreateEditor(charAnimator.gameObject);
            if (charAnimator != null && charAnimator.GetComponent<MoveStateMachine>() != null)
                EditorGUILayout.HelpBox("This gameObject already contains the component ThirdPersonMoveStateMachine", MessageType.Warning);

            controller = EditorGUILayout.ObjectField("Animator Controller: ", controller, typeof(RuntimeAnimatorController), false) as RuntimeAnimatorController;            
            
            
            GUILayout.Label("State Machines Presets:");
            foreach (var key in stateMachinesPresets.Keys.ToArray())
            {
                stateMachinesPresets[key] = EditorGUILayout.ObjectField(key, stateMachinesPresets[key], typeof(Preset), false) as Preset;
            }
           
            
            GUILayout.EndVertical();          

          
            charExist = charAnimator != null;
            isHuman = charExist ? charAnimator.isHuman : false;
            isValidAvatar = charExist ? charAnimator.avatar.isValid : false;

            if (CanCreate())
            {
                DrawHumanoidPreview();
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                
                if(copyFrom)
                    if (GUILayout.Button("Copy Bones names and Export Model"))
                        CopyRig();
                
                if (controller != null)
                {
                    if (GUILayout.Button("Create"))
                        Create();
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }

        }

        private bool CanCreate()
        {
            return isValidAvatar && isHuman && charAnimator != null && charAnimator.GetComponent<MoveStateMachine>() == null;
        }

        /// <summary>
        /// Draw the Preview window
        /// </summary>
        private void DrawHumanoidPreview()
        {
            GUILayout.Width(18);

            if (humanoidpreview != null)
            {
                humanoidpreview.OnInteractivePreviewGUI(GUILayoutUtility.GetRect(1, 350,GUILayout.ExpandWidth(true)), "window");
            }
        }

        public void CopyRig()
        {
            var oldAnimator = copyFrom.GetComponent<Animator>();
            
            var controllerGameObject = Instantiate(charAnimator, Vector3.zero, Quaternion.identity).gameObject;
            var controllerAnimator = controllerGameObject.GetComponent<Animator>();
            
            var BoneValues = Enum.GetValues(typeof(HumanBodyBones)).Cast<HumanBodyBones>();
            foreach (var value in BoneValues)
            {
                if (value == HumanBodyBones.LastBone) continue;

                var oldTransform = oldAnimator.GetBoneTransform(value);
                var newTransform = controllerAnimator.GetBoneTransform(value);
                
                if(oldTransform == null) continue;
                if (newTransform) newTransform.name = oldTransform.name;
            }
            
            var toExport = controllerGameObject;
            var windowType = Type.GetType("UnityEditor.Formats.Fbx.Exporter.ExportModelEditorWindow, Unity.Formats.Fbx.Editor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
            if (windowType == null)
            {
                UnityEditor.PackageManager.UI.Window.Open("com.unity.formats.fbx");
                DestroyImmediate(controllerGameObject);
                throw new NotImplementedException("Install FBX Exporter Plugin");
            }
            var args = new object[] {new Object[]{controllerGameObject}, toExport.name,false};
            windowType.GetMethod("Init", BindingFlags.Static | BindingFlags.Public).Invoke(null, args);
        }
        /// <summary>
        /// Created the Third Person Controller
        /// </summary>
        private void Create()
        {
            bool couldCopy = copyFrom;
            
            // base for the character
            var controllerGameObject = Instantiate(charAnimator, Vector3.zero, Quaternion.identity).gameObject;
            var controllerAnimator = controllerGameObject.GetComponent<Animator>();
            
            if (!controllerGameObject)
                return;          
            controllerGameObject.name = "CharacterController_" + charAnimator.gameObject.name;
            controllerGameObject.tag = "Player";
            controllerGameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
            
            //reference resolver 

            SerializedObject resolver;
            if (couldCopy)
            {
                ComponentUtility.CopyComponent(copyFrom.GetComponent<ReferenceResolver>());
                ComponentUtility.PasteComponentAsNew(controllerGameObject);
                controllerGameObject.GetComponent<ReferenceResolver>().Editor_Remap();
                resolver = new SerializedObject(controllerGameObject.GetComponent<ReferenceResolver>());
            }
            else
            {
                resolver = new SerializedObject(controllerGameObject.AddComponent<ReferenceResolver>());

            }

            //rigidbody
            
            var rigidbody = controllerGameObject.AddComponent<Rigidbody>();
            rigidbody.useGravity = true;
            rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
            rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
            rigidbody.mass = couldCopy ? copyFrom.GetComponent<Rigidbody>().mass :50;
            
            // capsule collider 
            
            var collider = controllerGameObject.AddComponent<CapsuleCollider>();
            collider.height = ColliderHeight(controllerAnimator);
            collider.center = new Vector3(0, (float)System.Math.Round(collider.height * 0.5f, 2), 0);
            collider.radius = (float)System.Math.Round(collider.height * 0.15f, 2);
            
            //camera root 
            
            var cameraRoot = new GameObject("PlayerCameraRoot");
            var headBone = controllerAnimator.GetBoneTransform(HumanBodyBones.Head);
            cameraRoot.transform.position = headBone.position;
            cameraRoot.transform.rotation = headBone.rotation;
            cameraRoot.transform.SetParent(controllerGameObject.transform);
            
            var postionConstatint = cameraRoot.AddComponent<PositionConstraint>();
            
            postionConstatint.weight = 1;
            postionConstatint.AddSource(new ConstraintSource()
            {
                sourceTransform = headBone,
                weight = 1
            });
            postionConstatint.locked = true;
            postionConstatint.constraintActive = true;
            postionConstatint.enabled = false;
            
            
            //state machines

            var stateMachines = new GameObject("StateMachines");
            stateMachines.transform.SetParent(controllerGameObject.transform);

            //movement state machine

            if (couldCopy)
            {
                var copyParent = copyFrom.GetComponentInChildren<CodeStateMachine>().transform.parent;
                foreach (Transform child in copyParent)
                {
                    GameObject prefab = PrefabUtility.GetCorrespondingObjectFromSource(child.gameObject);
                    if (prefab != null)
                        PrefabUtility.InstantiatePrefab(prefab, stateMachines.transform);
                    else
                        Instantiate(child.gameObject,stateMachines.transform);
                }
            }
            else
            {
                var movementStateMachine = new GameObject("Movement");
                var tpsStateMachine = new SerializedObject(movementStateMachine.AddComponent<MoveStateMachine>());

                stateMachinesPresets["Movement State Machine"].ApplyTo(tpsStateMachine.targetObject);
            
                tpsStateMachine.FindProperty("ReferenceResolver").objectReferenceValue = resolver.targetObject;
                tpsStateMachine.ApplyModifiedPropertiesWithoutUndo();
            
                movementStateMachine.transform.SetParent(stateMachines.transform);
            }
           
            if (controller)
            {
                controllerAnimator.runtimeAnimatorController = controller;
                controllerAnimator.applyRootMotion = false;
            }

            //copying target for animation rig
            if (couldCopy)
            {
                var oldAnimator = copyFrom.GetComponent<Animator>();
                var exceptionArray = copyFrom.GetComponentsInChildren<SkinnedMeshRenderer>().SelectMany(s => s.bones).ToArray();
                
                var BoneValues = Enum.GetValues(typeof(HumanBodyBones)).Cast<HumanBodyBones>();
                foreach (var value in BoneValues)
                {
                    if (value == HumanBodyBones.LastBone) continue;

                    var oldTransform = oldAnimator.GetBoneTransform(value);
                    var newTransform = controllerAnimator.GetBoneTransform(value);
                        
                    if(oldTransform == null) continue;
                    //if (newTransform) newTransform.name = oldTransform.name;
                    
                    foreach (Transform child in oldTransform)
                    {
                        if(Array.IndexOf(exceptionArray,child) > -1) continue;
                        var newInstance = Instantiate(child, newTransform);
                        child.GetLocalPositionAndRotation(out var pos, out var rot);
                        newInstance.SetLocalPositionAndRotation(pos,rot);
                        newInstance.name = child.name;
                    }
                }
            }

            if (couldCopy)
            {
                var rb = copyFrom.GetComponent<RigBuilder>();
                if (rb)
                {
                    var layer = rb.layers.FirstOrDefault(l => l.rig);
                    Transform altParent = copyFrom.transform.Cast<Transform>().FirstOrDefault(child => child.name.Contains("rig", StringComparison.OrdinalIgnoreCase));
                    if (layer.rig || altParent)
                    {
                        var copyRigParent = Instantiate(layer.rig?layer.rig.transform.parent:altParent, controllerGameObject.transform);
                        copyRigParent.name = (layer.rig? layer.rig.transform.parent : altParent).name;
                        var newRB = controllerGameObject.AddComponent(rb.GetType()) as RigBuilder;
                        newRB.layers = new List<RigLayer>(new RigLayer[rb.layers.Count]);
                        for (var i = 0; i < rb.layers.Count; i++)
                        {
                            if (rb.layers[i].rig)
                            {
                                newRB.layers[i] = 
                                    new RigLayer(controllerGameObject.transform
                                    .Find(AnimationUtility.CalculateTransformPath(rb.layers[i].rig.transform,
                                        copyFrom.transform))
                                    .GetComponent<Rig>());
                            }
                        }

                    }
                }
            }
            
            
            
            Selection.activeGameObject = controllerGameObject;
            UnityEditor.SceneView.lastActiveSceneView.FrameSelected();
            this.Close();
        }

        /// <summary>
        /// Capsule Collider height based on the Character height
        /// </summary>
        /// <param name="animator">animator humanoid</param>
        /// <returns></returns>
        private float ColliderHeight(Animator animator)
        {
            var foot = animator.GetBoneTransform(HumanBodyBones.LeftFoot);
            var hips = animator.GetBoneTransform(HumanBodyBones.Hips);
            return (float)System.Math.Round(Vector3.Distance(foot.position, hips.position) * 2f, 2);
        }       

    }
}
#endif

#endif
