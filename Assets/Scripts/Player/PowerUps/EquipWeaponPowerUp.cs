using UnityEngine;

/// <summary>
/// "Picking this up unlocks weapon T, and nothing else."
///
/// One class for every weapon in the game. It replaced four near-identical power-up
/// classes, which were copies of the same six lines
/// differing only in a type name - so a fix to one of them (the missing searchInactive
/// flag, for instance) reached one weapon and silently missed the other three.
///
/// A new weapon now needs NO power-up class at all: its pickable names the type and this
/// class does the rest (Open/Closed).
///
/// The constraint is IUseableWeapon, not a concrete weapon: this never learns what a
/// laser is, only that whatever it found can be equipped (Dependency Inversion).
/// </summary>
/// <typeparam name="TWeapon">The weapon component to unlock on the player.</typeparam>
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

        weapon.Equip();
        Debug.Log("[PowerUp] Picked up - " + typeof(TWeapon).Name + " unlocked");
    }
}
