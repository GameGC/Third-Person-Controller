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
    
#if UNITY_EDITOR
        [SerializeField] private bool visualiseRaycast;
#endif

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

            UpdateRays(leftLegData,_LeftFoot, _animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg));
            UpdateRays(rightLegData,_RightFoot, _animator.GetBoneTransform(HumanBodyBones.RightUpperLeg));
            
            DoRaycasts(leftLegData);
            DoRaycasts(rightLegData);
            
            PlaceIK(leftLegData, AvatarIKGoal.LeftFoot, in leftFootWeight, in _leftFeetBottomHeight, out var hit0);
            PlaceIK(rightLegData, AvatarIKGoal.RightFoot, in rightFootWeight, in _rightFeetBottomHeight, out var hit1);
            
            float distance0 = hit0.collider ? hit0.point.y + _leftFeetBottomHeight - _LeftFoot.position.y : 0;
            float distance1 = hit1.collider ? hit1.point.y + _rightFeetBottomHeight - _RightFoot.position.y : 0;

            float diffDistance = Mathf.Abs(distance0 * leftFootWeight + distance1 * rightFootWeight);
            if (diffDistance > 0.08f)
                _animator.bodyPosition -= Vector3.up * diffDistance;
            // high slope surface support 
            else if (_variables.SlopeAngle > 15f) _animator.bodyPosition -= Vector3.up * diffDistance;
        }

        public float wfdCast2Offset = -0.1f;


        private LegData leftLegData = new LegData();
        private LegData rightLegData = new LegData();

        private class LegData
        {
            public Ray heightCastRay;
            public Ray forwardInitialRay;
            public Ray midHeightCastRay;

            public RaycastHit heightCastHit;
            public RaycastHit forwardCastHit;
            public RaycastHit midCastHit;
        }

        private void UpdateRays(LegData data,Transform footTransform,Transform legTransform)
        {
            var bodyUp = _animator.transform.rotation * Vector3.up;
            
            footTransform.GetPositionAndRotation(out var footPosition, out var rotation);
            var localUp = rotation * Vector3.up;
            var localForward = rotation * Vector3.forward;
            if (_variables.SlopeAngle > 15)
            {
                localForward = Quaternion.AngleAxis(_variables.SlopeAngle, Vector3.up) * Vector3.forward;
            }
            
            //height cast ray first
            var tempPosition = footPosition + localForward * startOffsetX;
            tempPosition.y = legTransform.position.y;
            data.heightCastRay = new Ray(tempPosition, -bodyUp);
            
            //forward cast ray first
            data.forwardInitialRay = 
                new Ray(footPosition + localForward * startOffsetX + localUp * wfdCast2Offset, localForward);
            
            //middle height cast
            var midCastPoint = footPosition + localForward * stepLength / 2;
            midCastPoint.y = legTransform.position.y;
            data.midHeightCastRay = new Ray(midCastPoint, -bodyUp);
        }

        private void DoRaycasts(LegData data)
        {
            var distance = RaycastOriginY - RaycastEndY;
            bool wasHit = false;

            int groundLayer = _variables.GroundLayer;
            wasHit = Physics.Raycast(data.heightCastRay, out data.heightCastHit, distance, groundLayer,
                QueryTriggerInteraction.Ignore);

#if UNITY_EDITOR
            if (visualiseRaycast)
                Debug.DrawRay(data.heightCastRay.origin, data.heightCastRay.direction * distance, Color.white);
#endif

            wasHit = Physics.Raycast(data.forwardInitialRay, out data.forwardCastHit, stepLength, groundLayer,
                QueryTriggerInteraction.Ignore);

#if UNITY_EDITOR
            if (visualiseRaycast)
                Debug.DrawRay(data.forwardInitialRay.origin, data.forwardInitialRay.direction * stepLength,
                    wasHit ? Color.red : new Color(1f, 0.69f, 0.75f));
#endif
            
            wasHit = (Physics.Raycast(data.midHeightCastRay, out data.midCastHit, distance, 
                _variables.GroundLayer, QueryTriggerInteraction.Ignore));
#if UNITY_EDITOR
            if(visualiseRaycast)
                Debug.DrawRay(data.midHeightCastRay.origin,data.midHeightCastRay.direction*distance,wasHit?Color.green :
                    new Color(0.71f, 1f, 0.66f));
#endif
        }

        /// <summary>
        /// this function choose footstep position from 4 variants
        /// </summary>
        private void PlaceIK(LegData legData, AvatarIKGoal goal,in float weight, in float footBottomHeight,out RaycastHit chosenHit) 
        {
           _animator.SetIKPositionWeight(goal,0);
           _animator.SetIKRotationWeight(goal,0);
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
           
           if (!legData.heightCastHit.collider)
           {
               chosenHit = default;
               return;
           }

           RaycastHit forwardInitialHit = legData.forwardCastHit;
           RaycastHit heightHitInitial = legData.heightCastHit;
  
           Vector3 goalPosition;
           if (_variables.SlopeAngle < 15)
           {
               float yOffset = forwardInitialHit.point.y - _variables.GroundHit.point.y;
               var newYHeight = heightHitInitial.point.y + yOffset;
               
               //if height cast and forward cast start points match
               if (forwardInitialHit.collider && Math.Abs(legData.forwardInitialRay.origin.y - newYHeight) < 0.0001)
               {
                   goalPosition = legData.forwardInitialRay.origin - localForward * OffsetFwd(forwardInitialHit.distance);
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
                       float diff = Mathf.Abs(heightHitInitial.point.y - legData.midCastHit.point.y);
                       if (legData.midCastHit.collider && diff > 0.02f)
                       {
                           goalPosition = legData.midCastHit.point + localUp * footBottomHeight + localForward * stepLength / 2;
                           chosenHit = legData.midCastHit;
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
           
           _animator.SetIKPositionWeight(goal,1);
           _animator.SetIKPosition(goal,goalPosition);
           
           // Use the hit normal to calculate the desired rotation.
           var rotAxis = Vector3.Cross(localUp, heightHitInitial.normal);
           var angle = Vector3.Angle(localUp, heightHitInitial.normal);
           rotation = Quaternion.AngleAxis(angle, rotAxis) * rotation;

           _animator.SetIKRotationWeight(goal,1);
           _animator.SetIKRotation(goal, rotation);
       }

        private void ForwardCastFromHitPoint(out RaycastHit forwardCastFromSecondHit, 
            in Vector3 origin, in Vector3 localUp, in Vector3 localForward)
        {
            var ray = new Ray(origin + localUp * -wfdCast2Offset, localForward);
            bool raycast = (Physics.Raycast(ray, out forwardCastFromSecondHit,stepLength,
                _variables.GroundLayer, QueryTriggerInteraction.Ignore));
                       
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
                $"{path} {_animator?.GetInstanceID()} {_ikPassRedirector?.GetInstanceID()} {_LeftFoot?.GetInstanceID()} {_RightFoot?.GetInstanceID()}"
                    .GetHashCode();
        }
#endif
    }
}