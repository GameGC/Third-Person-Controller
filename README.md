# Third-Person-Controller VERSION:alpha 0.1
DO NOT USE IN COMERCIAL PROJECTS without permisions

![](https://img.shields.io/badge/Rigidbody-8775FF)
![](https://img.shields.io/badge/StateMachine-8775FF)
![](https://img.shields.io/badge/Custom%20DI-8775FF)

Third-Person-Controller is rigidbody character controller with statemachine based logic and custom Dependency Injection
<br/>

|                                       | Supported state machines|
|---------------------------------------|------------------      |
|Code State Machine                     |:heavy_check_mark:      |
|Visual scripting state machine         |:x: <b>[IN PROGRESS]</b>|
<br/>

|                        | Current States |
|------------------------|------------------
|Idle                    |:heavy_check_mark:
|Walk                    |:heavy_check_mark:
|Run                     |:heavy_check_mark:
|Sprint                  |:heavy_check_mark:
|Jump                    |:heavy_check_mark:
|Crouch                  |:heavy_check_mark:
|Prone                   |:heavy_check_mark:
|In Air                  |:heavy_check_mark:

###### Example: Creating custom Feature for state

```csharp
using StateMachineLogic.DI;
using ThirdPersonController.Core;
using UnityEngine;

public class ExampleFeature : BaseFeature
{
    [SerializeField] private float gravityValue = -10;
    
    private Rigidbody _rigidbody;
    
    public override void CacheReferences(IStateMachineVariables variables, IReferenceResolver resolver)
    {
      _rigidbody = resolver.GetComponent<Rigidbody>();
    }

    public override void OnEnterState() => Debug.Log("OnEnterState");

    public override void OnUpdateState() => Debug.Log("OnUpdateState");

    public override void OnFixedUpdateState() => _rigidbody.AddForce(Vector3.up * gravityValue, ForceMode.VelocityChange);

    public override void OnExitState() => Debug.Log("OnExitState");
}

```

###### Example: Creating custom Transition

```csharp
using System;
using StateMachineLogic.DI;
using ThirdPersonController.Core.CodeStateMachine;

public class GroundedStateTransition : BaseStateTransition
{
    public bool shouldBeGrounded;
    
    private IMoveStateMachineVariables _variables;
    public override void Initialise(IStateMachineVariables variables,IReferenceResolver resolver)
    {
        _variables = variables as IMoveStateMachineVariables;
    }

    public override bool couldHaveTransition => _variables.IsGrounded == shouldBeGrounded;
}
```
<br/> 


## Screenshots

<p align="center">	
<img src="Docs/overviewWindow.png" width="35%" height="35%">
<img src="Docs/characterCreator.png" width="35%" height="35%">
</p>
<img src="Docs/overviewWindow2.png" height="50%">

<br/> 
Known issues:
<br/>
Bad arhitecture when input use IMoveStateMachineVariables inspector reference
<br/>
FootStep Feature lagging and code refactor
<br/>
Input processors code needs to pe improved