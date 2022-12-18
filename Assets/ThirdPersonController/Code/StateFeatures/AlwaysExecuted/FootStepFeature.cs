using System;
using ThirdPersonController.Core.DI;
using ThirdPersonController.Core;
using UnityEngine;

namespace ThirdPersonController.MovementStateMachine.Features
{
    [Serializable]
    public class FootStepFeature : BaseFeature
    { 
        private static readonly int LeftFootIkProperty = Animator.StringToHash("a_LeftFootIK");
        private static readonly int RightFootIkProperty = Animator.StringToHash("a_RightFootIK");
    
       
        [Header("Cast Height properties")]
        [SerializeField] private float startOffsetX;
        [SerializeField] private float RaycastOriginY = 0.5f;
        [SerializeField] private float RaycastEndY = -0.2f;
        
        [Header("Cast Forward properties")]
        [SerializeField] private float startOffsetY;
        [SerializeField] private float stepLength;
        [SerializeField] private float smoothTime = 300;
    
    

        #region References
        private IMoveStateMachineVariables _variables;
        private Animator _animator;
        private IKPassRedirectorBehavior _ikPassRedirector;
        #endregion

        #region Temp Variables
        private Transform _LeftFoot;
        private Transform _RightFoot;

        private float _leftFeetBottomHeight;
        private float _rightFeetBottomHeight;
    
        #endregion
    

        public override void CacheReferences(IStateMachineVariables variables, IReferenceResolver resolver)
        {
            _variables = variables as IMoveStateMachineVariables;

            _animator = resolver.GetComponent<Animator>();
        
            _LeftFoot = _animator.GetBoneTransform(HumanBodyBones.LeftFoot);
            _RightFoot = _animator.GetBoneTransform(HumanBodyBones.RightFoot);

            _leftFeetBottomHeight = _animator.leftFeetBottomHeight;
            _rightFeetBottomHeight = _animator.rightFeetBottomHeight;
        }

        public override void OnEnterState()
        {
            _ikPassRedirector = _animator.GetBehaviour<IKPassRedirectorBehavior>();
            _ikPassRedirector.OnStateIKEvent += OnStateIK;
        }

  
        private void OnStateIK()
        {
            float leftFootWeight = _animator.GetFloat(LeftFootIkProperty);
            float rightFootWeight = _animator.GetFloat(RightFootIkProperty);

            HeightCast(_LeftFoot, AvatarIKGoal.LeftFoot,in rightFootWeight,in _rightFeetBottomHeight,out var hit0);
            HeightCast(_RightFoot, AvatarIKGoal.RightFoot,in rightFootWeight,in _rightFeetBottomHeight,out var hit1);
        
        
            float distance0 = hit0.collider ? hit0.point.y + _leftFeetBottomHeight - _LeftFoot.position.y:0;
            float distance1 = hit1.collider ? hit1.point.y + _rightFeetBottomHeight - _RightFoot.position.y:0;

            float diffDistance = Mathf.Abs(distance0 * leftFootWeight + distance1 * rightFootWeight);

            if(diffDistance>0.16f)
                _animator.bodyPosition -=Vector3.up * diffDistance;
            // high slope surface support 
            else if(_variables.SlopeAngle>15f) 
                _animator.bodyPosition -= Vector3.up * diffDistance;
        }


        private void HeightCast(Transform footTransform, AvatarIKGoal goal,in float weight, in float footBottomHeight, out RaycastHit hit)
        {
            _animator.SetIKPositionWeight(goal, weight);
            _animator.SetIKRotationWeight(goal, weight);

            if (weight == 0)
            {
                hit = default;
                return;
            }

            // Get the local up direction of the foot.
            var rotation = _animator.GetIKRotation(goal);
            var localUp = rotation * Vector3.up;
            var localForward = rotation * Vector3.forward;
        
            // high slope surface support
            if (_variables.SlopeAngle>15)
            {
                localForward = Quaternion.AngleAxis(_variables.SlopeAngle, Vector3.up) * Vector3.forward;
            }


            var position = footTransform.position;
            var distance = RaycastOriginY - RaycastEndY;

        
            position += localUp * RaycastOriginY;
            position += localForward * startOffsetX;


            Debug.DrawLine(position,position-localUp*distance,Color.white);
            if (!Physics.Raycast(position, -localUp, out hit, distance,_variables.GroundLayer,QueryTriggerInteraction.Ignore))
            {
                position += localUp * _variables.GroundDistance;
                if (!Physics.Raycast(position, -localUp, out hit, distance,_variables.GroundLayer,QueryTriggerInteraction.Ignore))
                    return;
            }
            
            position = hit.point - localForward * startOffsetX + localUp * footBottomHeight;
            
            // Use the hit normal to calculate the desired rotation.
            var rotAxis = Vector3.Cross(localUp, hit.normal);
            var angle = Vector3.Angle(localUp, hit.normal);
            rotation = Quaternion.AngleAxis(angle, rotAxis) * rotation;

            _animator.SetIKRotation(goal, rotation);
            
            
            
            
            ForwardCast( hit.point + localUp * 0.01f, localForward,ref position);
            _animator.SetIKPosition(goal,Vector3.Lerp(_animator.GetIKPosition(goal), position,Time.deltaTime *smoothTime));
        }


        private void ForwardCast(Vector3 position,Vector3 forwardDirection,ref Vector3 finalPosition)
        {
            Debug.DrawLine(position,position+forwardDirection*stepLength,Color.yellow);

            if (Physics.Raycast(position, forwardDirection, out var hit, stepLength,_variables.GroundLayer,QueryTriggerInteraction.Ignore))
            {
                var direction = hit.point-forwardDirection*stepLength;
                finalPosition.x = direction.x;
                finalPosition.z = direction.z;
            }
        }
        

    
        public override void OnExitState()
        {
            _ikPassRedirector.OnStateIKEvent -= OnStateIK;
        }

        public override int GetHashCode()
        {
            return
                $"{path} {_animator?.GetInstanceID()} {_ikPassRedirector?.GetInstanceID()} {_LeftFoot?.GetInstanceID()} {_RightFoot?.GetInstanceID()}"
                    .GetHashCode();
        }
    }
}
