using System;
using System.Collections;
using System.Collections.Generic;
using ThirdPersonController.Core.StateMachine;
using UnityEngine;

public class StateMachineLogger : MonoBehaviour
{
   private CodeStateMachine _stateMachine;

   private void Awake()
   {
      _stateMachine = GetComponent<CodeStateMachine>();
   }

   private void OnEnable()
   {
      _stateMachine.onStateChanged.AddListener(OnStateChanged);
   }
   
   private void OnDisable()
   {
      _stateMachine.onStateChanged.RemoveListener(OnStateChanged);
   }

   private void OnStateChanged()
   {
      Debug.Log(_stateMachine.CurrentState.Name);
   }
}