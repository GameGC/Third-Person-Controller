using System;
using UnityEngine;
using UnityEngine.Serialization;

public class HealthComponent : MonoBehaviour
{
    [Serializable]
    public struct HitBox
    {
        public Collider Collider;
        public float damageMultiplicator;
    }

    [SerializeField] private HitBox[] hitBoxes;
    [field: SerializeField] public float Health { get; private set; } = 100;
    [FormerlySerializedAs("hitEffect")] public SurfaceEffect defaultHitEffect;

    private int _hitBoxCount;

    protected virtual void Awake()
    {
        _hitBoxCount = hitBoxes.Length;
    }

   //private void OnCollisionEnter(Collision collision)
   //{
   //    if (collision.collider == hitBoxes[0].Collider)
   //    {
   //        Debug.Log("hiiiiit");
   //    }
   //}

    public virtual void OnHit(in RaycastHit hit,IDamageSender source)
    {
        float damage = source.damage;
        if (_hitBoxCount > 0)
        {
            for (int i = 0; i < _hitBoxCount; i++)
            {
                if (hitBoxes[i].Collider != hit.collider) continue;
                damage *= hitBoxes[i].damageMultiplicator;
                break;
            }
        }
        Health -= damage;
        SurfaceSystem.instance.OnSurfaceHit(hit,source.HitType,defaultHitEffect);
    }
}