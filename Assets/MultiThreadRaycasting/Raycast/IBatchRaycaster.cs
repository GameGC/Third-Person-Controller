using UnityEngine;

public interface IBatchRaycaster
{

}
public interface IBatchRaycasterMultiple : IBatchRaycaster
{
    public int totalPossibleRaycasts { get; }
    public void OnRaycastResult(RaycastHit[] hits);
}

public interface IBatchRaycasterSingle : IBatchRaycaster
{
    public void OnRaycastResult(RaycastHit hit);
}