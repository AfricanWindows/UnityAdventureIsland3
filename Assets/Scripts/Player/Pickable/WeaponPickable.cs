using UnityEngine;

/// <summary>
/// ROLE: Generic base of the weapon pickups.
/// PATTERNS: Factory Method - creates an EquipWeaponPowerUp; Generics - the weapon is a type argument.
///
/// A pickup that hands the player a weapon of type TWeapon.
///
/// Generic base so each weapon pickup is one line: AxePickable and BoomerangPickable used to
/// be the same class twice with a different type name. The type is now the generic
/// parameter and the rest is written here once (Open/Closed, Generics, Factory Method).
///
/// It stays abstract because Unity cannot put an open generic MonoBehaviour on a prefab -
/// each weapon still needs its closed class, the same shape as PickableDropper and the pool
/// managers.
/// </summary>
/// <typeparam name="TWeapon">The weapon component this pickup puts in the player's hand.</typeparam>
public abstract class WeaponPickable<TWeapon> : BasePickable
    where TWeapon : Component, IUseableWeapon
{
    protected sealed override IPowerUp CreatePowerUp()
    {
        return new EquipWeaponPowerUp<TWeapon>();
    }
}
