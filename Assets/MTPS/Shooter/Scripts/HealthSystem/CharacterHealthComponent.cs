using System;
using System.Collections;
using System.Threading.Tasks;
using GameGC.SurfaceSystem;
using MTPS.Core;
using MTPS.Core.Attributes;
using MTPS.Shooter.WeaponsSystem.ShootableWeapon;
using UnityEngine;

[DisallowMultipleComponent]
public class CharacterHealthComponent : HealthComponent, ICharacterHealthVariable
{ 
    [Serializable]
    public struct HitBox
    {
        public Collider Collider;
        public float damageMultiplicator;
    }

    HitBox[] ICharacterHealthVariable.HitBoxes => hitBoxes;

    
    [SerializeField] private HitBox[] hitBoxes;
    
    [SerializeField] protected bool startWhenResolverIsReady = true;
    public ReferenceResolver ReferenceResolver;
    
    [SerializeReference,SerializeReferenceAddButton(typeof(BaseHealthFeature))] 
    public BaseHealthFeature[] OnHitFeatures = Array.Empty<BaseHealthFeature>();
    
    [SerializeReference,SerializeReferenceAddButton(typeof(BaseHealthFeature))] 
    public BaseHealthFeature[] OnDeathFeatures = Array.Empty<BaseHealthFeature>();


    private int _hitBoxCount;

    protected virtual void Awake()
    {
        _hitBoxCount = hitBoxes.Length;
        
        if (startWhenResolverIsReady)
        {
            return;
        }

        CacheReferences();
    }

    protected virtual IEnumerator Start()
    {
        if (startWhenResolverIsReady)
        {
            if (!ReferenceResolver.isReady)
                yield return new WaitUntil(() => ReferenceResolver.isReady);
            
            CacheReferences();
        }
    }

    private void CacheReferences()
    {
        var target = ReferenceResolver.gameObject;
        
        foreach (var onHitFeature in OnHitFeatures)
        {
            onHitFeature.CacheReferences(this, ReferenceResolver);
            onHitFeature.Destroy = Destroy;
            onHitFeature.DestroyDelayed = Destroy;
            onHitFeature.SetActive = target.SetActive;
        }

        foreach (var onDeathFeature in OnDeathFeatures)
        {
            onDeathFeature.CacheReferences(this, ReferenceResolver);
            onDeathFeature.Destroy = Destroy;
            onDeathFeature.DestroyDelayed = Destroy;
            onDeathFeature.SetActive = target.SetActive;
        }
    }
    

    public override async void OnHit(RaycastHit hit, IDamageSender source)
    {
        bool alivePreviously = Health > 0;
        float damage = source.damage;

        if (alivePreviously)
        {
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
        }

        SurfaceSystem.instance.OnSurfaceHit(hit,(int) source.HitType,defaultHitEffect);

        if (alivePreviously)
        {
            //delay for realistic velocity copy to rigidbody
            await Task.Yield();
            CallHitFeatures(Health + damage, Health, hit.point, hit.normal, source);
        }
    }

    public async void OnHit(Vector3 hitPointOrigin,Collider hitCollider,Collider rootCollider, IDamageSender source)
    {
        bool alivePreviously = Health > 0;
        float damage = source.damage;

        if (alivePreviously)
        {
            if (hitCollider != rootCollider && _hitBoxCount > 0)
            {
                for (int i = 0; i < _hitBoxCount; i++)
                {
                    if (hitBoxes[i].Collider != hitCollider) continue;
                    damage *= hitBoxes[i].damageMultiplicator;
                    break;
                }
            }

            Health -= damage;
        }

        var hitPointClosest = hitCollider.ClosestPoint(hitPointOrigin);
        var hitNormal = (hitPointClosest - hitPointOrigin).normalized;
        
        
        SurfaceSystem.instance.OnSurfaceHit(hitCollider,rootCollider,hitPointClosest,hitNormal,defaultHitEffect);

        if (alivePreviously)
        {
            //delay for realistic velocity copy to rigidbody
            await Task.Yield();
            CallHitFeatures(Health + damage, Health, in hitPointClosest, in hitNormal, source);
        }
    }

    private void CallHitFeatures(in float previousHealth,in float newHealth, in Vector3 hitPoint, in Vector3 hitDirection,IDamageSender damageSender)
    {
        foreach (var onHitFeature in OnHitFeatures) 
            onHitFeature.OnHit(in previousHealth, in newHealth, in hitPoint,in hitDirection, damageSender);

        if (newHealth > 0.01f) return;
        foreach (var onDeathFeature in OnDeathFeatures) 
            onDeathFeature.OnHit(in previousHealth, in newHealth, in hitPoint,in hitDirection, damageSender);
    }
    
    

    private void Destroy() => Destroy(ReferenceResolver.gameObject);
    private void Destroy(float delay) => Destroy(ReferenceResolver.gameObject,delay);

}