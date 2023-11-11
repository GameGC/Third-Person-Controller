using System;
using UnityEngine;

public class HealthStateMachineVariables : MonoBehaviour,IHealthIStateMachineVariables
{
    [SerializeField] private float _health = 100;

    public float health
    {
        get => _health;
        set
        {
            OnHealthChanged?.Invoke();
            _health = value;
        }
    }

    public event Action OnHealthChanged;
}