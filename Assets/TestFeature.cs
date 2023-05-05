using System;
using ThirdPersonController.Core.DI;
using UnityEngine;

[Serializable]
public class TestFeature : BaseMultiParamFeature<TestFeatureVariable>
{
    public override void CacheReferences(IStateMachineVariables variables, IReferenceResolver resolver)
    {
        
    }

    public float otherProp;
    public float otherProp1;

}

public class TestFeatureVariable : ScriptableObject
{
    public string test0 = "";
    public int test1 = 0;
    public bool test2 = false;
}