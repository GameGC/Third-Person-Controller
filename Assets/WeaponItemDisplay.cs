using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class WeaponItemDisplay : MonoBehaviour
{
    public Image iconImage;
    public Image outline;
    public Text ammoText;

    private static readonly Color32 selectedColor = new Color32(225, 154, 0, 255);
    public void Init(Sprite icon)
    {
        iconImage.sprite = icon;
    }

    public void SetSelection(bool select)
    {
        outline.color = select ?selectedColor : Color.white;
    }
    
    public void SetCounteCounterActive(bool active)
    {
        ammoText.gameObject.SetActive(active);
    }

    public void UpdateAmmoInfo(int reminingAmmo,int maxAmmo)
    {
        ammoText.text = $"{reminingAmmo}/{maxAmmo}";
    }
    
}
