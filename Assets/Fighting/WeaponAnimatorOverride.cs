using UnityEngine;

public class WeaponAnimatorOverride : MonoBehaviour
{
    public AnimatorOverrideController Controller;
   
    private void Awake()
    {
        GetComponentInParent<HybridAnimator>().MecanimOverride = Controller;
    }
}