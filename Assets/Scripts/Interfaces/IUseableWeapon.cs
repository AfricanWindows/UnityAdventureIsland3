using UnityEngine;

public interface IUseableWeapon : IWeapon
{
    void Equip();
    void UnEquip();
}
