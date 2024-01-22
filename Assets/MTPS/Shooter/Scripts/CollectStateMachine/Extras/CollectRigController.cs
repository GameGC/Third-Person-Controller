using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Animations.Rigging;

public class CollectRigController : MonoBehaviour
{
    public ChainIKConstraint leftConstraint;
    public ParentConstraint leftTarget;
    
    public ChainIKConstraint rightConstraint;
    public ParentConstraint rightTarget;

    public void SetTargets(Transform leftHand, Transform rightHand)
    {
        if (leftHand)
        {
            var source = new ConstraintSource
            {
                sourceTransform = leftHand,
                weight = 1
            };
            if (leftTarget.sourceCount < 1)
                leftTarget.AddSource(source);
            else 
                leftTarget.SetSource(0,source);
            leftConstraint.weight = 1;
        }
        else
            leftConstraint.weight = 0;

        if (rightHand)
        {
            var source = new ConstraintSource
            {
                sourceTransform = rightHand,
                weight = 1
            };
            if (rightTarget.sourceCount < 1)
                rightTarget.AddSource(source);
            else 
                rightTarget.SetSource(0,source);
            rightConstraint.weight = 1;
        }
        else
            rightConstraint.weight = 0;
            
    }
}
