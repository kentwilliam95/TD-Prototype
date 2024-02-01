using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponHandler : MonoBehaviour
{
    public enum WeaponType
    {
        Melee,
        Range,
    }
    
    [SerializeField] private GameObject _melee;
    [SerializeField] private GameObject _range;

    public void Equip(WeaponType weaponType)
    {
        _melee.gameObject.SetActive(weaponType == WeaponType.Melee);
        _range.gameObject.SetActive(weaponType == WeaponType.Range);
    }

    public void UnEquip(WeaponType weaponType)
    {
        switch (weaponType)
        {
            case WeaponType.Melee:
                _melee.gameObject.SetActive(false);
                break;
            
            case WeaponType.Range:
                _range.gameObject.SetActive(false);
                break;
        }
    }
}
