using UnityEngine;

[CreateAssetMenu(menuName = "Create AmmoData", fileName = "AmmoData", order = 0)]
public class AmmoData : BaseItemData
{
    [Space]
    public GameObject bulletPrefab;
    [field:SerializeField]
    public override int MaxItemCount { get; protected set; }  = int.MaxValue;
}