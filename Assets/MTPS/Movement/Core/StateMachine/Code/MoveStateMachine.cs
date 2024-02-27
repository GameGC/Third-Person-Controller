using System.Collections;
using GameGC.CommonEditorUtils.Editor;
using MTPS.Core;
using MTPS.Core.Attributes;
using MTPS.Core.CodeStateMachine;
using MTPS.Movement.Core.StateMachine;
using UnityEditor;
using UnityEngine;

namespace MTPS.Movement.Core.StateMachine.Editor
{
    [RequireComponent(typeof(MovementStateMachineVariables))]
    public class MoveStateMachine : CodeStateMachine
    {
        [SerializeReference, SerializeReferenceAddButton(typeof(BaseFeature))]
        public BaseFeature[] alwaysExecutedFeatures = new BaseFeature[0];//= new BaseFeature[]{ new GroundCheckFeature(), new CheckSlopeFeature(), };
        
        protected override void Awake()
        {
            base.Awake();

            if (startWhenResolverIsReady)
                return;

            foreach (var feature in alwaysExecutedFeatures)
            {
                feature.CacheReferences(Variables,ReferenceResolver);
            }
        }

        protected override IEnumerator Start()
        {
            StartCoroutine(base.Start());
            
            if (startWhenResolverIsReady)
            {
                yield return new WaitUntil(() => ReferenceResolver.isReady);
                
                foreach (var feature in alwaysExecutedFeatures)
                {
                    feature.CacheReferences(Variables,ReferenceResolver);
                }
            }
        }

        public void Recache()
        {
            Awake();
            StartCoroutine(Start());
        }

        protected override void Update()
        {
            if(!isStarted) return;
            base.Update();
            foreach (var feature in alwaysExecutedFeatures)
            {
                feature.OnUpdateState();
            }
        }

        protected override void FixedUpdate()
        {
            if(!isStarted) return;
            base.FixedUpdate();
            foreach (var feature in alwaysExecutedFeatures)
            {
                feature.OnFixedUpdateState();
            }
        }


        [ContextMenu("Fix Missing")]
        public void Fix()
        {
            var missingTypes = SerializationUtility.GetManagedReferencesWithMissingTypes(this);
            var allTypes = AllTypesContainer.AllTypes.FindAll(t=>t.IsClass && t.IsSubclassOf(typeof(BaseFeature)));
            foreach (var missing in missingTypes)
            {
                int index = allTypes.FindIndex(t => t.Name == missing.className);
                if (index > -1)
                {
                    Debug.Log(missing.serializedData);
                    var newType = JsonUtility.FromJson(missing.serializedData, allTypes[index]);
                        
                    SerializationUtility.SetManagedReferenceIdForObject(this, newType, missing.referenceId);
                }
            }
        }
    }
}