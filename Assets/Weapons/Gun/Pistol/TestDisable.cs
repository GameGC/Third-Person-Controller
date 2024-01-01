using UnityEngine;

public class TestDisable : MonoBehaviour
{
  private void Awake()
  {
    var rb = GetComponentInChildren<Collider>();
    Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Character"),LayerMask.NameToLayer("Effects"));
  }
}