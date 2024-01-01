using UnityEngine;

public class CameraTargetPlacer : MonoBehaviour
{
    private Transform _transform;
    [SerializeField] private Transform targetTransform;

    public Transform Target => targetTransform;
    private void Awake() => _transform = transform;

   // private void OnEnable() => RaycastManager.SubscribeRaycastCalls(this);

  //  private void OnDisable() => RaycastManager.UnSubscribeRaycastCalls(this);

    private void FixedUpdate()
    {
        if (Physics.Raycast(_transform.position, _transform.forward, out var hit, 100))
            targetTransform.position = hit.point;
        else
            targetTransform.position = _transform.position + _transform.forward * 100;
    }
}
