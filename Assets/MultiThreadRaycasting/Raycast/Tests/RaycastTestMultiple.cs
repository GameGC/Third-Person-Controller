using System;
using UnityEngine;

public class RaycastTestMultiple: MonoBehaviour,IBatchRaycasterMultiple
{
    private void OnEnable()
    {
        RaycastManager.SubscribeRaycastCalls(this);
    }

    private void OnDisable()
    {
        RaycastManager.UnSubscribeRaycastCalls(this);
    }

    private void FixedUpdate()
    {
        this.Raycast(0,transform.position,transform.forward);
        this.Raycast(1,transform.position,-transform.forward);
        this.Raycast(2,transform.position,transform.up);
        this.Raycast(3,transform.position,-transform.up);
        this.Raycast(4,transform.position,transform.right);
        this.Raycast(5,transform.position,-transform.right);
    }

    public int totalPossibleRaycasts => 6;
    public void OnRaycastResult(RaycastHit[] hits)
    {
        string[] names = new[] {"forward", "back", "up", "down", "right", "left"};
        for (int i = 0; i < 6; i++)
        {
            Debug.Log($"{names[i]}: {hits[i].collider?.name}");
        }
    }
}