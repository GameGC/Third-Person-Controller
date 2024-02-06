using System;
using System.Collections;
using System.Collections.Generic;
using ThirdPersonController.Core;
using ThirdPersonController.Core.DI;
using ThirdPersonController.Input;
using UnityEngine;

public class NewFootFeature : BaseFeature
{
    public enum CastType
    {
        Ray,
        Sphere,
        RayAndSphere
    }

    private const float MAX_DISTANCE_POWER = 2;
    private const float MAX_SMOOTHING_ANGLE = 3;
    private const float SMOOTHING_POWER = 1f;
    private const float MAX_STEP_HEIGHT = 2;
    private const float HEIGHT_OFFSET = 0.5f;

    private Animator m_Anim;
    private Transform transform;
    private Limb m_LeftLeg;
    private Limb m_RightLeg;
    private bool m_IsInitialized = false;
    private float m_MeshHeightOffset;

    [Header("Main setting")] public LayerMask ignoreLayers;

    /// <summary>
    /// Setting "true" will enable addition raycast from toes' bones for better foot positioning. Toes' bones are required for proper work. 
    /// </summary>
    [Space(5)] public bool increasedAccuracy = false;

    /// <summary>
    /// Use to avoid unnatural knees bending. 
    /// </summary>
    public bool fixKnee = false;

    /// <summary>
    /// Use to avoid wide angles of foot rotation on surface.
    /// </summary>
    public bool footConstraint = false;

    private float m_Incline = 0.85f;
    private float m_InclineRadian;

    [Space(5)] [Range(0, MAX_STEP_HEIGHT)] [SerializeField]
    private float m_MaxStepHeight = 0.3f;

    /// <summary>
    /// Max height on which character can step up.
    /// </summary>
    public float MaxStepHeight
    {
        get => m_MaxStepHeight;
        set { m_MaxStepHeight = Mathf.Clamp(value, 0, MAX_STEP_HEIGHT); }
    }

    /// <summary>
    /// Offset of foot position on surface.
    /// </summary>
    [Range(-HEIGHT_OFFSET, HEIGHT_OFFSET)] [SerializeField]
    private float m_FootHeightOffset = 0;

    [Header("Movements Smooth")] [Range(0, MAX_DISTANCE_POWER)] [SerializeField]
    private float m_DistancePower = 1f;

    [Range(0, MAX_SMOOTHING_ANGLE)] [SerializeField]
    private float m_SmoothingAngle = 2f;

    [Range(0, SMOOTHING_POWER)] [SerializeField]
    private float m_GlobalSmoothingPower = 0f;

    /// <summary>
    /// Global smoothing power. Zero value disables smoothing.
    /// </summary>
    private float m_MinimalSmoothDistance = 0.005f;

    [Header("Casts")]
    /// <summary>
    /// Current type of the cast which is used to check surface under the character.
    /// </summary>
    public CastType type = CastType.Ray;

    /// <summary>
    /// Radius for Spherecast.
    /// </summary>
    [Range(0.01f, 0.3f)] public float sphereRadius = 0.03f;

    /// <summary>
    /// If there is an accessible surface under the character.
    /// </summary>
    public bool canReachTargets
    {
        get
        {
            if (m_LeftLeg != null && m_RightLeg != null)
                return m_LeftLeg.canReachTarget && m_RightLeg.canReachTarget;
            else
                return false;
        }
    }

    /// <summary>
    /// Returns the lowest foot position.y. Sets character tranform.position.y for better positioning. 
    /// </summary>
    public float LowestFootHeight
    {
        get
        {
            if (m_LeftLeg.LowestHitPoint.y < m_RightLeg.LowestHitPoint.y)
                return m_LeftLeg.LowestHitPoint.y;
            else
                return m_RightLeg.LowestHitPoint.y;
        }
    }

