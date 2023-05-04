using System.Collections;
using ThirdPersonController.Core;
using ThirdPersonController.Core.StateMachine;
using ThirdPersonController.Input;
using UnityEngine;

namespace ThirdPersonController.MovementStateMachine.Code
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

            //fix of circular dependency
            if (Variables is IMoveStateMachineVariables moveStateMachineVariables)
            {
                var inputReader = ReferenceResolver.GetComponent<IBaseInputReader>();
                inputReader.movementSmooth = moveStateMachineVariables.MovementSmooth;
            }
            
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
                
                //fix of circular dependency
                if (Variables is IMoveStateMachineVariables moveStateMachineVariables)
                {
                    var inputReader = ReferenceResolver.GetComponent<IBaseInputReader>();
                    inputReader.movementSmooth = moveStateMachineVariables.MovementSmooth;
                }
            
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
    }
}