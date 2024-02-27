using System;
using UnityEngine;

public class LateUpdateCaller : MonoBehaviour
{
   public event Action OnLateUpdate;

   private void LateUpdate()
   {
      OnLateUpdate?.Invoke();
   }
}
