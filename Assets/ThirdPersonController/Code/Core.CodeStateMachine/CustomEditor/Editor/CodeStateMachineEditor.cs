#if UNITY_2022_1_OR_NEWER
using UnityEditor;

namespace ThirdPersonController.Core.CodeStateMachine
{
    [UnityEditor.CustomEditor(typeof(StateMachine.CodeStateMachine),true)]
    public class CodeStateMachineEditor : Editor { }
}
#endif