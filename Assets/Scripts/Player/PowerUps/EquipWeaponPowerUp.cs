using UnityEngine;

/// <summary>
/// ROLE: Effect: put weapon T into the player's hand.
/// PATTERNS: Template Method - fills PutIn of SlotPowerUp; Generics - one class for every weapon.
///
/// "Picking this up puts weapon T in the player's hand, and takes out whatever was there."
///
/// One class for every weapon in the game. It replaced four near-identical power-up
/// classes, which were copies of the same six lines differing only in a type name - so a
/// fix to one of them (the missing searchInactive flag, for instance) reached one weapon
/// and silently missed the other three.
///
/// A new weapon needs NO power-up class at all: its pickable names the type and this class
/// does the rest (Open/Closed). Finding the weapon and the slot is SlotPowerUp's job - the
/// same one the animals use; this class only says "the weapon goes into IWeaponSlot.Equip".
///
/// The constraint is IUseableWeapon, not a concrete weapon: this never learns what a
/// boomerang is, only that whatever it found can be carried (Dependency Inversion).
/// </summary>
/// <typeparam name="TWeapon">The weapon component to put in the player's hand.</typeparam>
public class EquipWeaponPowerUp<TWeapon> : SlotPowerUp<IWeaponSlot, TWeapon>
    where TWeapon : Component, IUseableWeapon
{
    protected override void PutIn(IWeaponSlot slot, TWeapon weapon)
    {
        slot.Equip(weapon);
    }
}
