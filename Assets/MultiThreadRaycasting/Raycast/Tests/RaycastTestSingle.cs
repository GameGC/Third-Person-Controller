using UnityEngine;

public class RaycastTestSingle: MonoBehaviour,IBatchRaycasterSingle
{
    private void OnEnable()
    {
        RaycastManager.SubscribeRaycastCalls(this);
        this.Raycast(0,transform.position,transform.forward);
    }

    private void OnDisable()
    {
        RaycastManager.UnSubscribeRaycastCalls(this);
    }


    public int totalPossibleRaycasts => 1;
    public void OnRaycastResult(RaycastHit hit)
    {
        Debug.Log(hit.collider?.name);
    }
}