    public float LowestFootDist
    {
        get
        {
            if (m_LeftLeg.LowestHitPoint.y < m_RightLeg.LowestHitPoint.y)
                return m_LeftLeg.Length;
            else
                return m_RightLeg.Length;
        }
    }
    /// <summary>
    /// Returns the lowest foot position. Sets character tranform.position.y for better positioning.
    /// </summary>
    public Vector3 LowestFootPosition
    {
        get
        {
            if (m_LeftLeg.LowestHitPoint.y < m_RightLeg.LowestHitPoint.y)
                return m_LeftLeg.LowestHitPoint;
            else
                return m_RightLeg.LowestHitPoint;
        }
    }

    /// <summary>
    /// Returns the lowest foot position, takes into account the character movement. Set character tranform.position.y for better positioning.
    /// </summary>
    /// <param name="moveDirection">Global vector of character movement direction.</param>
    /// <returns></returns>
    public Vector3 DirectionalFootHeight(Vector3 moveDirection)
    {
        float left = Vector3.Dot(m_LeftLeg.LowBonePosition - transform.position, moveDirection);
        float right = Vector3.Dot(m_RightLeg.LowBonePosition - transform.position, moveDirection);

        if (left > right)
            return m_LeftLeg.LowestHitPoint;
        else
            return m_RightLeg.LowestHitPoint;
    }

    private IKPassRedirectorBehavior _ikPassRedirector;
    private LateUpdateCaller _lateUpdateCaller;
    private IBaseInputReader _baseInputReader;
    
    public override void OnEnterState()
    {
        _ikPassRedirector = m_Anim.GetBehaviour<IKPassRedirectorBehavior>();
        _ikPassRedirector.OnStateIKEvent += OnStateIK;
        _lateUpdateCaller.OnLateUpdate += OnLateUpdateState;
    }
    public override void OnExitState()
    {
        _ikPassRedirector.OnStateIKEvent -= OnStateIK;
        _lateUpdateCaller.OnLateUpdate -= OnLateUpdateState;
    }
    
    public override void CacheReferences(IStateMachineVariables variables, IReferenceResolver resolver)
    {
        if (m_IsInitialized) return;

        transform = resolver.GetComponent<Transform>();
        m_Anim = resolver.GetComponent<Animator>();
        _baseInputReader = resolver.GetComponent<IBaseInputReader>();
        _lateUpdateCaller = resolver.AddComponent<LateUpdateCaller>();
        
        if (!m_Anim.avatar) throw new Exception("No Avatar to use");
        if (!m_Anim.isHuman) throw new Exception("Not Humanoid Avatar");
        if (!m_Anim.runtimeAnimatorController) throw new Exception("No Animator Controller");

        if (AnimatorBone(HumanBodyBones.LeftToes) && AnimatorBone(HumanBodyBones.RightToes))
        {
            m_LeftLeg = new Limb(AnimatorBone(HumanBodyBones.LeftUpperLeg), AnimatorBone(HumanBodyBones.LeftLowerLeg),
                AnimatorBone(HumanBodyBones.LeftFoot), AnimatorBone(HumanBodyBones.LeftToes));
            m_RightLeg = new Limb(AnimatorBone(HumanBodyBones.RightUpperLeg),
                AnimatorBone(HumanBodyBones.RightLowerLeg), AnimatorBone(HumanBodyBones.RightFoot),
                AnimatorBone(HumanBodyBones.RightToes));

            float leftHeelToeDistance = Vector3.Distance(AnimatorBone(HumanBodyBones.LeftFoot).position,
                AnimatorBone(HumanBodyBones.LeftLowerLeg).position);
            float rightHeelToeDistance = Vector3.Distance(AnimatorBone(HumanBodyBones.RightFoot).position,
                AnimatorBone(HumanBodyBones.RightLowerLeg).position);

            if (leftHeelToeDistance <= sphereRadius || rightHeelToeDistance <= sphereRadius)
            {
                if (leftHeelToeDistance > rightHeelToeDistance)
                    sphereRadius = rightHeelToeDistance / 3f;
                else
                    sphereRadius = leftHeelToeDistance / 3f;

                if (sphereRadius <= 0.01) sphereRadius = 0.01f;
            }
        }
        else
        {
            m_LeftLeg = new Limb(AnimatorBone(HumanBodyBones.LeftUpperLeg), AnimatorBone(HumanBodyBones.LeftLowerLeg),
                AnimatorBone(HumanBodyBones.LeftFoot));
            m_RightLeg = new Limb(AnimatorBone(HumanBodyBones.RightUpperLeg),
                AnimatorBone(HumanBodyBones.RightLowerLeg), AnimatorBone(HumanBodyBones.RightFoot));
        }

        Vector3 lowestPoint = transform.position;
        lowestPoint.y = resolver.GetComponent<CapsuleCollider>().bounds.min.y;

        m_MeshHeightOffset = Vector3.Dot(lowestPoint - transform.position, transform.up);
        m_LeftLeg.distanceFromMesh = Vector3.Dot(m_LeftLeg.LowBonePosition - transform.position, transform.up) -
                                     m_MeshHeightOffset;
        m_RightLeg.distanceFromMesh = Vector3.Dot(m_RightLeg.LowBonePosition - transform.position, transform.up) -
                                      m_MeshHeightOffset;

        m_InclineRadian = Mathf.Acos(m_Incline); // Max incline in radian from vector up;
        m_IsInitialized = true;
    }

