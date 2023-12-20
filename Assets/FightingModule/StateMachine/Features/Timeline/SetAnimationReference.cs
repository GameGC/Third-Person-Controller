using UnityEngine;

public class SetAnimationReference : SetReferenceDefault
{
    public override void OnEnterState()
    {
        if(value == null)
            value = _variables.weaponInstance.GetComponent<Animation>();
        base.OnEnterState();
    }
}