using UnityEngine;

public class CameraTargetPlacer : MonoBehaviour
{
    Transform transform;
    [SerializeField] Transform targetTransform;
    
    void Awake()
    {
        transform = base.transform;
    }

    void FixedUpdate()
    {
        if (Physics.Raycast(transform.position, transform.forward, out var hit, 100))
            targetTransform.position = hit.point;
        else
            targetTransform.position = transform.position + transform.forward * 100;
    }
}
