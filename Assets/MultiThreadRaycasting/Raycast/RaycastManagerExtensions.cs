using UnityEngine;

public static class RaycastManagerExtensions
{
    private const int DefaultRaycastLayers = -5;
    
    
    
    
    public static void Raycast(
        this IBatchRaycaster requestor,int raycastId,
        Vector3 origin, Vector3 direction, float maxDistance = Mathf.Infinity, int layerMask = DefaultRaycastLayers)
    {
        RaycastManager.Raycast(requestor, raycastId,origin, direction, maxDistance, layerMask);
    }
    
    public static void Raycast(
        this IBatchRaycaster requestor,int raycastId,
        Vector3 origin, Vector3 direction, RaycastHit hitInfo, float maxDistance = Mathf.Infinity, int layerMask = DefaultRaycastLayers)
    {
        RaycastManager.Raycast(requestor, raycastId,origin, direction, hitInfo, maxDistance, layerMask);
    }
    
    public static void Raycast(
        this IBatchRaycaster requestor,int raycastId,
        Ray ray, float maxDistance = Mathf.Infinity, int layerMask = DefaultRaycastLayers)
    {
        RaycastManager.Raycast(requestor, raycastId,ray.origin, ray.direction, maxDistance, layerMask);
    }
    
    public static void Raycast(
        this IBatchRaycaster requestor,int raycastId,
        Ray ray, RaycastHit hitInfo, float maxDistance = Mathf.Infinity, int layerMask = DefaultRaycastLayers)
    {
        RaycastManager.Raycast(requestor, raycastId,ray.origin,ray.direction, hitInfo, maxDistance, layerMask);
    }
    
    public static void Linecast(
        this IBatchRaycaster requestor,int raycastId,
        Vector3 start,
        Vector3 end,
        RaycastHit hitInfo,
        int layerMask = DefaultRaycastLayers)
    {
        Vector3 direction = end - start;
        RaycastManager.Raycast(requestor, raycastId,start, direction, hitInfo, direction.magnitude, layerMask);
    }
}