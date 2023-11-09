using System.Collections;
using System.Collections.Generic;
using ThirdPersonController.Code.AnimatedStateMachine;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class AltRigContoller : MonoBehaviour
{
    private FightingStateMachine _fightingStateMachine;

    public MultiParentConstraint BlendConstraint;
    public MultiAimConstraint[] AimConstraints;

    public TwoBoneIKConstraint leftHand;
    public TwoBoneIKConstraint rightHand;

    
    // Start is called before the first frame update
    private void Awake()
    {
        _fightingStateMachine = FindObjectOfType<FightingStateMachine>();
        _fightingStateMachine.onStateChanged.AddListener(ChangeWeights);
        GetComponent<Rig>().weight = 1;
        ChangeWeights();
    }

    private void ChangeWeights()
    {
        var objects = BlendConstraint.data.sourceObjects;

        leftHand.weight = 1;
        if (_fightingStateMachine?.CurrentState?.Name is "Aim" or "Shoot")
        {
            foreach (var multiAimConstraint in AimConstraints)
            {
                multiAimConstraint.weight = 1;
            }

            objects.SetWeight(0,0);
            objects.SetWeight(1, 1);
            rightHand.weight = 1;
        }
        else
        {
            foreach (var multiAimConstraint in AimConstraints)
            {
                multiAimConstraint.weight = 0;
            }
            
            objects.SetWeight(0,1);
            objects.SetWeight(1,0);
            rightHand.weight = 0;
        }

        BlendConstraint.data.sourceObjects = objects;
        GetComponent<Rig>().weight = 1;
    }
}
