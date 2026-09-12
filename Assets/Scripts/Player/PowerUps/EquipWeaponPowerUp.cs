using UnityEngine;

/// <summary>
/// "Picking this up puts weapon T in the player's hand, and takes out whatever was there."
///
/// One class for every weapon in the game. It replaced four near-identical power-up
/// classes, which were copies of the same six lines differing only in a type name - so a
/// fix to one of them (the missing searchInactive flag, for instance) reached one weapon
/// and silently missed the other three.
///
/// A new weapon needs NO power-up class at all: its pickable names the type and this class
/// does the rest (Open/Closed).
///
/// It hands the weapon to the player's IWeaponSlot instead of calling Equip() on it
/// directly. That single redirection is what makes the swap work: the slot is the one
/// place that knows a hand can hold only one thing, so this class never has to remember to
/// take the previous weapon away - and cannot forget (Single Responsibility).
///
/// The constraint is IUseableWeapon, not a concrete weapon: this never learns what a
/// boomerang is, only that whatever it found can be carried (Dependency Inversion).
/// </summary>
/// <typeparam name="TWeapon">The weapon component to put in the player's hand.</typeparam>
public class EquipWeaponPowerUp<TWeapon> : IPowerUp where TWeapon : Component, IUseableWeapon
{
    public void ApplyPowerUp(GameObject player)
    {
        if (player == null)
            return;

        // Searches inactive children too, so a weapon may sit on the player switched off.
        TWeapon weapon = player.GetComponentInChildren<TWeapon>(true);

        if (weapon == null)
        {
            Debug.LogWarning("[PowerUp] No " + typeof(TWeapon).Name + " under " + player.name);
            return;
        }

        IWeaponSlot slot = player.GetComponentInChildren<IWeaponSlot>(true);

        if (slot == null)
        {
            // Deliberately NOT falling back to weapon.Equip(). A player with no slot has
            // nothing that pulls a trigger, so the fallback would equip a weapon that can
            // never fire while quietly bringing back the two-weapons-at-once bug the slot
            // exists to prevent. Better to say so.
            Debug.LogError("[PowerUp] " + player.name + " has no IWeaponSlot - add the " +
                           "WeaponsHandler component. Weapon not given.", player);
            return;
        }

        slot.Equip(weapon);
    }
}
