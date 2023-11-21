using System;
using ThirdPersonController.Code.AnimatedStateMachine;
using UnityEditor;
using UnityEngine;

public class PlayerHUD : MonoBehaviour
{
    public GameObject scope;
    public GameObject fullScreenScope;
    
    public WeaponItemDisplay weaponDisplayPrefab;
    public Transform weaponDisplayParent;
    private WeaponItemDisplay[] _itemDisplays;

    private IFightingStateMachineVariables _variables;
    private IWeaponInfo _weaponInfo;
    private void Awake()
    {
        if(scope)
            scope.SetActive(false);
        if(fullScreenScope)
            fullScreenScope.SetActive(false);
    }

    public void SetScopeActive(bool active)
    {
        scope.SetActive(active);
    }
    
    public void SetFullScreenScopeActive(bool active)
    {
        fullScreenScope.SetActive(active);
        weaponDisplayParent.gameObject.SetActive(!active);
    }

    public void DisplayAllWeapon(Sprite[] icons,IFightingStateMachineVariables variables)
    {
        _itemDisplays = new WeaponItemDisplay[icons.Length];
        _variables = variables;
        for (int i = 0; i < icons.Length; i++)
        {
            _itemDisplays[i] = Instantiate(weaponDisplayPrefab, weaponDisplayParent);
            _itemDisplays[i].Init(icons[i]);
            _itemDisplays[i].SetCounteCounterActive(false);
        }
        
        _itemDisplays[0].SetSelection(true);
    }

    private int prevSelection = 0;
    public void SetSelection(int index)
    {
        _itemDisplays[index].SetSelection(true);
        _itemDisplays[prevSelection].SetSelection(false);
        _itemDisplays[prevSelection].SetCounteCounterActive(false);
        _weaponInfo = null;
        prevSelection = index;
    }
    
    public void SetVariables(IFightingStateMachineVariables variables)
    {
        _variables = variables;
    }

    private void Update()
    {
        if (prevSelection > 0 && _variables != null)
        {
            if (_weaponInfo == null && _variables.weaponInstance)
            {
                _weaponInfo = _variables.weaponInstance.GetComponent<IWeaponInfo>();
                _itemDisplays[prevSelection].SetCounteCounterActive(true);
            }

            if(_weaponInfo != null)
                _itemDisplays[prevSelection].UpdateAmmoInfo(_weaponInfo.remainingAmmo,_weaponInfo.maxAmmo);
        }
    }
}
