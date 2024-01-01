using System;
using ThirdPersonController.Core.DI;
using ThirdPersonController.Core;
using ThirdPersonController.Input;
using UnityEngine;
using UnityEngine.Serialization;

namespace ThirdPersonController.MovementStateMachine.Features
{
    [Serializable]
    public class FootStepFeature : BaseFeature
    { 
        private static readonly int LeftFootIkProperty = Animator.StringToHash("a_LeftFootIK");
        private static readonly int RightFootIkProperty = Animator.StringToHash("a_RightFootIK");
    
       
        [Header("Cast Height properties")]
        [SerializeField] private float startOffsetX;
        [SerializeField] private float raycastHeight = 0.16f;

        [Header("Cast Forward properties")]
        [FormerlySerializedAs("wfdCast2Offset")] public float startOffsetY = -0.1f;
        [SerializeField] private float stepLength;
        [SerializeField] private float minDiffToAdjustBodyPos =  0.15f;
    
#if UNITY_EDITOR
        [SerializeField] private bool visualiseRaycast;
#endif

        #region References
        private IMoveStateMachineVariables _variables;
        private Animator _animator;
        private IKPassRedirectorBehavior _ikPassRedirector;
        #endregion

        #region Temp Variables

        private IBaseInputReader _inputReader;
        
        private Transform _leftFoot;
        private Transform _rightFoot;
        
        private Transform _leftLeg;
        private Transform _rightLeg;

        private float _leftFeetBottomHeight;
        private float _rightFeetBottomHeight;
    
        #endregion

        public override void CacheReferences(IStateMachineVariables variables, IReferenceResolver resolver)
        {
            _variables = variables as IMoveStateMachineVariables;
            
            _inputReader = resolver.GetComponent<IBaseInputReader>();
            _animator = resolver.GetComponent<Animator>();
        
            _leftFoot = _animator.GetBoneTransform(HumanBodyBones.LeftFoot);
            _rightFoot = _animator.GetBoneTransform(HumanBodyBones.RightFoot);

            _leftLeg  = _animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
            _rightLeg = _animator.GetBoneTransform(HumanBodyBones.RightUpperLeg);
            
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
            float fps = 1.0f / Time.smoothDeltaTime;
            //make sure fps is greater than 2
            //on 2 fps system will not work fine
            if (_inputReader.moveInputMagnitude > 0 && fps < 3)
            {
                return;
            }
            
            float leftFootWeight = _animator.GetFloat(LeftFootIkProperty);
            float rightFootWeight = _animator.GetFloat(RightFootIkProperty);

            UpdateRays(_leftLegData,AvatarIKGoal.LeftFoot,_leftFoot,    _leftLeg);
            UpdateRays(_rightLegData,AvatarIKGoal.RightFoot,_rightFoot, _rightLeg);
            
            DoRaycasts(_leftLegData);
            DoRaycasts(_rightLegData);
            
            PlaceIK(_leftLegData, AvatarIKGoal.LeftFoot, in leftFootWeight, in _leftFeetBottomHeight, out var hit0);
            PlaceIK(_rightLegData, AvatarIKGoal.RightFoot, in rightFootWeight, in _rightFeetBottomHeight, out var hit1);
            
            //this code executes when fps is 15 or less
            if ((!hit0.collider ||!hit1.collider) && _inputReader.moveInputMagnitude > 0.5f && fps < 16)
            {
                if (!hit0.collider)
                    hit0 = _variables.GroundHit;
                if (!hit1.collider) 
                    hit1 = _variables.GroundHit;
            }
            
            if(!hit0.collider || !hit1.collider) return;
            
            float distance0 = hit0.collider ? hit0.point.y + _leftFeetBottomHeight - _leftFoot.position.y : 0;
            float distance1 = hit1.collider ? hit1.point.y + _rightFeetBottomHeight - _rightFoot.position.y : 0;

            float diffDistance = Mathf.Abs(distance0 * leftFootWeight + distance1 * rightFootWeight);
            if (diffDistance > minDiffToAdjustBodyPos)
                _animator.bodyPosition -= Vector3.up * diffDistance;
            // high slope surface support 
            else if (_variables.SlopeAngle > 15f) _animator.bodyPosition -= Vector3.up * diffDistance;
        }



