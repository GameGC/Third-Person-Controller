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
    
    public SurfaceEffect defaultImpactEffect;

    private RaycastHit[] hits = new RaycastHit[2];
    // Start is called before the first frame update

    private void Reset()
    {
        impactMask = LayerMask.GetMask("Default","Character","Char_Collision");
    }

    private void Start()
    {
        transform = base.transform;
        
        var ray = new Ray(transform.position, transform.forward);
        int hitsCount = Physics.RaycastNonAlloc(ray, hits, distance, impactMask, QueryTriggerInteraction.Ignore);
        if (hitsCount > 0)
        {
            //for (int i = 0; i < hitsCount; i++)
            //{
            //    Debug.Log(i+" "+hits[i].collider);
            //}
            Debug.DrawLine(transform.position,hits[0].point,Color.red);
            ref var hit = ref hits[Mathf.Min(hitsCount, 1)];
            flyDestination = hit.point;
            SurfaceSystem.instance.OnSurfaceHit(hit,defaultImpactEffect);
            hit.transform.GetComponentInParent<HealthComponent>()?.OnHit(in hit,this);
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