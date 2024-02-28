using UnityEngine;

namespace MTPS.Inventory.ItemTypes
{
    public abstract class BaseItemData : ScriptableObject
    {
        public new string name;
        public Sprite icon;

        public abstract int MaxItemCount { get; protected set; }
        private void OnValidate() => name = base.name;
    }
}