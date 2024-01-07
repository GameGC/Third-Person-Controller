using System;
using System.Collections.Generic;
using System.Linq;
using ThirdPersonController.Code.AnimatedStateMachine;
using UnityEngine;

public class PlayerHUD : MonoBehaviour
{
    [SerializeField] private Inventory dataSource;
    
    [SerializeField] private GameObject scope;
    [SerializeField] private GameObject fullScreenScope;
    
    [SerializeField] private WeaponItemDisplay weaponDisplayPrefab;
    [SerializeField] private Transform weaponDisplayParent;
    
    private WeaponItemDisplay[] _itemDisplays;
    private WeaponData[] _displayedWeapons;

    private IFightingStateMachineVariables _variables;
    private IWeaponInfo _weaponInfo;
    private void Awake()
    {
        if(scope)
            scope.SetActive(false);
        if(fullScreenScope)
            fullScreenScope.SetActive(false);
        if(dataSource)
            dataSource.onItemEquiped.AddListener(OnItemEquiped);
    }

    public void AddInventory(Inventory inventory)
    {
        dataSource = inventory;
        dataSource.onItemEquiped.AddListener(OnItemEquiped);
        Start();
    }

    private void OnItemEquiped(BaseItemData arg0)
    {
        _variables = dataSource.FightingStateMachine.GetComponent<IFightingStateMachineVariables>();
        SetSelection(Array.IndexOf(_displayedWeapons, arg0));
    }

    private void Start()
    {
        if(!dataSource) return;
        _displayedWeapons = dataSource.AllItems.Where(i => i.Key is WeaponData)
            .Select(w=>w.Key as WeaponData).ToArray();
        DisplayAllWeapon(_displayedWeapons);
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

    private void DisplayAllWeapon(BaseItemData[] weaponDatas)
    {
        _itemDisplays = new WeaponItemDisplay[weaponDatas.Length];
        for (int i = 0; i < weaponDatas.Length; i++)
        {
            if (weaponDatas[i] is WeaponData)
            {
                _itemDisplays[i] = Instantiate(weaponDisplayPrefab, weaponDisplayParent);
                _itemDisplays[i].Init(weaponDatas[i].icon);
                _itemDisplays[i].SetCounteCounterActive(false);
            }
        }
        
        _itemDisplays[0].SetSelection(true);
    }

    private int prevSelection = 0;
    private void SetSelection(int index)
    {
        _itemDisplays[index].SetSelection(true);
        _itemDisplays[prevSelection].SetSelection(false);
        _itemDisplays[prevSelection].SetCounteCounterActive(false);
        _weaponInfo = null;
        prevSelection = index;
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

    private void OnDestroy()
    {
        dataSource.onItemEquiped.RemoveListener(OnItemEquiped);
    }
}
