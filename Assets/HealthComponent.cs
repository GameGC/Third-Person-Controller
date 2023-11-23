using System;
using System.Collections;
using System.Collections.Generic;
using ThirdPersonController.Core;
using UnityEngine;

public class HealthComponent : MonoBehaviour
{
    [Serializable]
    public struct HitBox
    {
        public Collider Collider;
        public float damageMultiplicator;
    }

    [SerializeField] private HitBox[] hitboxes;
    [SerializeField] private int health = 100;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider == hitboxes[0].Collider)
        {
            Debug.Log("hiiiiit");
        }
    }

    public void OnHit(RaycastHit hit)
    {
        if (hit.collider == hitboxes[0].Collider)
        {
            Debug.Log("hiiiiit");
        }
    }
}
