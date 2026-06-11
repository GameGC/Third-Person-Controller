using System.Collections.Generic;
using UnityEngine;

public class TestController : MonoBehaviour
{
    [Header("Raycast Settings")]
    [SerializeField] private float raycastHeight = 1f;
    [SerializeField] private float raycastDistance = 2f;
    [SerializeField] private LayerMask groundMask;

    [Header("IK Settings")]
    [SerializeField] private float footHeightOffset = 0.02f;
    [SerializeField] private float footXOffset = 0.02f;

    [Header("CapsuleCast Settings")]
    [SerializeField] private float capsuleRadius = 0.1f;
    [SerializeField] private float capsuleHeight = 0.5f;

    private Animator animator;
    private Transform leftFootTransform;
    private Transform rightFootTransform;

    private readonly List<Vector3> sampledPoints = new();
    private readonly List<Quaternion> sampledRotations = new();

    private static Mesh capsuleMesh;
    
    private void Awake()
    {
        animator = GetComponent<Animator>();
        leftFootTransform = animator.GetBoneTransform(HumanBodyBones.LeftFoot);
        rightFootTransform = animator.GetBoneTransform(HumanBodyBones.RightFoot);
    }

    private void OnAnimatorIK(int layerIndex)
    {
        sampledPoints.Clear();
        sampledRotations.Clear();

        SampleSingleFootPosition(leftFootTransform);
        SampleSingleFootPosition(rightFootTransform);
    }

    private void SampleSingleFootPosition(Transform footTransform)
    {
        Vector3 offsetPos = footTransform.position + footTransform.forward * footXOffset;
        
        Vector3 capsuleBottom = offsetPos + Vector3.up * capsuleRadius;
        Vector3 capsuleTop = offsetPos + Vector3.up * (capsuleHeight - capsuleRadius);

        // Just one capsule cast straight down from foot position
        if (Physics.CapsuleCast(capsuleTop, capsuleBottom, capsuleRadius, Vector3.down, out RaycastHit hit, raycastDistance, groundMask))
        {
            Vector3 hitPoint = hit.point + Vector3.up * footHeightOffset;
            sampledPoints.Add(hitPoint);

            // Orient capsule so its up aligns with the ground normal
            Quaternion rotation = Quaternion.LookRotation(hit.normal);
            sampledRotations.Add(rotation);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (capsuleMesh == null)
            capsuleMesh = CreateCapsuleMesh();

        Gizmos.color = Color.green;

        for (var i = 0; i < sampledPoints.Count; i++)
        {
            var point = sampledPoints[i];
            // Calculate scale from capsuleHeight and capsuleRadius
            Vector3 scale = new Vector3(capsuleRadius * 2, capsuleHeight / 2, capsuleRadius * 2);

            // Create matrix for capsule position, scale, and identity rotation (vertical capsule)

            Gizmos.DrawWireMesh(capsuleMesh, 0, point, sampledRotations[i], scale);
        }
    }

    // Create a capsule mesh from a temporary primitive capsule GameObject
    private static Mesh CreateCapsuleMesh()
    {
        GameObject tempCapsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        Mesh mesh = tempCapsule.GetComponent<MeshFilter>().sharedMesh;
        Object.DestroyImmediate(tempCapsule);
        return mesh;
    }
}