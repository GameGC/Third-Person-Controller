#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine.Animations.Rigging;
#endif

namespace UTPS.FightingStateMachine.Extras
{
   /// <summary>
   /// Rig Builder with fixed size
   /// </summary>
   public class RigBuilderFixed : RigBuilder
   {
#if UNITY_EDITOR
      public void OnValidate()
      {
         int counts = Enum.GetValues(typeof(RigTypes)).Length;
         if (layers.Count != counts)
         {
            while (layers.Count < counts)
               layers.Add(new RigLayer(null));

            while (layers.Count > counts)
               layers.RemoveAt(layers.Count - 1);
            
            EditorUtility.SetDirty(this);
         }
      }

      private void Reset() => OnValidate();
#endif
   }
}