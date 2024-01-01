using UnityEngine;

public class ItemDetailedCollect : ItemCollect
{
    public Transform leftTarget;
    public Transform rightTarget;

    public Transform characterPoint;
    public override void Collect(Inventory receiver)
    {
        receiver.GetComponentInChildren<ICollectStateMachineVariables>().OnItemCollect.Invoke(this,characterPoint,leftTarget,rightTarget);
        //base.Collect(receiver);
    }


    public void AddItemToInventory(Inventory receiver)
    {
        base.Collect(receiver);
    }
}