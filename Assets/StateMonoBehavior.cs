using System;
using GameGC.Collections;
using UnityEngine;

public abstract class StateMonoBehavior<T> : MonoBehaviour where T : ScriptableObject
{
    [SerializeField] private SDictionary<string, T> states;

    protected T State { get;private set; }

    public void SetState(string s)
    {
        if (states.TryGetValue(s, out var newState)) 
            State = newState;
    }

    private void Reset()
    {
        states.Add("Default",ScriptableObject.CreateInstance<T>());
    }
}