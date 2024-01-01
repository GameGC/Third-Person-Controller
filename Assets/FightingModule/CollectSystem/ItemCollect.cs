using System;
using UnityEngine;

public abstract class ItemCollect : MonoBehaviour
{
    public BaseItemData WeaponData;

    private void OnCollisionEnter(Collision collision)
    {
        Collect(collision.transform.root.GetComponent<Inventory>());
    }

    public virtual void Collect(Inventory receiver)
    {
        receiver.AddItem(WeaponData);
        Destroy(gameObject);
    }
}