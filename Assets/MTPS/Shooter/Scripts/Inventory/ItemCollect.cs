using GameGC.Collections;
using UnityEngine;
using UTPS.Inventory.ItemTypes;

namespace UTPS.Inventory
{
    public class ItemCollect : MonoBehaviour
    {
        [SerializeField] private SKeyValuePair<BaseItemData,int>[] items;

        private void OnCollisionEnter(Collision collision)
        {
            var inventory = collision.transform.root.GetComponent<Inventory>();
            if (inventory)
                Collect(inventory);
        }

        public virtual void Collect(Inventory receiver)
        {
            foreach (var item in items) 
                receiver.AddItem(item.Key, item.Value);
            Destroy(gameObject);
        }
    }
}