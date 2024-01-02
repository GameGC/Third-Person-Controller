using System;
using UnityEngine.Animations.Rigging;

/// <summary>
/// Rig Builder with fixed size
/// </summary>
public class RigBuilderFixed : RigBuilder
{
   private void OnValidate()
   {
      int counts = Enum.GetValues(typeof(RigTypes)).Length;
      if (layers.Count != counts)
      {
         while (layers.Count < counts) 
            layers.Add(new RigLayer(null));

         while (layers.Count > counts) 
            layers.RemoveAt(layers.Count - 1);
      }
   }

   private void Reset() => OnValidate();
}
