using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace GoSystem
{
    public class MovingSphere : MonoBehaviour
    {
        [SerializeField] private float targetAngle = 90;
        [SerializeField] private float speed = 4;

        private float _currentAngle;
        private bool _invertDirection;

        private new Transform transform;
        private Vector3 _forward;
        private void Awake()
        {
            transform = base.transform;
            _invertDirection = targetAngle < 0;
            _forward = transform.rotation * Vector3.forward;
        }

        private void Update()
        {
            _currentAngle = Mathf.MoveTowards(_currentAngle, targetAngle, speed * Time.deltaTime);
            transform.rotation = Quaternion.AngleAxis(_currentAngle,_forward);
            
            if (_invertDirection && _currentAngle < targetAngle + 1)
            {
                targetAngle = -targetAngle;
                _invertDirection = false;
            }
            else if (!_invertDirection && _currentAngle > targetAngle - 1)
            {
                targetAngle = -targetAngle;
                _invertDirection = true;
            }
        }

        private void OnTriggerEnter(Collider hitCollider)
        {
            Debug.Log(hitCollider);
        }
    }
}