    public void OnLateUpdateState()
    {
        if (!m_Anim.enabled) return;

        if (m_Anim.updateMode != AnimatorUpdateMode.AnimatePhysics)
            FootIK();
        else
        {
            RotationSynchronization(m_LeftLeg);
            RotationSynchronization(m_RightLeg);
        }
    }


    /// <summary>
    /// Works only with AnimatorUpdateMode.AnimatePhysics and enabled the IK Pass checkbox in the Layer settings in Animator window.
    /// </summary>
    private void OnStateIK()
    {
        var pos = m_Anim.bodyPosition;
        
        pos.y = (m_LeftLeg.LowestHitPoint+ m_RightLeg.LowestHitPoint).y/2+ (m_LeftLeg.Length+ m_RightLeg.Length)/2;
        //if (_baseInputReader.moveInputMagnitude < 0.01f)
        //{
        //    pos.y = LowestFootHeight+ LowestFootDist;
        //}
        //else
        //{
        //    pos.y = DirectionalFootHeight(transform.forward).y+ DirectionalPlaceHeight(transform.forward);
        //}
        m_Anim.bodyPosition = pos;
        if (m_Anim.updateMode == AnimatorUpdateMode.AnimatePhysics) FootIK();

       
    }

    public float DirectionalPlaceHeight(Vector3 moveDirection)
    {
        float left = Vector3.Dot(m_LeftLeg.LowBonePosition - transform.position, moveDirection);
        float right = Vector3.Dot(m_RightLeg.LowBonePosition - transform.position, moveDirection);

        if(left > right)
            return m_LeftLeg.Length;
        else
            return m_RightLeg.Length;
    }

    /// <summary>
    /// Calculates legs' position. Can be used from another componet with "true" outsideUpdate parameter.
    /// </summary>
    public void FootIK()
    {
        SetPositionRotationFromRayCast(m_LeftLeg);
        Smoothing(m_LeftLeg);
        FootsPlacement(m_LeftLeg, null);

        SetPositionRotationFromRayCast(m_RightLeg);
        Smoothing(m_RightLeg);
        FootsPlacement(m_RightLeg, null);

        GlobalSmoothing(m_LeftLeg);
        GlobalSmoothing(m_RightLeg);

        SavingPositionRotation(m_LeftLeg);
        SavingPositionRotation(m_RightLeg);
    }

    #region IK voids

