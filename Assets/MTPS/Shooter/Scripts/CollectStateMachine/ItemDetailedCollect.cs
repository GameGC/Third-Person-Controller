using System.Threading.Tasks;
using UnityEngine;
using MTPS.Inventory;
using MTPS.Inventory.ItemTypes;

public class ItemDetailedCollect : ItemCollect
{
    public Vector3 collectOffset;
    
    public Transform leftTarget;
    public Transform rightTarget;

    public Transform characterPoint;
    public override async void Collect(Inventory receiver)
    {
        var collectVariables = receiver.GetComponentInChildren<ICollectStateMachineVariables>();
        
        if(collectVariables.IsCollecting) return;
        
        // Only try to swap to Hands if a fighting state machine is present and armed
        if (receiver.FightingStateMachine != null &&
            receiver.EquippedItemData != null &&
            receiver.EquippedItemData.name != "Hands")
        {
            var hands = receiver.removeExceptions.Find(i => i.name == "Hands") as WeaponData;
            if (hands != null)
            {
                await receiver.Equip(hands);
                await Task.Delay(3000);
            }
        }

        if(collectVariables.IsCollecting) return;
        
        var handler = receiver.GetComponent<GrenadePickupHandler>();
        if (handler != null)
        {
            handler.Collect(this);
            return;
        }
        collectVariables.OnItemCollect
            .Invoke(collectOffset,this,characterPoint,leftTarget,rightTarget);
        //base.Collect(receiver);
    }


    public void AddItemToInventory(Inventory receiver)
    {
        base.Collect(receiver);
    }
}