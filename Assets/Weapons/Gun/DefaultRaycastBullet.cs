using UnityEngine;

public class DefaultRaycastBullet : MonoBehaviour
{
    private Transform transform;
    private Vector3 flyDestination;
    
    public int speed = 100;
    public int distance = 1000;
    
    
    // Start is called before the first frame update
    private void Start()
    {
        transform = base.transform;
        if (Physics.Raycast(transform.position, transform.forward,out var hit,distance,-5,QueryTriggerInteraction.Ignore))
        {
            flyDestination = hit.point;
            Debug.Log(hit.collider);
            hit.transform.GetComponentInParent<HealthComponent>().OnHit(hit);
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
//public class BulletEventArgs : Eve