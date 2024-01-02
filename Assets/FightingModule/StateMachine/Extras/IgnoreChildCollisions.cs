using System.Collections.Generic;
using UnityEngine;

public class IgnoreChildCollisions: MonoBehaviour
{
    public Collider parentCollider;
    
    private int prevChildCount;

    private List<Collider> _colliders = new List<Collider>();

    private Transform _transform;
    private void Awake()
    {
        _colliders.Add(parentCollider);
        _transform = transform;
    }

    private void FixedUpdate()
    {
        if(prevChildCount == _transform.hierarchyCount) return;
        int length = _colliders.Count;
        if (prevChildCount > _transform.hierarchyCount)
        {
            for (var i = 0; i < length; i++)
            {
                if (_colliders[i] && !_colliders[i].transform.IsChildOf(_transform))
                    for (int j = length - 1; j >= 0; j--)
                    {
                        if(i == j) continue;
                        if(_colliders[j])
                            Physics.IgnoreCollision(_colliders[i], _colliders[j], false);
                    }
            }
        }
        GetComponentsInChildren(_colliders);
        _colliders.Add(parentCollider);
        length = _colliders.Count;

        for (int i = 0; i < length; i++)
        {
            for (var j = length - 1; j >= 0; j--)
            {
                if(i == j) continue;
                Physics.IgnoreCollision(_colliders[i],_colliders[j],true);
            }
        }

        prevChildCount = _transform.hierarchyCount;
    }
}