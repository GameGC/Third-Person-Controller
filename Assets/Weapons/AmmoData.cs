using UnityEngine;

[CreateAssetMenu(menuName = "Create AmmoData", fileName = "AmmoData", order = 0)]
public class AmmoData : BaseItemData
{
    public GameObject bulletPrefab;
    [field:SerializeField]
    public override int maxAmmoInInventory { get; protected set; }  = int.MaxValue;
}