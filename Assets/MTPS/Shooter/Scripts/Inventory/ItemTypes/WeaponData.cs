using MTPS.Core.CodeStateMachine;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.Serialization;

namespace UTPS.Inventory.ItemTypes
{

    [CreateAssetMenu(menuName = "Create WeaponData", fileName = "WeaponData")]
    public class WeaponData : BaseItemData
    {
        [Space(9)] public CodeStateMachine stateMachine;
        [FormerlySerializedAs("rig")] public Rig rigLayer;

        public AmmoData ammoItem;

        [field: SerializeField] public override int MaxItemCount { get; protected set; } = 1;
    }
}