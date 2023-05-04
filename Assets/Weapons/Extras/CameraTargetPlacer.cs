using System;
using UnityEngine;

public class CameraTargetPlacer : MonoBehaviour, IBatchRaycasterSingle
{
    private Transform _transform;
    [SerializeField] private Transform targetTransform;

    private void Awake() => _transform = base.transform;

    private void OnEnable() => RaycastManager.SubscribeRaycastCalls(this);

    private void OnDisable() => RaycastManager.UnSubscribeRaycastCalls(this);

    private void FixedUpdate() => this.Raycast(0,_transform.position,_transform.forward,100);

    public void OnRaycastResult(RaycastHit hit)
    {
        if (hit.collider != null)
        {
            targetTransform.position = hit.point;
        }
        else
        {
            targetTransform.position = _transform.position + _transform.forward * 100;
        }
    }
}
