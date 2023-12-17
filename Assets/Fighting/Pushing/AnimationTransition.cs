using System;
using UnityEngine;

[Serializable]
public struct AnimationTransition
{
    public int stateFromIndex;
    public int stateToIndex;
    
  
    
    [Min(0)]
    public float time;

    public bool CouldBeApplyable(string prevState,string newState)
    {
        return string.Equals(stateFrom, prevState, StringComparison.InvariantCultureIgnoreCase)
            &&  string.Equals(stateTo, newState, StringComparison.InvariantCultureIgnoreCase);
    }
    
    public bool CouldBeApplyable(int prevState,int newState)
    {
        return stateFromIndex == prevState && stateToIndex == newState;
    }
    
    public string stateFrom;
    public string stateTo;
    
    internal bool Validate(in string[] keys)
    {
        return ValidateStateFrom(keys) || ValidateStateTo(keys);
    }
    private bool ValidateStateFrom(in string[] keys)
    {
        int nexIndex = Array.IndexOf(keys, stateFrom);
        if (nexIndex < 0 && stateFromIndex>-1)
        {
            stateFrom = keys[stateFromIndex];
            return true;
        }

        if (stateFromIndex < 0) 
            Debug.LogError($"Null StateFrom {stateFrom} {stateTo} {stateFromIndex} {stateTo}");

        if (nexIndex == stateFromIndex) return false;
        stateFromIndex = nexIndex;
        return true;
    }
    
    private bool ValidateStateTo(in string[] keys)
    {
        int nexIndex = Array.IndexOf(keys, stateTo);
        if (nexIndex < 0 && stateToIndex>-1)
        {
            stateTo = keys[stateToIndex];
            return true;
        }

        if (stateToIndex < 0) 
            Debug.LogError($"Null StateTo {stateFrom} {stateTo} {stateFromIndex} {stateTo}");

        if (nexIndex == stateToIndex) return false;
        stateToIndex = nexIndex;
        return true;
    }
}