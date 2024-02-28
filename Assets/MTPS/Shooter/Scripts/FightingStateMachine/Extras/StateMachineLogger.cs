using MTPS.Core.CodeStateMachine;
using UnityEngine;

namespace MTPS.FightingStateMachine.Extras
{
   public class StateMachineLogger : MonoBehaviour
   {
      private CodeStateMachine _stateMachine;

      private void Awake() => _stateMachine = GetComponent<CodeStateMachine>();

      private void OnEnable() => _stateMachine.onStateChanged.AddListener(OnStateChanged);

      private void OnDisable() => _stateMachine.onStateChanged.RemoveListener(OnStateChanged);

      private void OnStateChanged() => Debug.Log(_stateMachine.CurrentState.Name);

      private void OnValidate() => hideFlags = HideFlags.DontSaveInBuild;
   }
}