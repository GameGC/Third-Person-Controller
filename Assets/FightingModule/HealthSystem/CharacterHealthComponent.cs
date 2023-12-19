using System;
using System.Collections;
using ThirdPersonController.Core.DI;
using UnityEngine;

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


    private bool _isStarted;
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
        
        _isStarted = true;
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
    

    public override void OnHit(in RaycastHit hit, IDamageSender source)
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
        
        foreach (var onHitFeature in OnHitFeatures) 
            onHitFeature.OnHit(Health + damage, Health, hit, source);

        if (Health < 0.01f)
        {
            foreach (var onDeathFeature in OnDeathFeatures) 
                onDeathFeature.OnHit(Health + damage, Health, hit, source);
        }
    }

    private void Destroy() => Destroy(ReferenceResolver.gameObject);
    private void Destroy(float delay) => Destroy(ReferenceResolver.gameObject,delay);

}