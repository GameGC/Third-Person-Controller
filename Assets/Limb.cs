using UnityEngine;

public class Limb
{
    public Transform UpBone { get; }
    public Transform MiddleBone { get; }
    public Transform LowBone { get; }
    public Transform ExtraBone { get; }

    public float UpperLength { get; }
    public float LowerLength { get; }
    public float Length { get; }

    public float distanceFromMesh;
    private float CosineRuleNumeratorPart;
    private float CosineRuleDenominator;

    public Vector3 targetPosition;
    public Quaternion targetRotation;

    public Vector3 lastLowBonePosition;

    public Quaternion lastLowBoneRotation;
    public Quaternion lastMiddleBoneRotation;
    public Quaternion lastUpBoneRotation;

    public Vector3 lastLowBoneAnimationPosition;
    public Quaternion lastLowBoneAnimationRotation;

    public bool canReachTarget;
    public Vector3 LowestHitPoint;

    public Vector3 LowBonePosition => LowBone.position;

    public Quaternion LowBoneRotation
    {
        get => LowBone.rotation;
        set => LowBone.rotation = value;
    }

    public Limb(Transform upBone, Transform middleBone, Transform lowBone)
    {
        UpBone = upBone;
        MiddleBone = middleBone;
        LowBone = lowBone;

        var upBonePos = upBone.position;
        var midBonePos = middleBone.position;
        lowBone.GetPositionAndRotation(out var lowBonePos,out var lowBoneRot);
                
        UpperLength = (upBonePos - midBonePos).magnitude;
        LowerLength = (midBonePos - lowBonePos).magnitude;
        Length = UpperLength + LowerLength;

        var upperLengthSquared = (upBonePos - midBonePos).sqrMagnitude;
        var lowerLengthSquared = (midBonePos - lowBonePos).sqrMagnitude;

        CosineRuleNumeratorPart = upperLengthSquared + lowerLengthSquared;
        CosineRuleDenominator = 2 * UpperLength * LowerLength;

        lastLowBonePosition = lowBonePos;

        lastLowBoneAnimationPosition = lowBonePos;
        lastLowBoneAnimationRotation = lowBoneRot;

        canReachTarget = false;
    }
        
    public Limb(Transform upBone, Transform middleBone, Transform lowBone, Transform extraBone)
    {
        UpBone = upBone;
        MiddleBone = middleBone;
        LowBone = lowBone;
        ExtraBone = extraBone;

        var upBonePos = upBone.position;
        var midBonePos = middleBone.position;
        lowBone.GetPositionAndRotation(out var lowBonePos,out var lowBoneRot);
                
        UpperLength = (upBonePos - midBonePos).magnitude;
        LowerLength = (midBonePos - lowBone.position).magnitude;
        var extraLength = (extraBone.position - lowBonePos).magnitude;
        Length = UpperLength + LowerLength + extraLength;

        var upperLengthSquared = (upBonePos - midBonePos).sqrMagnitude;
        var lowerLengthSquared = (midBonePos - lowBonePos).sqrMagnitude;

        CosineRuleNumeratorPart = upperLengthSquared + lowerLengthSquared;
        CosineRuleDenominator = 2 * UpperLength * LowerLength;

        lastLowBonePosition = lowBonePos;

        lastLowBoneAnimationPosition  = lowBonePos;
        lastLowBoneAnimationRotation = lowBoneRot;

        canReachTarget = false;
    }
    /// <summary>
    /// Cosine Rule to find middle bone angle
    /// </summary>
    /// <param name="targetDistance"></param>
    /// <returns></returns>
    public float MiddleBoneAngle(float targetDistance)
    {
        return Mathf.Acos(Mathf.Clamp((CosineRuleNumeratorPart - (targetDistance * targetDistance)) / CosineRuleDenominator, -1, 1)) * Mathf.Rad2Deg;
    }
}