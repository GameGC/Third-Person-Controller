using System.Threading.Tasks;
using UnityEngine;

public class ItemDetailedCollect : ItemCollect
{
    public Transform leftTarget;
    public Transform rightTarget;

    public Transform characterPoint;
    public override async void Collect(Inventory receiver)
    {
        var collectVariables = receiver.GetComponentInChildren<ICollectStateMachineVariables>();
        
        if(collectVariables.IsCollecting) return;
        
        if (receiver.EquipedItemData.name != "Hands")
        {
            var hands = receiver.removeExceptions.Find(i => i.name == "Hands") as WeaponData;
            await receiver.Equip(hands);
            await Task.Delay(3000);
        }

        if(collectVariables.IsCollecting) return;
        
        collectVariables.OnItemCollect
            .Invoke(this,characterPoint,leftTarget,rightTarget);
        //base.Collect(receiver);
    }


    public void AddItemToInventory(Inventory receiver)
    {
        base.Collect(receiver);
    }
}