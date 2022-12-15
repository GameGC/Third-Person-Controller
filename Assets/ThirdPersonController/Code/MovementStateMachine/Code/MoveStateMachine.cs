using ThirdPersonController.Core;
using ThirdPersonController.Core.StateMachine;
using ThirdPersonController.Input;
using UnityEngine;

namespace ThirdPersonController.MovementStateMachine.Code
{
    [RequireComponent(typeof(MovementStateMachineVariables))]
    public class MoveStateMachine : CodeStateMachine
    {
        [SerializeReference, NonReorderable, SerializeReferenceAddButton(typeof(BaseFeature))]
        public BaseFeature[] alwaysExecutedFeatures = new BaseFeature[0];//= new BaseFeature[]{ new GroundCheckFeature(), new CheckSlopeFeature(), };
        
        protected override void Awake()
        {
            base.Awake();

            //fix of circular dependency
            if (Variables is IMoveStateMachineVariables moveStateMachineVariables)
            {
                var inputReader = ReferenceResolver.GetComponent<BaseInputReader>();
                inputReader.movementSmooth = moveStateMachineVariables.MovementSmooth;
            }
            
            foreach (var feature in alwaysExecutedFeatures)
            {
                feature.CacheReferences(Variables,ReferenceResolver);
            }
        }

        protected override void Update()
        {
            base.Update();
            foreach (var feature in alwaysExecutedFeatures)
            {
                feature.OnUpdateState();
            }
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();
            foreach (var feature in alwaysExecutedFeatures)
            {
                feature.OnFixedUpdateState();
            }
        }
    }
}