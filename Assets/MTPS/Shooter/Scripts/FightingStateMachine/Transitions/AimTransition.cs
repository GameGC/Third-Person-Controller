using MTPS.Core;
using ThirdPersonController.Input;
using UnityEngine;

namespace Fighting.Pushing
{
    public class AimTransition : CouldAimNoIntersectTransition
    {
        [SerializeField] private bool isAim;

        private IShooterInput _input;
        public override void Initialise(IStateMachineVariables variables, IReferenceResolver resolver)
        {
            _input = resolver.GetComponent<IShooterInput>();
            base.Initialise(variables,resolver);
        }

        public override bool couldHaveTransition
        {
            get
            {
                if (isAim)
                    return _input.IsAim && base.couldHaveTransition;

                return !_input.IsAim || !base.couldHaveTransition;
            }
        }
    }
}