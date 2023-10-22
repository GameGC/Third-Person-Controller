using System.Threading.Tasks;
using UnityEngine;

public class GrenadeBullet : MonoBehaviour
{
    Transform transform;
    Vector3 flyDestination;
    
    
    private Vector3[] _points;
    private int _pointIndex = 1;
    private float _distance;
    private float _size;

    
    public int speed = 100;
    public float explodeTimeSeconds = 3;
    public GameObject effectPrefab;
    
    public void Init(Vector3[] points)
    {
        this._points = points;
        var ext = GetComponentInChildren<SphereCollider>().radius;
        _size = ext;
    }
    
    // Start is called before the first frame update
    private void Start()
    {
        transform = base.transform;
    }

    public void Cast()
    {
        flyDestination = _points[^1];
        _distance = Vector3.Distance(_points[_pointIndex - 1],_points[_pointIndex]) + _size;
        if (Physics.Raycast(transform.position, (_points[_pointIndex] - transform.position).normalized,out var hit,_distance))
        {
            flyDestination = hit.point;
            return;
        }

        if (Vector3.Distance(transform.position,_points[_pointIndex]) < _size)
            if(_points.Length-1>_pointIndex)
                _pointIndex++;
    }
    // Update is called once per frame
    private void Update()
    {
        if(stopUpdate) return;
        Cast();
        transform.position = Vector3.MoveTowards(transform.position, _points[_pointIndex], Time.deltaTime * speed);
        if (Vector3.Distance(transform.position, flyDestination) < 0.1f)
        {
            stopUpdate = true;
            OnTouchGround();
        }
    }

    private bool stopUpdate = false;

    private async void OnTouchGround()
    {
        GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
        await Task.Delay((int)(explodeTimeSeconds*1000));
        Destroy(Instantiate(effectPrefab, transform.position, transform.rotation),1);
        Destroy(gameObject);
    }
}