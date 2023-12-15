using System;
using UnityEngine;

public class DefaultRaycastBullet : MonoBehaviour , IDamageSender
{
    private Transform transform;
    private Vector3 flyDestination;

    public LayerMask impactMask;
    [field: SerializeField] public float damage { get; private set; } = 10;
    public int speed = 100;
    public int distance = 1000;
    
    public SurfaceHitType HitType;
    public SurfaceEffect defaultImpactEffect;

    // Start is called before the first frame update

    private void Reset()
    {
        impactMask = LayerMask.GetMask("Default","Character","Char_Collision");
    }

    private void Start()
    {
        transform = base.transform;
        
        var ray = new Ray(transform.position, transform.forward);
        if (Physics.Raycast(ray,out var hit, distance, impactMask, QueryTriggerInteraction.Ignore))
        {
            //for (int i = 0; i < hitsCount; i++)
            //{
            //    Debug.Log(i+" "+hits[i].collider);
            //}

            Debug.Log(hit.collider.name);
            var isCharacter = hit.collider.gameObject.layer == LayerMask.NameToLayer("Character") || hit.collider.gameObject.layer == LayerMask.NameToLayer("Char_Collision");
            
            Debug.DrawLine(transform.position,hit.point,Color.red);
           // ref var hit = ref hits[isCharacter?Mathf.Min(hitsCount, 1):Mathf.Max(hitsCount-1, 0)];
            flyDestination = hit.point;
            try
            {
                SurfaceSystem.instance.OnSurfaceHit(hit,HitType, defaultImpactEffect);
            }
            catch (Exception e)
            {
            }

            if (isCharacter)
            {
                var gameObject = hit.collider.gameObject;
                bool isCharPart = gameObject.layer == LayerMask.NameToLayer("Char_Collision");
                if (!isCharPart)
                {
                    ray.origin = hit.point;
                    Physics.Raycast(ray, out hit, distance,
                        LayerMask.GetMask("Char_Collision"), QueryTriggerInteraction.Ignore);
                }
                gameObject.GetComponentInChildren<HealthComponent>()?.OnHit(in hit, this);
            }
        }
        else
        {
            flyDestination = transform.position + transform.forward * distance;
        }
    }

    public void Init(Vector3 flyDestination)
    {
        this.flyDestination = flyDestination;
    }

    // Update is called once per frame
    private void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, flyDestination, Time.deltaTime * speed);
        if(Vector3.Distance(transform.position,flyDestination)<0.1f)
            Destroy(gameObject);
    }

}

public interface IDamageSender
{
    public float damage { get; }
}
//public class BulletEventArgs : Eve