        private LegData _leftLegData = new LegData();
        private LegData _rightLegData = new LegData();

        private class LegData
        {
            public Ray HeightCastRay;
            public Ray ForwardInitialRay;
            public Ray MidHeightCastRay;

            public RaycastHit HeightCastHit;
            public RaycastHit ForwardCastHit;
            public RaycastHit MidCastHit;
        }

        private void UpdateRays(LegData data,AvatarIKGoal goal,Transform footTransform,Transform legTransform)
        {
            var bodyUp = _animator.transform.rotation * Vector3.up;
            
            var footPosition = footTransform.position;
            var rotation = _animator.GetIKRotation(goal);
            var localUp = rotation * Vector3.up;
            var localForward = rotation * Vector3.forward;
            if (_variables.SlopeAngle > 15)
            {
                localForward = Quaternion.AngleAxis(_variables.SlopeAngle, Vector3.up) * Vector3.forward;
            }
            
            //height cast ray first
            var tempPosition = footPosition + localForward * startOffsetX;
            tempPosition.y = legTransform.position.y;
            data.HeightCastRay = new Ray(tempPosition, -bodyUp);
            
            //forward cast ray first
            data.ForwardInitialRay = 
                new Ray(footPosition + localForward * startOffsetX + localUp * startOffsetY, localForward);
            
            //middle height cast
            var midCastPoint = footPosition + localForward * stepLength / 2;
            midCastPoint.y = legTransform.position.y;
            data.MidHeightCastRay = new Ray(midCastPoint, -bodyUp);
        }

        private void DoRaycasts(LegData data)
        {
            bool wasHit = false;

            int groundLayer = _variables.GroundLayer;
            wasHit = Physics.Raycast(data.HeightCastRay, out data.HeightCastHit, raycastHeight, groundLayer,
                QueryTriggerInteraction.Ignore);

#if UNITY_EDITOR
            if (visualiseRaycast)
                Debug.DrawRay(data.HeightCastRay.origin, data.HeightCastRay.direction * raycastHeight, Color.white);
#endif

            wasHit = Physics.Raycast(data.ForwardInitialRay, out data.ForwardCastHit, stepLength, groundLayer,
                QueryTriggerInteraction.Ignore);

#if UNITY_EDITOR
            if (visualiseRaycast)
                Debug.DrawRay(data.ForwardInitialRay.origin, data.ForwardInitialRay.direction * stepLength,
                    wasHit ? Color.red : new Color(1f, 0.69f, 0.75f));
#endif

            wasHit = Physics.Raycast(data.MidHeightCastRay, out data.MidCastHit, raycastHeight, _variables.GroundLayer,
                QueryTriggerInteraction.Ignore);
#if UNITY_EDITOR
            if (visualiseRaycast)
                Debug.DrawRay(data.MidHeightCastRay.origin, data.MidHeightCastRay.direction * raycastHeight,
                    wasHit ? Color.green : new Color(0.71f, 1f, 0.66f));
#endif
        }