    /// <summary>
    /// Set the target position and rotation
    /// </summary>
    /// <param name="limb">Character limb.</param>
    private void SetPositionRotationFromRayCast(Limb legIK)
    {
        var down = -transform.up;
        var up = -down;

        float currentHeight = Vector3.Dot(up, legIK.LowBonePosition - transform.position);

        var low = UniCast(legIK.LowBonePosition + up * (m_MaxStepHeight - currentHeight), down, out var hit);

        if (increasedAccuracy && legIK.ExtraBone)
        {
            currentHeight = Vector3.Dot(up, legIK.ExtraBone.position - transform.position);

            var extra = UniCast(legIK.ExtraBone.position + up * (m_MaxStepHeight - currentHeight), down,
                out var extraHit);

            if (low && extra)
            {
                Vector3 localLow = transform.InverseTransformPoint(hit.point);
                Vector3 localExtra = transform.InverseTransformPoint(extraHit.point);

                if (localLow.y > localExtra.y)
                {
                    SetFootFromLow(legIK, hit, extraHit);
                    legIK.LowestHitPoint = extraHit.point;
                }
                else
                {
                    SetFootFromExtra(legIK, extraHit, hit);
                    legIK.LowestHitPoint = hit.point;
                }
            }
            else if (low && !extra)
            {
                SetFootFromLow(legIK, hit);
                legIK.LowestHitPoint = hit.point;
            }
            else if (!low && extra)
            {
                SetFootFromExtra(legIK, extraHit);
                legIK.LowestHitPoint = extraHit.point;
            }
            else
                legIK.canReachTarget = false;
        }
        else
        {
            if (low)
            {
                SetFootFromLow(legIK, hit);
                legIK.LowestHitPoint = hit.point;
            }
            else
                legIK.canReachTarget = false;
        }
    }

    private bool UniCast(Vector3 point, Vector3 direction, out RaycastHit hit)
    {
        var ray = new Ray(point, direction);
        bool result = false;
        if (type == CastType.Ray)
            result = Physics.Raycast(ray, out hit, m_MaxStepHeight * 2, ~ignoreLayers);
        else if (type == CastType.Sphere)
            result = Physics.SphereCast(ray, sphereRadius, out hit, m_MaxStepHeight * 2, ~ignoreLayers);
        else
        {
            result = Physics.Raycast(ray, out hit, m_MaxStepHeight * 2, ~ignoreLayers);
            if (!result) result = Physics.SphereCast(ray, sphereRadius, out hit, m_MaxStepHeight * 2, ~ignoreLayers);
        }

        return result;
    }

    /// <summary>
    /// Use low (foot) bone to find the target position
    /// </summary>
    /// <param name="limb">Character limb.</param>
    /// <param name="lowHit">Hit point.</param>
    private void SetFootFromLow(Limb limb, RaycastHit lowHit, RaycastHit? extraHit = null)
    {
        if (extraHit.HasValue) lowHit.normal = (lowHit.normal + extraHit.Value.normal) / 2;

        if (footConstraint) lowHit.normal = ConstraintedNormal(lowHit.normal);

        Vector3 animatorHeight = transform.up * (Vector3.Dot(transform.up, limb.LowBonePosition - transform.position) -
                                                 limb.distanceFromMesh - m_MeshHeightOffset);
        Vector3 footHeight = lowHit.normal * (limb.distanceFromMesh - m_FootHeightOffset);

        limb.targetPosition =
            lowHit.point + footHeight +
            animatorHeight; //Add height as in animation clip and height offsets to hit point
        limb.targetRotation = Quaternion.FromToRotation(transform.up, lowHit.normal) * limb.LowBoneRotation;

        limb.canReachTarget = ((Vector3.Distance(limb.UpBone.position, limb.targetPosition) <=
                                (limb.UpperLength + limb.LowerLength + m_MaxStepHeight)));
    }

