using MTPS.Shooter.WeaponsSystem.ShootableWeapon;
using UnityEngine;

public class HitZone : MonoBehaviour, IDamageSender
{
    [field:SerializeField] public float damage { get; private set; }
    [field: SerializeField] public SurfaceHitType HitType { get; private set; } = SurfaceHitType.Hand;

    private Rigidbody _thisColliderRigidbody;
    private void Awake()
    {
        _thisColliderRigidbody = GetComponent<Collider>().attachedRigidbody;
    }

    private void OnTriggerEnter(Collider hitCollider)
    {
        if(hitCollider.attachedRigidbody == _thisColliderRigidbody) return;

        var healthComponent = hitCollider.GetComponentInParent<CharacterHealthComponent>();

        if(!healthComponent) return;
        //detect that hit collider not ours
        Collider rootCollider = hitCollider;
        if (hitCollider.gameObject != healthComponent.gameObject)
            rootCollider = healthComponent.GetComponent<Collider>();
        
        healthComponent.OnHit(transform.position,hitCollider,rootCollider,this);
    }

    //private void OnTriggerExit(Collider hitCollider)
    //{
    //    if(hitCollider.attachedRigidbody == _thisColliderRigidbody) return;
    //    Debug.Log(hitCollider.name);
    //}
}
