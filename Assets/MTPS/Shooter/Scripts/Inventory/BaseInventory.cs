using System.Collections.Generic;
using GameGC.Collections;
using MTPS.Inventory.ItemTypes;
using UnityEngine;
using UnityEngine.Events;

namespace MTPS.Inventory
{
    public abstract class BaseInventory : MonoBehaviour
    {
        public SKeyValueList<BaseItemData, int> AllItems => items;

        [SerializeField] protected SKeyValueList<BaseItemData, int> items = new SKeyValueList<BaseItemData, int>();

        public List<BaseItemData> removeExceptions;

        public UnityEvent<BaseItemData, int> onItemAdded;
        public UnityEvent<BaseItemData, int> onItemMinus;
        public UnityEvent<BaseItemData> onItemRemoved;

        public virtual bool AddItem(BaseItemData itemData, int count = 1)
        {
            if (items.TryGetValue(itemData, out int prevCount) && itemData is WeaponData)
                return false;
            if (itemData.MaxItemCount > prevCount + count)
            {
                items[itemData] += count;
                onItemAdded.Invoke(itemData, count);
                return true;
            }

            return false;
        }

        public bool MinusItem(BaseItemData itemData, int count = 1)
        {
            if (itemData is WeaponData) return false;

            if (items[itemData] - count > -1)
            {
                items[itemData] -= count;

                if (items[itemData] < 1)
                {
                    RemoveItem(itemData);
                    return false;
                }

                onItemMinus.Invoke(itemData, count);
                return true;
            }

            return false;
        }

        public bool RemoveItem(BaseItemData itemData)
        {
            if (removeExceptions.Contains(itemData))
                return false;

            onItemRemoved.Invoke(itemData);
            return items.Remove(itemData);
        }

#if UNITY_EDITOR
        public SKeyValueList<BaseItemData, int> EDItorREF => items;
#endif

    }
}