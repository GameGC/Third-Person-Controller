using MTPS.Core.CodeStateMachine;
using UnityEngine;

namespace MTPS.Inventory.ItemTypes
{

    [CreateAssetMenu(menuName = "Create WeaponData", fileName = "WeaponData")]
    public class WeaponData : BaseItemData
    {
        [Space(9)] public CodeStateMachine stateMachine;

        public AmmoData ammoItem;

        [field: SerializeField] public override int MaxItemCount { get; protected set; } = 1;
    }
}