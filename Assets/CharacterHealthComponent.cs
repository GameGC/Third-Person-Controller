using System;
using System.Collections;
using ThirdPersonController.Core;
using ThirdPersonController.Core.DI;
using UnityEngine;

public class CharacterHealthComponent : HealthComponent
{ 
    [SerializeField] protected bool startWhenResolverIsReady = true;
    public ReferenceResolver ReferenceResolver;
    
    [SerializeReference,FuzzyAddButton(typeof(BaseFeature))] 
    public BaseFeature[] aliveFeatures = Array.Empty<BaseFeature>();
    
    [SerializeReference,FuzzyAddButton(typeof(BaseFeature))] 
    public BaseFeature[] OnHitFeatures = Array.Empty<BaseFeature>();
    
    [SerializeReference,FuzzyAddButton(typeof(BaseFeature))] 
    public BaseFeature[] OnDeathFeatures = Array.Empty<BaseFeature>();

    private bool _isStarted;

    protected override void Awake()
    {
        base.Awake();
        
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
        
        EnterInitialState();
        _isStarted = true;
    }

    private void CacheReferences()
    {
        foreach (var aliveFeature in aliveFeatures) 
            aliveFeature.CacheReferences(null, ReferenceResolver);

        foreach (var onHitFeature in OnHitFeatures) 
            onHitFeature.CacheReferences(null, ReferenceResolver);

        foreach (var onDeathFeature in OnDeathFeatures) 
            onDeathFeature.CacheReferences(null, ReferenceResolver);
    }

    private void EnterInitialState()
    {
        foreach (var feature in Health > 0 ? aliveFeatures : OnDeathFeatures) 
            feature.OnEnterState();
    }

    public override void OnHit(in RaycastHit hit, IDamageSender source)
    {
        base.OnHit(in hit, source);
        foreach (var onHitFeature in OnHitFeatures)
        {
            onHitFeature.OnEnterState();
            onHitFeature.OnExitState();
        }
        
        if (Health < 0.01f)
        {
            Debug.Log(Health);
            foreach (var onDeathFeature in OnDeathFeatures)
            {
                onDeathFeature.OnEnterState();
                onDeathFeature.OnExitState();
            }

            Debug.Log("destroy");
            Destroy(GetComponentInParent<ReferenceResolver>().gameObject);
        }
    }

    private void Update()
    {
        if(!_isStarted) return;
        foreach (var feature in Health > 0 ? aliveFeatures : OnDeathFeatures) 
            feature.OnUpdateState();
    }

    private void FixedUpdate()
    {
        if(!_isStarted) return;
        foreach (var feature in Health > 0 ? aliveFeatures : OnDeathFeatures) 
            feature.OnFixedUpdateState();
    }
}