        /// <summary>
        /// this function choose footstep position from 4 variants
        /// </summary>
        private void PlaceIK(LegData legData, AvatarIKGoal goal,in float weight, in float footBottomHeight,out RaycastHit chosenHit) 
        {
           _animator.SetIKPositionWeight(goal,weight);
           _animator.SetIKRotationWeight(goal,weight);
           if (weight == 0)
           {
               chosenHit = default;
               return;
           }

           // Get the local up direction of the foot.
           var rotation = _animator.GetIKRotation(goal);
           var localUp = rotation * Vector3.up;
           var localForward = rotation * Vector3.forward;
           if (_variables.SlopeAngle > 15)
           {
               localForward = Quaternion.AngleAxis(_variables.SlopeAngle, Vector3.up) * Vector3.forward;
           }
           
           if (!legData.HeightCastHit.collider)
           {
               chosenHit = default;
               return;
           }

           RaycastHit forwardInitialHit = legData.ForwardCastHit;
           RaycastHit heightHitInitial = legData.HeightCastHit;
  
           Vector3 goalPosition;
           if (_variables.SlopeAngle < 15)
           {
               float yOffset = forwardInitialHit.point.y - _variables.GroundHit.point.y;
               var newYHeight = heightHitInitial.point.y + yOffset;
               
               //if height cast and forward cast start points match
               if (forwardInitialHit.collider && Math.Abs(legData.ForwardInitialRay.origin.y - newYHeight) < 0.0001)
               {
                   goalPosition = legData.ForwardInitialRay.origin - localForward * OffsetFwd(forwardInitialHit.distance);
                   goalPosition.y = newYHeight;
                   chosenHit = forwardInitialHit;
               }
               else
               { 
                   ForwardCastFromHitPoint(out var forwardCastFromSecondHit, heightHitInitial.point,in localUp, in localForward);

                   if (forwardCastFromSecondHit.collider)
                   {
                       float forwardOffset = OffsetFwd(forwardCastFromSecondHit.distance);
                       goalPosition = heightHitInitial.point - localForward * (startOffsetX + forwardOffset) + localUp * footBottomHeight;
                       chosenHit = forwardCastFromSecondHit;
                   }
                   else
                   {
                       float diff = Mathf.Abs(heightHitInitial.point.y - legData.MidCastHit.point.y);
                       if (legData.MidCastHit.collider && diff > 0.02f)
                       {
                           goalPosition = legData.MidCastHit.point + localUp * footBottomHeight + localForward * stepLength / 2;
                           chosenHit = legData.MidCastHit;
                       }
                       else
                       {
                           goalPosition = heightHitInitial.point - localForward * startOffsetX + localUp * footBottomHeight;
                           chosenHit = heightHitInitial;
                       }
                   }
               }
           }
           // for high slope simplified algorithm used
           else
           {
               float forwardOffset = forwardInitialHit.collider ? OffsetFwd(forwardInitialHit.distance) : 0;
               goalPosition = heightHitInitial.point - localForward * (startOffsetX + forwardOffset) + localUp * footBottomHeight;
               chosenHit = heightHitInitial;
           }
           
           _animator.SetIKPosition(goal,goalPosition);
           
           // Use the hit normal to calculate the desired rotation.
           var rotAxis = Vector3.Cross(localUp, heightHitInitial.normal);
           var angle = Vector3.Angle(localUp, heightHitInitial.normal);
           rotation = Quaternion.AngleAxis(angle, rotAxis) * rotation;

           _animator.SetIKRotation(goal, rotation);
       }

        private void ForwardCastFromHitPoint(out RaycastHit forwardCastFromSecondHit, 
            in Vector3 origin, in Vector3 localUp, in Vector3 localForward)
        {
            var ray = new Ray(origin + localUp * -startOffsetY, localForward);
            bool raycast = Physics.Raycast(ray, out forwardCastFromSecondHit,stepLength,
                _variables.GroundLayer, QueryTriggerInteraction.Ignore);
                       
#if UNITY_EDITOR
            if(visualiseRaycast)
                Debug.DrawRay(ray.origin,ray.direction*stepLength, raycast? new Color(1f, 0.42f, 0f) : Color.yellow);
#endif
        }

        private float OffsetFwd(float dis)
        {
            var result = stepLength - dis;
            if (result < 0) result = stepLength + result;
            return result;
        }

        public Vector3 BlendPositionXZByDirection(Vector3 position0, Vector3 position1, Vector3 axis)
        {
            position0.x = Mathf.Lerp(position0.x, position1.x, Mathf.Abs(axis.x));
            position0.z = Mathf.Lerp(position0.z, position1.z, Mathf.Abs(axis.z));
            return position0;
        }

    
        public override void OnExitState()
        {
            _ikPassRedirector.OnStateIKEvent -= OnStateIK;
        }

#if UNITY_EDITOR
        
        public override int GetHashCode()
        {
            return
                $"{path} {_animator?.GetInstanceID()} {_ikPassRedirector?.GetInstanceID()} {_leftFoot?.GetInstanceID()} {_rightFoot?.GetInstanceID()}"
                    .GetHashCode();
        }
#endif
    }
}
