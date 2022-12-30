using UnityEngine;

public class DefaultBullet : MonoBehaviour
{
    Transform transform;
    Vector3 flyDestination;
    
    public int speed = 100;
    public int distance = 1000;
    
    
    // Start is called before the first frame update
    void Start()
    {
        transform = base.transform;
        if (Physics.Raycast(transform.position, transform.forward,out var hit,distance))
        {
            flyDestination = hit.point;
        }
        else
        {
            flyDestination = transform.position + transform.forward * distance;
        }
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, flyDestination, Time.deltaTime * speed);
        if(Vector3.Distance(transform.position,flyDestination)<0.1f)
            Destroy(gameObject);
    }
}
//public class BulletEventArgs : Eve