using System;
using MTPS.Core;

[Serializable]
public class TestFeature : BaseMultiParamFeature<TestFeatureVariable>
{
    public override void CacheReferences(IStateMachineVariables variables, IReferenceResolver resolver)
    {
    }

    public float otherProp;
    public float otherProp1;

}