    /// <summary>
    /// Uses extra (toe) bone to find the target position
    /// </summary>
    /// <param name="limb">Character limb.</param>
    /// <param name="extraHit">Hit point.</param>
    private void SetFootFromExtra(Limb limb, RaycastHit extraHit, RaycastHit? lowHit = null)
    {
        if (lowHit.HasValue) extraHit.normal = (extraHit.normal + lowHit.Value.normal) / 2;

        if (footConstraint) extraHit.normal = ConstraintedNormal(extraHit.normal);

        Quaternion fromUpToNormal = Quaternion.FromToRotation(transform.up, extraHit.normal);

        Vector3 footDirection = (limb.ExtraBone.position - limb.LowBone.position);

        float forward = Vector3.Dot(footDirection, transform.forward);
        float right = Vector3.Dot(footDirection, transform.right);
        footDirection = transform.rotation * new Vector3(right, 0, forward);

        Vector3 directioOnGround = fromUpToNormal * footDirection;

        Vector3 animatorHeight = transform.up * (Vector3.Dot(transform.up, limb.LowBonePosition - transform.position) -
                                                 limb.distanceFromMesh - m_MeshHeightOffset);
        Vector3 footHeight = extraHit.normal * (limb.distanceFromMesh - m_FootHeightOffset);

        limb.targetPosition =
            extraHit.point - directioOnGround + footHeight +
            animatorHeight; //Add height as in animation clip and height offsets to hit point
        limb.targetRotation = Quaternion.FromToRotation(transform.up, extraHit.normal) * limb.LowBoneRotation;

        limb.canReachTarget = ((Vector3.Distance(limb.UpBone.position, limb.targetPosition) <=
                                (limb.UpperLength + limb.LowerLength + m_MaxStepHeight)));
    }

    /// <summary>
    /// Places foots using Cosine Rule
    /// </summary>
    /// <param name="limb">Character limb.</param>
    /// <param name="pole">Target to calculate new plane.</param>
    private void FootsPlacement(Limb limb, Transform pole)
    {
        if (limb.canReachTarget)
        {
            Vector3 planeThirdPoint;
            float targetDistance = Mathf.Min((limb.targetPosition - limb.UpBone.position).magnitude,
                limb.LowerLength + limb.UpperLength - .001f);

            if (!fixKnee)
            {
                planeThirdPoint =
                    (limb.MiddleBone.position + limb.LowBone.position) / 2 +
                    transform.forward * 0.05f; // or limb.MiddleBone.position
            }
            else
            {
                Vector3 targetVector = limb.MiddleBone.position - limb.UpBone.position;
                targetVector.Normalize();

                float ang = Vector3.Angle(transform.right, targetVector);
                Vector3 cross = Quaternion.AngleAxis(-ang, transform.up) * transform.forward;
                cross.Normalize();

                targetVector = Quaternion.AngleAxis(90, cross) * targetVector;
                planeThirdPoint = limb.MiddleBone.position + targetVector * limb.Length;
            }

            float targetAngle = limb.MiddleBoneAngle(targetDistance);
            float angle = Vector3.Angle(limb.LowBonePosition - limb.MiddleBone.position,
                limb.UpBone.position - limb.MiddleBone.position); // Angle between bones in animator
            Vector3 axis = Vector3.Cross(limb.LowBonePosition - planeThirdPoint,
                limb.UpBone.position - planeThirdPoint); // Rotation of angle around this axis
            axis.Normalize();

            //limb.MiddleBone.RotateAround(limb.MiddleBone.position, axis, angle - targetAngle);// Add rotation to middle bone, (same below)

            limb.MiddleBone.rotation =
                Quaternion.AngleAxis(angle - targetAngle, axis) *
                limb.MiddleBone.rotation; // Add rotation to middle bone
            limb.UpBone.rotation =
                Quaternion.FromToRotation(limb.LowBonePosition - limb.UpBone.position,
                    limb.targetPosition - limb.UpBone.position) * limb.UpBone.rotation; // Add rotation to upper bone

            limb.LowBoneRotation = limb.targetRotation;
        }
    }

    #endregion

