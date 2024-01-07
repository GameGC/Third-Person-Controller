using UnityEngine;
using UTPS.Inventory.ItemTypes;

namespace UTPS.Inventory
{
    public abstract class ItemCollect : MonoBehaviour
    {
        public BaseItemData WeaponData;

        private void OnCollisionEnter(Collision collision)
        {
            var inventory = collision.transform.root.GetComponent<Inventory>();
            if (inventory)
                Collect(inventory);
        }

        public virtual void Collect(Inventory receiver)
        {
            receiver.AddItem(WeaponData);
            Destroy(gameObject);
        }
    }
}