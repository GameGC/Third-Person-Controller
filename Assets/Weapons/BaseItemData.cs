using UnityEngine;

public abstract class BaseItemData : ScriptableObject
{
    public new string name;
    public Sprite icon;
   
    public abstract int maxAmmoInInventory { get; protected set; }
    private void OnValidate() => name = base.name;
}