    /// <summary>
    /// Smoothing legs movements.
    /// </summary>
    /// <param name="limb">Character limb.</param>
    private void Smoothing(Limb limb)
    {
        if (limb.canReachTarget && m_DistancePower > 0)
        {
            float animatededDistance = Vector3.Distance(limb.lastLowBoneAnimationPosition, limb.LowBonePosition);
            float movementDistance = Vector3.Distance(limb.lastLowBonePosition, limb.targetPosition);

            if (animatededDistance < m_MinimalSmoothDistance) animatededDistance = m_MinimalSmoothDistance;

            limb.targetPosition = Vector3.Lerp(limb.lastLowBonePosition, limb.targetPosition,
                (animatededDistance * (MAX_DISTANCE_POWER - m_DistancePower)) / movementDistance);
        }

        if (limb.canReachTarget && m_SmoothingAngle > 0)
        {
            float animatededAngle = Quaternion.Angle(limb.lastLowBoneAnimationRotation, limb.LowBoneRotation);
            float targetAngle = Quaternion.Angle(limb.lastLowBoneRotation, limb.targetRotation);

            limb.targetRotation = Quaternion.Lerp(limb.lastLowBoneRotation, limb.targetRotation,
                Mathf.Clamp01((animatededAngle + (MAX_SMOOTHING_ANGLE - m_SmoothingAngle)) / targetAngle));
        }

        limb.lastLowBoneAnimationPosition = limb.LowBone.position;
        limb.lastLowBoneAnimationRotation = limb.LowBoneRotation;
    }

    /// <summary>
    /// Smoothing legs movements.
    /// </summary>
    /// <param name="limb">Character limb.</param>
    private void GlobalSmoothing(Limb limb)
    {
        if (m_GlobalSmoothingPower <= 0) return;

        limb.UpBone.rotation =
            Quaternion.Lerp(limb.lastUpBoneRotation, limb.UpBone.rotation, 1 - m_GlobalSmoothingPower);
        limb.MiddleBone.rotation = Quaternion.Lerp(limb.lastMiddleBoneRotation, limb.MiddleBone.rotation,
            1 - m_GlobalSmoothingPower);
        limb.LowBoneRotation =
            Quaternion.Lerp(limb.lastLowBoneRotation, limb.LowBoneRotation, 1 - m_GlobalSmoothingPower);
    }

    /// <summary>
    /// Saves limb bones rotation
    /// </summary>
    /// <param name="limb">Character limb.</param>
    private void SavingPositionRotation(Limb limb)
    {
        limb.lastUpBoneRotation = limb.UpBone.rotation;
        limb.lastMiddleBoneRotation = limb.MiddleBone.rotation;
        limb.lastLowBoneRotation = limb.LowBone.rotation;

        limb.lastLowBonePosition = limb.LowBone.position;
    }

    /// <summary>
    /// Synchronizes bones rotation in case of use "AnimatorUpdateMode.AnimatePhysics"
    /// </summary>
    /// <param name="limb">Character limb.</param>
    private void RotationSynchronization(Limb limb)
    {
        limb.UpBone.rotation = limb.lastUpBoneRotation;
        limb.MiddleBone.rotation = limb.lastMiddleBoneRotation;
        limb.LowBone.rotation = limb.lastLowBoneRotation;
    }

    /// <summary>
    /// Checks Normal to constraint foots rotation;
    /// </summary>
    /// <param name="normal">Hit point normal.</param>
    /// <returns></returns>
    private Vector3 ConstraintedNormal(Vector3 normal)
    {
        if (normal.y < m_Incline)
            return Vector3.RotateTowards(transform.up, normal, m_InclineRadian,
                0f); // Max incline rotation from up to Normal vector 
        else
            return normal;
    }

    /// <summary>
    /// Returns bone transform
    /// </summary>
    /// <param name="bone"></param>
    /// <returns></returns>
    private Transform AnimatorBone(HumanBodyBones bone)
    {
        return m_Anim.GetBoneTransform(bone);
    }
}